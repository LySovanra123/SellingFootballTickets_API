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

        [NonAction]
        public string GenerateUniqueCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

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
        public async Task<ActionResult<List<Tickets>>> AddTicket(Tickets ticket,int quantity)
        {
            var tickets = new List<Tickets>();

            for (int i=0; i<quantity; i++)   
            {
                tickets.Add(new Tickets
                {
                    Code = GenerateUniqueCode(),
                    Description = ticket.Description,
                    Stadium = ticket.Stadium,
                    Price = ticket.Price,
                    KickOff = ticket.KickOff,
                    DateSale = DateTime.Now,
                    DateExpriseSale = ticket.DateExpriseSale,
                    Block = ticket.Block,
                    Row = ticket.Row,
                    Seat = ticket.Seat + i,
                    IsAvailable = true
                });
            }
            _context.tickets.AddRange(tickets);
            await _context.SaveChangesAsync();

            return Ok(await _context.tickets.ToListAsync());
        }
        
    }
}
