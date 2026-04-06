
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SellingFootballTickets_API.Data;
using SellingFootballTickets_API.DTO;
using SellingFootballTickets_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SellingFootballTickets_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly ServiceContext _context;

        public UserController(ServiceContext context)
        {
            _context = context;
        }
        //==================================================
        //                 GetUsers
        //==================================================
        [HttpGet]
        [Authorize(Policy = "adminOnly")]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            var users = await _context.users.ToListAsync();
            return Ok(users);
        }

        // =================================================
        //                 SignUpUser
        // =================================================
        [AllowAnonymous]
        [HttpPost("/api/SignUpUser")]
        public async Task<ActionResult<Users>> CreateUser(Users user)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);

        }
        // =================================================
        //                 GetUserByEmail
        // =================================================
        [HttpPost("/api/GetUserByEmail")]
        [Authorize]
        public async Task<ActionResult<Users>> GetUserByEmail([FromBody] RequestSignIn request)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return NotFound(new {message = "User not found" });
            }

            return Ok(user);
        }

        // =================================================
        //                 UpdateUser
        // =================================================
        [Authorize(Policy = "Personal")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, Users user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.Entry(user).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }


        // =================================================
        //                 SignInUser
        // =================================================
        [AllowAnonymous]
        [HttpPost("/api/SignInUser")]
        public async Task<ActionResult<Users>> SignInUser([FromBody] RequestSignIn request)
        {
            var existingUser = await _context.users.FirstOrDefaultAsync(u => u.Email == request.Email && u.Active == true);
            if (existingUser == null || !BCrypt.Net.BCrypt.Verify(request.Password, existingUser.Password))
            {
                return Unauthorized(new { message = "User not Found", statusCode = StatusCodes.Status401Unauthorized });
            }
            var token = GenerateJwtToken(existingUser);
            return Ok(new { Token = token });
        }
        // =================================================
        //                 GenerateJwtToken
        // =================================================
        private string GenerateJwtToken(Users user)
        {
            var Claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a-string-secret-at-least-256-bits-long"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: Claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool UserExists(int id)
        {
            throw new NotImplementedException();
        }

        // =================================================
        //                 enable user login
        // =================================================
        [HttpPost("/api/EnableUserLogin")]
        [Authorize(Policy = "adminOnly")]
        public async Task<IActionResult> EnableUserLogin(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Active = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }


        //==================================================
        //                 disable user login
        //==================================================
        [HttpPost("/api/DisableUserLogin")]
        [Authorize(Policy = "adminOnly")]
        public async Task<IActionResult> DisableUserLogin(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Active = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //==================================================
        //                search user by name
        //==================================================
        [HttpGet("/api/SearchUserByName")]
        [Authorize(Policy = "adminOnly")]
        public async Task<ActionResult<IEnumerable<Users>>> SearchUserByName(string name)
        {
            var users = await _context.users.Where(u => u.Name.Contains(name)).ToListAsync();
            return Ok(users);
        }
    }
}
