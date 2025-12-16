using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Services
{
    public class CancelExpiredOrdersService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public CancelExpiredOrdersService(IServiceScopeFactory scopeFactory)
            => _scopeFactory = scopeFactory;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var expired = await db.Orders
                    .Where(o => o.PaymentStatus == "Pending" &&
                                o.OrderDate < DateTime.Now.AddMinutes(-15))
                    .ToListAsync();

                foreach (var o in expired)
                    o.PaymentStatus = "Expired";

                await db.SaveChangesAsync();

                // kiểm tra mỗi 5 phút
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
