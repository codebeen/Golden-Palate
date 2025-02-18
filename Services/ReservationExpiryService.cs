using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RRS.Services
{
    public class ReservationExpiryService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ReservationExpiryService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var reservationCanceller = scope.ServiceProvider.GetRequiredService<CancelExpiredReservations>();
                    await reservationCanceller.CancelExpiredReservationsAsync();
                }

                // Calculate time until next day (run every day at midnight)
                var now = DateTime.Now;
                var nextRunTime = now.Date.AddDays(1); // Next midnight
                var delay = nextRunTime - now;

                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
