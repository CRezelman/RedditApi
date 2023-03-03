using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedditApi.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace RedditApi.Controllers
{
    [Route("api/Post")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly PostsContext _context;
        private readonly CommentContext _commentContext;
        private readonly RatingsPostContext _ratingsPostContext;
        public PostsController(PostsContext context, CommentContext commentContext, RatingsPostContext ratingsPostContext)
        {
            _context = context;
            _commentContext = commentContext;
            _ratingsPostContext = ratingsPostContext;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts([FromQuery] PostQuery query)
        {
            if (_context.Post == null)
            {
                return NotFound();
            }
            var posts = await _context.Post.ToListAsync();
            Utilities.RedditApi.FetchPostRatings(_ratingsPostContext, posts);

            if (query.IdUser != 0)
            {
               posts = posts.Where(p => p.IdUser == query.IdUser).ToList();
            }

            if (query.IdUserRatings != 0)
            {
                posts = posts.Where(p => p.Ratings.FindAll(r => r.IdUser == query.IdUserRatings).Count > 0).ToList();
            }

            if (query.Ratings != RatingsType.None)
            {
                posts = posts.Where(p => p.Ratings.FindAll(r => r.Rating == query.Ratings).Count > 0).ToList();
            }
            
            Utilities.RedditApi.FetchComments(_commentContext, posts);

            return posts;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(long id)
        {
            if (_context.Post == null)
            {
              return NotFound();
            }
            var post = await _context.Post.FindAsync(id);

            if (post == null)
            {
                return NotFound();
            }
            Utilities.RedditApi.FetchComments(_commentContext, post);

            return post;
        }
        [HttpGet("Rating/{id}")]
        public async Task<ActionResult<RatingsPost>> GetRatings(long id)
        {
            if (_ratingsPostContext.RatingsPosts == null)
            {
                return NotFound();
            }
            var ratings = await _ratingsPostContext.RatingsPosts.FindAsync(id);

            if (ratings == null)
            {
                return NotFound();
            }

            return ratings;
        }
        
        [HttpPut("{id}")]
        [SwaggerResponse(201, "Updated post.")]
        [SwaggerResponse(404, "Post does not exist")]
        [SwaggerResponse(409, "Trying to edit a post that does not belong to you.")]
        public async Task<IActionResult> PutPost(long id, PostNew postPut)
        {
            Post post = new Post(postPut);
            if (id != post.Id)
            {
                return BadRequest("Route ID does not match body ID.");
            }

            var postLookup = await _context.Post.FindAsync(id);
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            
            if (postLookup == null)
            {
                return NotFound("Post does not exist");
            }
            
            if (postLookup.IdUser != Convert.ToInt64(userId.Value))
            {
                return Unauthorized("You are not authorized to edit this post.");
            }
            
            post.IdUser = postLookup.IdUser;
            _context.Entry(postLookup).State = EntityState.Detached;

            _context.Entry(post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Utilities.RedditApi.PostExists(_context, id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }
        
        [HttpPut("{idPost}/Rating/{id}")]
        public async Task<IActionResult> PutRating(long id, long idPost, RatingsPost ratingsPost)
        {
            if (id != ratingsPost.Id)
            {
                return BadRequest("Route ID does not match body ID.");
            }
            
            if (idPost != ratingsPost.IdPost)
            {
                return BadRequest("Route ID Post does not match body ID Post.");
            }

            var ratingLookup = await _ratingsPostContext.RatingsPosts.FindAsync(id);
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            
            if (ratingLookup == null)
            {
                return NotFound("Rating does not exist");
            }
            
            if (ratingLookup.IdUser != Convert.ToInt64(userId.Value))
            {
                return Unauthorized("You are not authorized to edit this rating.");
            }
            
            ratingsPost.IdUser = ratingLookup.IdUser;
            _ratingsPostContext.Entry(ratingLookup).State = EntityState.Detached;

            _ratingsPostContext.Entry(ratingsPost).State = EntityState.Modified;

            try
            {
                await _ratingsPostContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Utilities.RedditApi.RatingPostExists(_ratingsPostContext, id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return CreatedAtAction(nameof(GetRatings), new { id = ratingsPost.Id }, ratingsPost);
        }
        
        [HttpPost]
        [SwaggerResponse(201, "Created new post.")]
        public async Task<ActionResult<Post>> PostPost(PostNew postNew)
        {
            Post post = new Post(postNew);
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            post.IdUser = Convert.ToInt64(userId.Value);

            if (Utilities.RedditApi.PostExists(_context, post.Id))
            {
                return Conflict($"Post with ID {post.Id} exists already, use ID: 0 to seed a new ID.");
            }
            
            _context.Post.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }
        
        [Route("{idPost}/Rating")]
        [HttpPost]
        [SwaggerResponse(201, "Create or Update a post's ratings")]
        public async Task<ActionResult<RatingsPost>> PostRatings(long idPost, RatingsPost ratingsPost)
        {
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            ratingsPost.IdUser = Convert.ToInt64(userId.Value);
            ratingsPost.IdPost = idPost;

            if (!Utilities.RedditApi.PostExists(_context, idPost))
            {
                return Conflict($"Post with ID {idPost} does not exist");
            }
            
            if (Utilities.RedditApi.RatingPostExists(_ratingsPostContext, ratingsPost.IdPost))
            {
                return Conflict($"Post already has a rating");
            }
            
            _ratingsPostContext.Add(ratingsPost);
            await _ratingsPostContext.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetRatings), new { id = ratingsPost.Id }, ratingsPost);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(long id)
        {
            if (_context.Post == null)
            {
                return NotFound();
            }

            var post = await _context.Post.FindAsync(id);
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
    
            if (post == null)
            {
                return NotFound("Post does not exist.");
            }
            
            if (post.IdUser != Convert.ToInt64(userId.Value))
            {
                return Unauthorized("You are not authorized to delete this post.");
            }

            _context.Post.Remove(post);
            await _context.SaveChangesAsync();

            return Ok($"Deleted post with ID {post.Id}.");
        }
    }
}
