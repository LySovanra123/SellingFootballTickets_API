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
        private readonly TicketContext ticketContext;

        public TicketController(TicketContext ticketContext)
        {
            this.ticketContext = ticketContext;
        }
        [HttpGet]
        public async Task<ActionResult<List<Tickets>>> Get()
        {
            return Ok(await ticketContext.Tickets.ToListAsync());
        }
        [HttpPost]
        public async Task<ActionResult<List<Tickets>>> AddTicket(Tickets ticket)
        {
            ticketContext.Tickets.Add(ticket);
            await ticketContext.SaveChangesAsync();
            return Ok(await ticketContext.Tickets.ToListAsync());
        }
        
    }
}
