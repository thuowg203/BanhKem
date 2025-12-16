using System.Diagnostics;
using DoAnLapTrinhWeb_QLyTiemBanh.Extensions;
using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Models.Enums;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ admin mới truy cập được
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult SearchCustomer(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Json(new List<object>());


            var results = _context.Users
                .Where(u => u.Email.Contains(term) || u.PhoneNumber.Contains(term))
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    phoneNumber = u.PhoneNumber
                })
                .Take(10)
                .ToList();

            return Json(results);
        }
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách sản phẩm
            var products = _context.Products.ToList();
            if (products == null || products.Count == 0)
            {
                ModelState.AddModelError("", "Không có sản phẩm nào.");
            }
            ViewBag.Products = products;

            // Lấy danh sách khách hàng có vai trò "Customer"
            var customers = (from u in _context.Users
                             join ur in _context.UserRoles on u.Id equals ur.UserId
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where r.Name == "Customer"
                             select new { u.Id, u.FullName })
                             .ToList();
            ViewBag.Customers = new SelectList(customers, "Id", "FullName");

            // Khởi tạo session OrderDetails nếu chưa có
            var orderDetails = HttpContext.Session.GetObjectFromJson<List<OrderDetail>>("OrderDetails");
            if (orderDetails == null)
            {
                HttpContext.Session.SetObjectAsJson("OrderDetails", new List<OrderDetail>());
            }

            return View();
        }


       [HttpPost]
