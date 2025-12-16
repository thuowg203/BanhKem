using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            IProductRepository productRepository,
            ApplicationDbContext context)
        {
            _logger = logger;
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var ordersQuery = _context.Orders.AsQueryable();

            // Doanh thu
            ViewBag.TodaysRevenue = await ordersQuery
                .Where(o => o.OrderDate.Date == today)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            ViewBag.MonthlyRevenue = await ordersQuery
                .Where(o => o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            // Đơn hàng
            ViewBag.TodaysOrders = await ordersQuery.CountAsync(o => o.OrderDate.Date == today);
            ViewBag.PendingOrders = await ordersQuery.CountAsync(o => !o.IsAvailable); // chưa xác nhận
            ViewBag.PendingDeliveryOrders = await ordersQuery.CountAsync(o => o.IsAvailable && !o.IsDelivered); // đã xác nhận, chưa giao

            // Dữ liệu biểu đồ doanh thu trong tháng
            ViewBag.RevenueByDay = await ordersQuery
                .Where(o => o.OrderDate >= startOfMonth && o.OrderDate <= endOfMonth)
                .GroupBy(o => o.OrderDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    day = g.Key.ToString("dd/MM"),
                    revenue = g.Sum(o => o.TotalPrice)
                })
                .ToListAsync();

            // 5 đơn hàng mới nhất
            ViewBag.RecentOrders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // 5 đơn hàng chờ giao gần nhất
            ViewBag.PendingDeliveryOrderList = await ordersQuery
                .Where(o => o.IsAvailable && !o.IsDelivered)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
