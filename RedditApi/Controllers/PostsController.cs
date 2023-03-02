using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly UserContext _userContext;

        public PostsController(PostsContext context, UserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetPosts([FromQuery] PostQuery query)
        {
            if (_context.Post == null)
            {
                return NotFound();
            }

            if (query.IdUser != 0)
            {
                return await _context.Post.Where(p => p.IdUser == query.IdUser).ToListAsync();
            }

            return await _context.Post.ToListAsync();
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

            return post;
        }
        
        [HttpPut("{id}")]
        [SwaggerResponse(201, "Updated post.")]
        [SwaggerResponse(404, "Post does not exist")]
        [SwaggerResponse(409, "Trying to edit a post that does not belong to you.")]
        public async Task<IActionResult> PutPost(long id, Post post)
        {
            if (id != post.Id)
            {
                return BadRequest();
            }

            var postLookup = await _context.Post.FindAsync(id);
            
            if (postLookup != null && postLookup.IdUser != post.IdUser)
            {
                return Conflict("User ID differs from original post.");
            }
            
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

            if (post.IdUser != Convert.ToInt64(userId.Value))
            {
                return Unauthorized("You are not authorized to edit this post.");
            }
            

            // _context.Entry(post).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id))
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
        
        [HttpPost]
        [SwaggerResponse(201, "Created new post.")]
        public async Task<ActionResult<Post>> PostPost(Post post)
        {
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            post.IdUser = Convert.ToInt64(userId.Value);
            
            _context.Post.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(long id)
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

            _context.Post.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostExists(long id)
        {
            return (_context.Post?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
