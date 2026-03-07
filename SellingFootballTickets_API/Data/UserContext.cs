using Microsoft.EntityFrameworkCore;
using SellingFootballTickets_API.Models;

namespace SellingFootballTickets_API.Data
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
        }
        public DbSet<Users> Users { get; set; }
    }
}
