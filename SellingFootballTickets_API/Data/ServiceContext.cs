using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SellingFootballTickets_API.Models;

namespace SellingFootballTickets_API.Data
{
    public class ServiceContext : DbContext
    {
        public ServiceContext(DbContextOptions<ServiceContext> options) : base(options)
        {
        }
        public DbSet<Users> users { get; set; }
        public DbSet<Tickets> tickets { get; set; }
        public DbSet<Orders> orders { get; set; }
        public DbSet<OrderTicket> orderTickets { get; set; }
        public DbSet<Payment> payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Users>().ToTable("users");
            modelBuilder.Entity<Tickets>().ToTable("tickets");
            modelBuilder.Entity<Orders>().ToTable("orders");
            modelBuilder.Entity<OrderTicket>().ToTable("orderTickets");
            modelBuilder.Entity<Payment>().ToTable("payments");

            // Configure relationships for Users and Orders
            modelBuilder.Entity<Users>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId);

            // Configure relationships for the join table (OrderTickets)
            modelBuilder.Entity<OrderTicket>()
                 .HasKey(ot => ot.Id); // Primary keyy

            modelBuilder.Entity<OrderTicket>()
                .HasOne(ot => ot.Order) // navigation property
                .WithMany(o => o.OrderTickets)
                .HasForeignKey(ot => ot.OrderId);

            modelBuilder.Entity<OrderTicket>()
                .HasOne(ot => ot.Ticket) // navigation property
                .WithMany(t => t.OrderTickets)
                .HasForeignKey(ot => ot.TicketId);


            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId);

        }
    }
}
