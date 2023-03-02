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
using Utilities;

namespace RedditApi.Controllers
{
    [Route("api/Comment")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly CommentContext _context;
        private readonly PostsContext _postsContext;
        public CommentController(CommentContext context, PostsContext postsContext)
        {
            _context = context;
            _postsContext = postsContext;
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments([FromQuery] CommentQuery query)
        {
            if (_context.Comment == null)
            {
                return NotFound();
            }
            
            //Must implement query for idpost!!

            if (query.IdUser != 0)
            {
                return await _context.Comment.Where(c => c.IdUser == query.IdUser).ToListAsync();
            }

            return await _context.Comment.ToListAsync();
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
        
        [HttpPut("{id}")]
        [SwaggerResponse(201, "Updated Comment.")]
        [SwaggerResponse(404, "Comment does not exist")]
        [SwaggerResponse(409, "Trying to edit a comment that does not belong to you.")]
        public async Task<IActionResult> PutComment(long id, Comment comment)
        {
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
                if (!Utilities.Utilities.CommentExists(_context, id))
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
        
        [HttpPost]
        [SwaggerResponse(201, "Created new comment.")]
        public async Task<ActionResult<Comment>> PostComment(Comment comment)
        {
            Claim userId = User.Claims.First(a => a.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            comment.IdUser = Convert.ToInt64(userId.Value);

            if (Utilities.Utilities.CommentExists(_context, comment.Id))
            {
                return Conflict($"Comment with ID {comment.Id} exists already, use ID: 0 to seed a new ID.");
            }

            if (!Utilities.Utilities.PostExists(_postsContext, comment.IdPost))
            {
                return Conflict($"Post with ID {comment.IdPost} does not exist.");
            }
            
            _context.Comment.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
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
