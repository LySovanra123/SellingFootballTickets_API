using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SellingFootballTickets_API.Data;
using SellingFootballTickets_API.DTO;
using SellingFootballTickets_API.Models;
using SellingFootballTickets_API.Service;

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
            var tickets = await _context.tickets.Where(t => t.IsAvailable == true).ToListAsync();
            if(tickets == null || tickets.Count == 0)
            {
                return NotFound();
            }
            return Ok(tickets);
        }
        [HttpGet("{row}")]
        public async Task<ActionResult<List<Tickets>>> GetByRow(char row)
        {
            var tickets = await _context.tickets.Where(t => t.Row == row).ToListAsync();
            if (tickets == null || tickets.Count == 0)
            {
                return NotFound();
            }
            return Ok(tickets);
        }
        [HttpPost("YourOrder")]
        public async Task<ActionResult<List<Tickets>>> GetYourOrder([FromBody] RequestViewOrder request)
        {
            var tickets = await _context.orderTickets
                .Where(ot => ot.Order.UserId == request.UserId &&
                ot.Ticket.IsAvailable == false &&
                ot.Ticket.DateExpriseSale > DateTime.Now)
                .Select(ot => ot.Ticket)
                .ToListAsync();

            if (!tickets.Any())
                return NotFound();

            return Ok(tickets);
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
