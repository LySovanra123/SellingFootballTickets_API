
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SellingFootballTickets_API.Data;
using SellingFootballTickets_API.Models;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            var users = await _context.users.ToListAsync();
            return Ok(users);
        }
        [HttpPost]
        public async Task<ActionResult<Users>> CreateUser(Users user)
        {
            _context.users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, Users user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
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

        private bool UserExists(int id)
        {
            throw new NotImplementedException();
        }
    }
}
