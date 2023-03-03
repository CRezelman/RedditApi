using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedditApi.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace RedditApi.Controllers
{
    [Route("api/Comment")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly CommentContext _context;
        private readonly PostsContext _postsContext;
        private readonly RatingsCommentsContext _ratingsCommentsContext;
        public CommentController(CommentContext context, PostsContext postsContext, RatingsCommentsContext ratingsCommentsContext)
        {
            _context = context;
            _postsContext = postsContext;
            _ratingsCommentsContext = ratingsCommentsContext;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments([FromQuery] CommentQuery query)
        {
            if (_context.Comment == null)
            {
                return NotFound();
            }
            
            var comments = await _context.Comment.ToListAsync();

            if (query.IdUser != 0)
            {
                comments = comments.Where(c => c.IdUser == query.IdUser).ToList();
            }

            if (query.IdPost != 0)
            {
                comments = comments.Where(c => c.IdPost == query.IdPost).ToList();
            }

            return comments;
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(long id)
        {
            if (_context.Comment == null)
            {
              return NotFound();
            }
            var comment = await _context.Comment.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            return comment;
        }
        
        [HttpGet("Rating/{id}")]
        public async Task<ActionResult<RatingsComments>> GetRatings(long id)
        {
            if (_ratingsCommentsContext.RatingsComments == null)
            {
                return NotFound();
            }
            var ratings = await _ratingsCommentsContext.RatingsComments.FindAsync(id);

            if (ratings == null)
            {
                return NotFound();
            }

            return ratings;
        }
        
        [HttpPut("{id}")]
        [SwaggerResponse(201, "Updated Comment.")]
        [SwaggerResponse(404, "Comment does not exist")]
        [SwaggerResponse(409, "Trying to edit a comment that does not belong to you.")]
        public async Task<IActionResult> PutComment(long id, CommentNew commentPut)
        {
            Comment comment = new Comment(commentPut);
            if (id != comment.Id)
            {
                return BadRequest("Route ID does not match body ID.");
            }

            var commentLookup = await _context.Comment.FindAsync(id);
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            
            if (commentLookup == null)
            {
                return NotFound("Comment does not exist");
            }
            
            if (commentLookup.IdUser != Convert.ToInt64(userId.Value))
            {
                return Unauthorized("You are not authorized to edit this comment.");
            }
            
            comment.IdUser = commentLookup.IdUser;
            comment.IdPost = commentLookup.IdPost;
            _context.Entry(commentLookup).State = EntityState.Detached;

            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Utilities.RedditApi.CommentExists(_context, id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }
        
        [HttpPut("{idComment}/Rating/{id}")]
        public async Task<IActionResult> PutRating(long id, long idComment, RatingsComments ratingsComments)
        {
            if (id != ratingsComments.Id)
            {
                return BadRequest("Route ID does not match body ID.");
            }
            
            if (idComment != ratingsComments.IdComment)
            {
                return BadRequest("Route ID Comment does not match body ID Comment.");
            }

            var ratingLookup = await _ratingsCommentsContext.RatingsComments.FindAsync(id);
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            
            if (ratingLookup == null)
            {
                return NotFound("Rating does not exist");
            }
            
            if (ratingLookup.IdUser != Convert.ToInt64(userId.Value))
            {
                return Unauthorized("You are not authorized to edit this rating.");
            }
            
            ratingsComments.IdUser = ratingLookup.IdUser;
            _ratingsCommentsContext.Entry(ratingLookup).State = EntityState.Detached;

            _ratingsCommentsContext.Entry(ratingsComments).State = EntityState.Modified;

            try
            {
                await _ratingsCommentsContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Utilities.RedditApi.RatingCommentExists(_ratingsCommentsContext, id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            return CreatedAtAction(nameof(GetRatings), new { id = ratingsComments.Id }, ratingsComments);
        }
        
        [HttpPost]
        [SwaggerResponse(201, "Created new comment.")]
        public async Task<ActionResult<Comment>> PostComment(CommentNew commentNew)
        {
            Comment comment = new Comment(commentNew);
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            comment.IdUser = Convert.ToInt64(userId.Value);

            if (Utilities.RedditApi.CommentExists(_context, comment.Id))
            {
                return Conflict($"Comment with ID {comment.Id} exists already, use ID: 0 to seed a new ID.");
            }

            if (!Utilities.RedditApi.PostExists(_postsContext, comment.IdPost))
            {
                return Conflict($"Post with ID {comment.IdPost} does not exist.");
            }
            
            _context.Comment.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }
        
        [Route("{idComment}/Rating")]
        [HttpPost]
        [SwaggerResponse(201, "Add a rating to a comment.")]
        public async Task<ActionResult<RatingsComments>> PostRatings(long idComment, RatingsComments ratingsComments)
        {
            if (idComment != ratingsComments.IdComment)
            {
                return BadRequest("Route IdComment does not match body IdComment");
            }
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            ratingsComments.IdUser = Convert.ToInt64(userId.Value);
            ratingsComments.IdComment = idComment;

            if (!Utilities.RedditApi.CommentExists(_context, idComment))
            {
                return Conflict($"Comment with ID {idComment} does not exist");
            }
            
            if (Utilities.RedditApi.RatingCommentExists(_ratingsCommentsContext, ratingsComments.IdComment))
            {
                return Conflict($"Comment already has a rating");
            }
            
            _ratingsCommentsContext.Add(ratingsComments);
            await _ratingsCommentsContext.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetRatings), new { id = ratingsComments.Id }, ratingsComments);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(long id)
        {
            if (_context.Comment == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment.FindAsync(id);
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
    
            if (comment == null)
            {
                return NotFound("Comment does not exist.");
            }
            
            if (comment.IdUser != Convert.ToInt64(userId.Value))
            {
                return Unauthorized("You are not authorized to delete this comment.");
            }

            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok($"Deleted comment with ID {comment.Id}.");
        }
    }
}
