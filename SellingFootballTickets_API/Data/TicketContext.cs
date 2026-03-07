using Microsoft.EntityFrameworkCore;
using SellingFootballTickets_API.Models;

namespace SellingFootballTickets_API.Data
{
    public class TicketContext : DbContext
    {
        public TicketContext(DbContextOptions<TicketContext> options) : base(options)
        {
        }
        public DbSet<Tickets> Tickets { get; set; }
    }
}
