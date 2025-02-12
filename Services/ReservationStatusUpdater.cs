using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using RRS.Data;

namespace RRS.Services
{
    public class ReservationStatusUpdater : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ReservationStatusUpdater(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Get the current time
                    TimeSpan currentTime = DateTime.Now.TimeOfDay;
                    DateTime today = DateTime.Today;

                    // Update reservations that have passed their allowed time
                    var expiredReservations = context.Reservations
                        .Where(r => r.ReservationDate == DateOnly.FromDateTime(today) &&
                                    ((r.BuffetType == "Breakfast" && currentTime > new TimeSpan(8, 30, 0)) ||
                                     (r.BuffetType == "Lunch" && currentTime > new TimeSpan(12, 0, 0)) ||
                                     (r.BuffetType == "Dinner" && currentTime > new TimeSpan(17, 30, 0))))
                        .ToList();

                    foreach (var reservation in expiredReservations)
                    {
                        reservation.Status = "Cancelled";
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Runs every 5 minutes
            }
        }
    }
}
