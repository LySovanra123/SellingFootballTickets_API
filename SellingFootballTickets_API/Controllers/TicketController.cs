using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SellingFootballTickets_API.Data;
using SellingFootballTickets_API.Models;

namespace SellingFootballTickets_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ServiceContext _context;

        public TicketController(ServiceContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<List<Tickets>>> Get()
        {
            return Ok(await _context.tickets.ToListAsync());
        }
        [HttpPost]
        public async Task<ActionResult<List<Tickets>>> AddTicket(Tickets ticket)
        {
            _context.tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return Ok(await _context.tickets.ToListAsync());
        }
        
    }
}
