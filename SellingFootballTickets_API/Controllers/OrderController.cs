using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SellingFootballTickets_API.Data;
using SellingFootballTickets_API.Models;

namespace SellingFootballTickets_API.Controllers
{
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ServiceContext _context;

        public OrderController(ServiceContext context)
        {
            _context = context;
        }
        [HttpGet("simulate")]
        public async Task<IActionResult> SimulateTicketPurchase()
        {
            // 1️⃣ Create 5 users if not exist
            if (!_context.users.Any())
            {
                var users = new List<Users>
                {
                    new Users { Name = "User1", Email = "user1@gmail.com", Password = "pass1", Phone="0000000000", Gender="Male", Description="", Active = true },
                    new Users { Name = "User2", Email = "user2@gmail.com", Password = "pass2", Phone="0000000001", Gender="Male", Description="",Active = true },
                    new Users { Name = "User3", Email = "user3@gmail.com", Password = "pass3", Phone="0000000002", Gender="Male", Description="",Active = true },
                    new Users { Name = "User4", Email = "user4@gmail.com", Password = "pass4", Phone="0000000003", Gender="Male", Description="",Active = true },
                    new Users { Name = "User5", Email = "user5@gmail.com", Password = "pass5", Phone="0000000004", Gender="Male", Description="",Active = true },
                };
                _context.users.AddRange(users);
                await _context.SaveChangesAsync();
            }

            // 2️⃣ Create 1 ticket if not exist
            var ticket = await _context.tickets.FirstOrDefaultAsync();
            if (ticket == null)
            {
                ticket = new Tickets
                {
                    Code = "FT-002-00A",
                    Description = "Football Match Ticket",
                    Stadium = "Olympic Stadium",
                    Price = 25,
                    Block= 1,
                    Row = 'A',
                    Quantity = 50,
                    KickOff = DateTime.Parse("2026-06-10 19:00"),
                    IsAvailable = true
                };
                _context.tickets.Add(ticket);
                await _context.SaveChangesAsync();
            }

            // 3️⃣ Simulate users buying tickets
            var usersList = await _context.users.ToListAsync();

            foreach (var user in usersList)
            {
                int buyQty = 5; // each user buys 5 tickets

                if (ticket.Quantity < buyQty)
                    continue; // Not enough tickets

                // Create Order
                var order = new Orders
                {
                    UserId = user.Id,
                    TotlePrice = ticket.Price * buyQty,
                    Status = "Paid",
                    OrderDate = DateTime.Now
                };
                _context.orders.Add(order);
                await _context.SaveChangesAsync();

                // Create OrderTickets
                var orderTickets = new List<OrderTicket>();
                for (int i = 0; i < buyQty; i++)
                {
                    orderTickets.Add(new OrderTicket
                    {
                        OrderId = order.Id,
                        TicketId = ticket.Id
                    });
                }
                _context.orderTickets.AddRange(orderTickets);

                // Update Ticket Quantity
                ticket.Quantity -= buyQty;
                if (ticket.Quantity == 0)
                    ticket.IsAvailable = false;

                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                TicketCode = ticket.Code,
                RemainingQuantity = ticket.Quantity,
                IsAvailable = ticket.IsAvailable
            });
        }
    }
}
