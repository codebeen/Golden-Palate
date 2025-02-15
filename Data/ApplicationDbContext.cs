using Microsoft.EntityFrameworkCore;
using RRS.Models;
using RRS.Models.ViewModels;

namespace RRS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationDetailsViewModel> ReservationDetails { get; set; }
        public DbSet<ReservationDetailsDto> ReservationDetailsDto { get; set; }
    }
}
