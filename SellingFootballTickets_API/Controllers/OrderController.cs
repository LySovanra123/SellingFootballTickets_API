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
    public class OrderController : ControllerBase
    {
        private readonly ServiceContext _context;

        public OrderController(ServiceContext context)
        {
            _context = context;
        }
        //===============================================
        //              Order Ticket
        //===============================================

        [HttpPost("Ticket")]
        public async Task<IActionResult> BuyTicket([FromBody] BuyTicketRequest request)
        {
            var user = await _context.users.FindAsync(request.UserId);
            if (user == null) return NotFound("User not found");

            var tickets = await _context.tickets
                .Where(t => t.Row.ToString() == request.Row && t.IsAvailable)
                .OrderBy(t => t.Seat)
                .Take(request.Quantity)
                .ToListAsync();

            if (tickets.Count < request.Quantity)
                return BadRequest("Not enough tickets available");

            var order = new Orders
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotlePrice = tickets.Sum(t => t.Price),
                OrderTickets = new List<OrderTicket>()
            };

            foreach (var ticket in tickets)
            {
                order.OrderTickets.Add(new OrderTicket
                {
                    TicketId = ticket.Id,
                    Quantity = 1
                });

                ticket.IsAvailable = false;
            }

            _context.orders.Add(order);
            await _context.SaveChangesAsync();

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = order.TotlePrice,
                PaymentMethod = request.PaymentMethod,
                PaymentDate = DateTime.Now,
                Status = "Paid"
            };

            _context.payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                UsersId = user.Id,
                OrderId = order.Id,
                TotalPrice = order.TotlePrice,
                Tickets = tickets.Select(t => new { t.Id, t.Row, t.Seat }),
                Payment = new { payment.Id, payment.Amount, payment.PaymentMethod, payment.Status }
            });
        }

        //===============================================================
        //              Get All Orders between two dates
        //===============================================================
        [Authorize(Policy = "adminOnly")]
        [HttpPost("/api/GetPaymentsByMethods")]
        public async Task<ActionResult<IEnumerable<Orders>>> GetOrdersBetweenDates([FromBody] RequestGetOrderDashboard request)
        {
            var orders = await _context.orders
                .Where(o => o.OrderDate >= request.StartDate && o.OrderDate <= request.EndDate)
                .Include(o => o.OrderTickets)
                .ToListAsync();
            return Ok(orders);
        }
    }
}