public async Task<IActionResult> Create(Order model)
{
    // Bước 1: Gán UserId nếu thiếu
    if (string.IsNullOrEmpty(model.UserId))
    {
        var guestUser = await _userManager.FindByNameAsync("guest");
        if (guestUser != null)
        {
            model.UserId = guestUser.Id;
            ModelState.Remove(nameof(model.UserId)); // Xóa lỗi required cũ
        }
    }

    // Bước 2: Lấy danh sách sản phẩm đã chọn
    var formOrderDetails = Request.Form
        .Where(kv => kv.Key.StartsWith("OrderDetails[") && kv.Key.Contains(".Quantity"))
        .Select(kv =>
        {
            var productIdStr = kv.Key.Split('[', ']')[1];
            var quantity = int.TryParse(kv.Value, out var q) ? q : 0;
            return new { ProductId = int.Parse(productIdStr), Quantity = quantity };
        })
        .Where(x => x.Quantity > 0)
        .ToList();

    if (!formOrderDetails.Any())
    {
        ModelState.AddModelError("", "Chưa chọn sản phẩm nào.");
        LoadViewData();
        return View(model);
    }

    var orderDetails = new List<OrderDetail>();
    foreach (var item in formOrderDetails)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
        if (product == null)
        {
            ModelState.AddModelError("", $"Sản phẩm có ID {item.ProductId} không tồn tại.");
            LoadViewData();
            return View(model);
        }

        orderDetails.Add(new OrderDetail
        {
            ProductId = product.Id,
            Quantity = item.Quantity,
            Price = product.Price
        });
    }

    if (ModelState.IsValid)
    {
        var total = orderDetails.Sum(d => d.Quantity * d.Price);

        var order = new Order
        {
            UserId = model.UserId,
            OrderDate = DateTime.Now,
            DeliveryDateTime = DateTime.Now,
            IsDelivered = true,
            IsAvailable = true,
            TotalPrice = total,
            RecipientName = "Tại cửa hàng",
            RecipientPhone = "Không có",
            District = "Tại cửa hàng",
            Ward = "Tại cửa hàng",
            SpecificAddress = "Tại cửa hàng",
            Notes = model.Notes,
            OrderDetails = orderDetails
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    LoadViewData();
    return View(model);
}

        private void LoadViewData()
        {
            var products = _context.Products.ToList();
            ViewBag.Products = products;

            var customers = (from u in _context.Users
                             join ur in _context.UserRoles on u.Id equals ur.UserId
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where r.Name == "Customer"
                             select new { u.Id, u.FullName })
                             .ToList();
            ViewBag.Customers = new SelectList(customers, "Id", "FullName");
        }

        public async Task<IActionResult> Index(string? filter)
        {
            ViewBag.Filter = filter;
            var orders = _context.Orders.AsQueryable();
            var now = DateTime.Now;
            var today = DateTime.Today;

            switch (filter)
            {
                case "today":
                    orders = orders.Where(o => o.DeliveryDateTime.Date == today);
                    break;
                case "pending":
                    orders = orders.Where(o =>
                    o.OrderStatus == OrderStatus.ChoXacNhan ||
                    o.OrderStatus == OrderStatus.ChoLayHang ||
                    o.OrderStatus == OrderStatus.ChoGiaoHang);
                    break;
                case "overdue":
                    // Đơn chưa giao nhưng đã trễ thời gian giao
                    orders = orders.Where(o =>
                                (o.OrderStatus == OrderStatus.ChoLayHang || o.OrderStatus == OrderStatus.ChoGiaoHang)
                                 && o.DeliveryDateTime < now);
                    break;
                case "urgent":
                    // Lọc đơn hàng chưa giao và thời gian giao trong vòng 60 phút tới
                    orders = orders.Where(o => o.OrderStatus == OrderStatus.ChoGiaoHang &&
                                               EF.Functions.DateDiffMinute(now, o.DeliveryDateTime) <= 60 &&
                                               o.DeliveryDateTime > now);
                    break;
                case "unconfirmed":
                    orders = orders.Where(o => o.OrderStatus == OrderStatus.ChoXacNhan);
                    break;
                case "completed":
                    // Đơn đã giao
                    orders = orders.Where(o => o.OrderStatus == OrderStatus.DaGiao);
                    break;

                case "cancelled":
                    // Đơn đã hủy
                    orders = orders.Where(o => o.OrderStatus == OrderStatus.DaHuy);
                    break;

                default:
                    // Nếu không có filter hoặc filter không hợp lệ thì lấy tất cả
                    break;
            }
          

            var list = await orders
                 .Include(o => o.ApplicationUser)
                 .ToListAsync();

            //// Nếu tất cả đơn hàng đã giao => sắp xếp theo mã đơn giảm dần
            //if (list.All(o => o.IsDelivered))
            //{
            //    list = list.OrderByDescending(o => o.Id).ToList();
            //}
            //else
            //{
            //    // Ngược lại vẫn sắp theo trạng thái + ngày giao
            //    list = list
            //        .OrderBy(o => o.IsDelivered)
            //        .ThenBy(o => o.DeliveryDateTime)
            //        .ThenByDescending(o => o.OrderDate)
            //        .ToList();
            //}
            return View(list);
        }


        // Xem chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        [HttpPost]
        public IActionResult ToggleDeliveryStatus(int id)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                // Nếu chưa xác nhận đơn thì không cho đổi trạng thái giao hàng
                if (!order.IsAvailable)
                {
                    ModelState.AddModelError("", "Chưa xác nhận đơn, không thể đánh dấu đã giao.");
                    return RedirectToAction("Index");
                }

                order.IsDelivered = !order.IsDelivered;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleAvailableStatus(int id)
        {
            var order = _context.Orders.Find(id);
            if (order != null)
            {
                // Nếu đã đánh dấu đã giao thì không cho hủy xác nhận đơn
                if (order.IsDelivered)
                {
                    ModelState.AddModelError("", "Đơn đã giao, không thể hủy xác nhận.");
                    return RedirectToAction("Index");
                }

                order.IsAvailable = !order.IsAvailable;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Order order)
        {
            if (id != order.Id)
                return NotFound();

            var existingOrder = _context.Orders.Find(id);
            if (existingOrder == null)
                return NotFound();

            existingOrder.OrderStatus = order.OrderStatus;
            existingOrder.IsDelivered = order.OrderStatus == OrderStatus.DaGiao;
            existingOrder.IsAvailable = order.OrderStatus != OrderStatus.ChoXacNhan;

            _context.SaveChanges();

            TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            order.OrderStatus = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"✅ Đã cập nhật trạng thái đơn #{id} thành {status}.";
            return RedirectToAction(nameof(Index));
        }


    }
}
