using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedditApi.Models;

namespace RedditApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserContext _context;

        public AuthController(UserContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            if (_context.Users.Where(u => u.Username == request.Username).ToList().Count > 0)
            {
                return BadRequest("Username already exists! Try a different username.");
            }
            User user = new User();
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            user.Username = request.Username;
            user.PasswordHash = passwordHash;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<User>> Login(UserDto request)
        {
            var user = _context.Users.Where(u => u.Username == request.Username).ToList();

            if (user.Count == 0)
            {
                return BadRequest("Username not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.ToList().First().PasswordHash))
            {
                return BadRequest("Password/usernames do not match");
            }

            return Ok(user);
        }
    }
}
