
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SellingFootballTickets_API.Data;
using SellingFootballTickets_API.DTO;
using SellingFootballTickets_API.exceptions;
using SellingFootballTickets_API.Models;
using SellingFootballTickets_API.Service;
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
        private readonly IOTPService _otpService;
        // store OTP with timestamp
        private static Dictionary<string, (string Otp, DateTime Expiry)> otpStorage = new();

        public UserController(ServiceContext context, IOTPService otpService)
        {
            _context = context;
            _otpService = otpService;
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

        //==================================================
        //                 RequestOTP
        //==================================================

        [AllowAnonymous]
        [HttpPost("/api/request-otp")]
        public async Task<IActionResult> RequestOTP([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required" });

            string otp = _otpService.GenerateOTP();

            otpStorage[email] = (otp, DateTime.UtcNow.AddMinutes(3));

            await _otpService.SendOTPAsync(email, otp);

            return Ok(new { message = "OTP sent successfully" });
        }


        //==================================================
        //                 VerifyOTP and SignUp
        //==================================================
        [AllowAnonymous]
        [HttpPost("/api/SignUpUser")]
        public async Task<ActionResult<Users>> SignUp([FromBody] Users user, [FromQuery] string otp)
        {
            var existingUser = await _context.users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already exists" });
            }

            if (user == null || string.IsNullOrWhiteSpace(user.Password))
                return BadRequest(new { message = "Invalid user data" });

            if (!otpStorage.ContainsKey(user.Email))
                return BadRequest(new { message = "OTP not found or expired" });

            var stored = otpStorage[user.Email];

            if (stored.Otp != otp)
                return BadRequest(new { message = "Invalid OTP" });

            if (DateTime.UtcNow > stored.Expiry)
            {
                otpStorage.Remove(user.Email);
                return BadRequest(new { message = "OTP has expired" });
            }
            otpStorage.Remove(user.Email);

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.users.Add(user);
            var token = GenerateJwtToken(user);
            await _context.SaveChangesAsync();

            return Ok(new { Token = token });
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
                throw new NotFoundException($"User not found with email {request.Email}");
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
                    throw new NotFoundException($"{nameof(UserExists)}");
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
                throw new NotFoundException($"User not found with id {id}");
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
                throw new NotFoundException($"User not found with id {id}");
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
