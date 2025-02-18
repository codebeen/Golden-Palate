using RRS.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace RRS.Services
{
    public class CancelExpiredReservations
    {
        private readonly ApplicationDbContext _context;

        public CancelExpiredReservations(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CancelExpiredReservationsAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("EXEC CancelExpiredReservations");
        }
    }
}
