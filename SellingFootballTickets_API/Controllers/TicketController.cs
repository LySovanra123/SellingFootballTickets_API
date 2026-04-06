using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
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

        //===========================================================
        //         Generate unique code for each ticket
        //===========================================================
        [NonAction]
        public string GenerateUniqueCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //===========================================================
        //         Generate QR code for each ticket
        //===========================================================
        [NonAction]
        public byte[] GenerateQRCode(string code)
        {
            using (var qrCenerator = new QRCodeGenerator())
            {
                var qrData = qrCenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new QRCode(qrData))
                {
                    using (var bitmap = qrCode.GetGraphic(20))
                    {
                        using (var stream = new MemoryStream())
                        {
                            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            return stream.ToArray();
                        }
                    }
                }

            }
        }

        public TicketController(ServiceContext context)
        {
            _context = context;
        }

        //==============================================
        //         Get all available tickets 
        //==============================================
        [HttpGet("/api/GetTicket")]
        [Authorize]
        public async Task<ActionResult<List<Tickets>>> Get()
        {
            var tickets = await _context.tickets.Where(t => t.IsAvailable == true).ToListAsync();
            if (tickets == null || tickets.Count == 0)
            {
                return NotFound();
            }
            return Ok(tickets);
        }


        //==============================================
        //         Get tickets by row
        //==============================================
        [HttpGet("{row}")]
        [Authorize]
        public async Task<ActionResult<List<Tickets>>> GetByRow(char row)
        {
            var tickets = await _context.tickets.Where(t => t.Row == row).ToListAsync();
            if (tickets == null || tickets.Count == 0)
            {
                return NotFound();
            }
            return Ok(tickets);
        }


        //==============================================
        //         Get tickets for personal order
        //==============================================
        [HttpPost("YourOrder")]
        [Authorize(Policy = "userOnly")]
        public async Task<ActionResult<List<Tickets>>> GetYourOrder([FromBody] RequestViewOrder request)
        {
            var tickets = await _context.orderTickets
                .Include(ot => ot.Order)
                .Include(ot => ot.Ticket)
                .Where(ot => ot.Order.UserId == request.UserId &&
                             !ot.Ticket.IsAvailable &&
                             ot.Ticket.DateExpriseSale > DateTime.Now)
                .Select(ot => ot.Ticket)
                .ToListAsync();

            if (!tickets.Any())
                return NotFound(new { message = "Ticket not found" })
            ;

            return Ok(tickets);
        }


        //==============================================
        //         Add new tickets
        //==============================================
        [HttpPost("/api/addTicket")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<List<Tickets>>> AddTicket(Tickets ticket, int quantity)
        {
            var tickets = new List<Tickets>();

            for (int i = 0; i < quantity; i++)
            {
                string code = GenerateUniqueCode();
                string match = ticket.Description;
                string stadium = ticket.Stadium;
                string kickoff = ticket.KickOff.ToString("yyyy-MM-dd");
                string time = ticket.KickOff.ToString("HH:mm");
                string price = ticket.Price.ToString("F2");
                string block = ticket.Block.ToString();
                string row = ticket.Row.ToString();
                string seat = (ticket.Seat + i).ToString();

                string stringCode = "Code : " + code +
                    "\nMatch : " + match +
                    "\nStadium : " + stadium +
                    "\nPrice : " + price + "$" +
                    "\nKick Off : " + kickoff +
                    "\nTime : " + time +
                    "\nBlock : " + block +
                    "\nRow : " + row +
                    "\nSeat : " + seat;

                tickets.Add(new Tickets
                {
                    Code = code,
                    QRCode = GenerateQRCode(stringCode),
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



        //================================================
        //        Group tickets by match and date
        //================================================
        [HttpGet("/api/GroupticketMatch")]
        [Authorize]
        public async Task<ActionResult> GetTicketsByMatchAndDate()
        {
            var result = await _context.tickets
                .Where(t => t.IsAvailable == true)
                .GroupBy(t => new { t.Description, t.KickOff })
                .Select(g => new
                {
                    Description = g.Key.Description,
                    KickOff = g.Key.KickOff,
                    TicketCount = g.Count()
                })
                .ToListAsync();

            return Ok(result);
        }



        // Fix....


        //==============================================
        //         Update ticket by Row and KickOff
        //==============================================
        [HttpPost("/api/Ticket/Update")]
        [Authorize(Policy = "adminOnly")]
        public async Task<IActionResult> UpdateTicketByRowAndKickOff(char row,[FromBody] RequestSelectMatch request,[FromBody] Tickets updatedTicket)
        {
            var ticket = await _context.tickets.FirstOrDefaultAsync(t => t.Row == row && t.KickOff == request.KickOff && t.Description == request.Description);
            if (ticket == null)
            {
                return NotFound();
            }
            ticket.Description = updatedTicket.Description;
            ticket.Stadium = updatedTicket.Stadium;
            ticket.Price = updatedTicket.Price;
            ticket.DateExpriseSale = updatedTicket.DateExpriseSale;
            ticket.Block = updatedTicket.Block;
            ticket.Seat = updatedTicket.Seat;
            ticket.IsAvailable = updatedTicket.IsAvailable;
            await _context.SaveChangesAsync();
            return NoContent();

        }
    }
}
