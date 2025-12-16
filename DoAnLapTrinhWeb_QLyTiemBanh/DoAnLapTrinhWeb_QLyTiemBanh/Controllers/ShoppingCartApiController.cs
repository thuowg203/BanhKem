using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using DoAnLapTrinhWeb_QLyTiemBanh.Services;
using DoAnLapTrinhWeb_QLyTiemBanh.Models.Enums;
using DoAnLapTrinhWeb_QLyTiemBanh.Extensions;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DoAnLapTrinhWeb_QLyTiemBanh.Migrations;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingCartApiController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ShoppingCartApiController(
            IConfiguration config,
            ApplicationDbContext context,
            IProductRepository productRepo,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _config = config;
            _context = context;
            _productRepo = productRepo;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // 🛒 Lấy giỏ hàng
        [Authorize]
        [HttpGet("cart")]
        public async Task<IActionResult> GetCart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cart = await _context.UserCarts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
                return Ok(new { items = new List<object>(), total = 0 });

            var data = cart.CartItems.Select(i => new
            {
                i.ProductId,
                i.Name,
                i.Quantity,
                i.Price,
                i.ImageUrl,
                Category = i.Product?.Category?.TenLoai
            });

            var total = cart.CartItems.Sum(i => i.Price * i.Quantity);
            return Ok(new { items = data, total });
        }

        // ➕ Thêm sản phẩm vào giỏ
        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartAddDto model)
        {
            if (model.ProductId <= 0 || model.Quantity <= 0)
                return BadRequest("Dữ liệu không hợp lệ.");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var product = await _productRepo.GetByIdAsync(model.ProductId);
            if (product == null) return NotFound("Không tìm thấy sản phẩm.");

            var cart = await _context.UserCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new UserCart { UserId = user.Id };
                _context.UserCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var item = cart.CartItems.FirstOrDefault(i => i.ProductId == model.ProductId);
            if (item != null)
            {
                item.Quantity += model.Quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = model.ProductId,
                    Quantity = model.Quantity,
                    Price = product.Price,
                    Name = product.Name,
                    ImageUrl = product.Image,
                    UserId = user.Id,
                    UserCartId = cart.Id
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã thêm sản phẩm vào giỏ hàng." });
        }

        // ❌ Xóa sản phẩm
        [Authorize]
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cart = await _context.UserCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null) return NotFound("Giỏ hàng không tồn tại.");

            var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return NotFound("Không tìm thấy sản phẩm.");

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa sản phẩm." });
        }

        // 🔄 Cập nhật số lượng
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartUpdateDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cart = await _context.UserCarts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null) return NotFound("Không có giỏ hàng.");

            var item = cart.CartItems.FirstOrDefault(i => i.ProductId == model.ProductId);
            if (item == null) return NotFound("Không tìm thấy sản phẩm.");

            item.Quantity = model.Quantity;
            if (item.Quantity <= 0)
                _context.CartItems.Remove(item);

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công." });
        }

        // 💳 Thanh toán
        [Authorize]
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // ⏰ Kiểm tra thời gian nhận hàng (phải sau ít nhất 3 tiếng)
            if (model.DeliveryDateTime < DateTime.Now.AddHours(3))
            {
                return BadRequest("Thời gian nhận hàng phải sau thời điểm đặt ít nhất 3 tiếng.");
            }

            var shipping = 30000m;
            var subtotal = model.TotalPrice > 0 ? model.TotalPrice - shipping : 0;
            var total = subtotal + shipping;

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalPrice = total,
                RecipientName = model.RecipientName,
                RecipientPhone = model.RecipientPhone,
                SpecificAddress = model.SpecificAddress,
                Ward = model.Ward,
                District = model.District,
                DeliveryDateTime = model.DeliveryDateTime,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = model.PaymentMethod == "VNPAY" ? "Đang chờ thanh toán" : "Chưa thanh toán",
                Notes = model.Notes,
                IsAvailable = false,
                IsDelivered = false
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 📝 Ghi chú từng bánh từ FE
            foreach (var d in model.OrderDetails)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = d.ProductId,
                    Quantity = d.Quantity,
                    Price = d.Price,
                    Notes = d.Notes
                });
            }
            await _context.SaveChangesAsync();

            // 💳 Nếu chọn VNPAY → tạo URL thanh toán
            if (model.PaymentMethod == "VNPAY")
            {
                string vnp_Returnurl = _config["VnPay:ReturnUrlMobile"];

                
                string vnp_Url = _config["VnPay:BaseUrl"];
                string vnp_TmnCode = _config["VnPay:TmnCode"];
                string vnp_HashSecret = _config["VnPay:HashSecret"];

                VnPayLibrary vnpay = new VnPayLibrary();
                vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                vnpay.AddRequestData("vnp_Amount", ((int)order.TotalPrice * 100).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toán đơn hàng #{order.Id}");
                vnpay.AddRequestData("vnp_OrderType", "billpayment");
                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString());

                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                return Ok(new { paymentUrl, orderId = order.Id });
            }

            // 💵 Nếu COD → xác nhận đơn
            if (model.PaymentMethod == "COD")
            {
                // ✅ Xóa giỏ hàng của người dùng sau khi đặt hàng COD
                var cart = await _context.UserCarts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (cart != null)
                {
                    _context.CartItems.RemoveRange(cart.CartItems);
                    _context.UserCarts.Remove(cart);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Đặt hàng thành công", orderId = order.Id });
            }
            return Ok(new { message = "Đặt hàng thành công", orderId = order.Id });
        }


        [Authorize]
        [HttpGet("check-payment-status/{orderId}")]
        public async Task<IActionResult> CheckPaymentStatus(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            bool isPaid = order.PaymentStatus == "Đã thanh toán";
            return Ok(new { isPaid, status = order.PaymentStatus });
        }


        [AllowAnonymous]
        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            var vnpayData = Request.Query;
            VnPayLibrary vnpay = new VnPayLibrary();

            foreach (var item in vnpayData)
                vnpay.AddResponseData(item.Key, item.Value);

            string vnp_HashSecret = _config["VnPay:HashSecret"];
            bool checkSignature = vnpay.ValidateSignature(vnpayData["vnp_SecureHash"], vnp_HashSecret);

            string orderIdStr = vnpay.GetResponseData("vnp_TxnRef");
            string responseCode = vnpay.GetResponseData("vnp_ResponseCode");

            if (!checkSignature || !int.TryParse(orderIdStr, out int orderId))
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng" });

            if (responseCode == "00")
            {
                order.PaymentStatus = "Đã thanh toán";
                await _context.SaveChangesAsync();

                // ✅ Tìm và xóa giỏ hàng của người dùng
                var cart = await _context.UserCarts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == order.UserId);

                if (cart != null)
                {
                    _context.CartItems.RemoveRange(cart.CartItems);
                    _context.UserCarts.Remove(cart);
                    await _context.SaveChangesAsync();
                }

                var html = @"
    <!DOCTYPE html>
    <html lang='vi'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Thanh toán thành công</title>
        <style>
            body {
                background: #f9fff9;
                font-family: 'Segoe UI', sans-serif;
                text-align: center;
                padding-top: 100px;
                color: #333;
            }
            h2 { color: #2ecc71; font-size: 28px; }
            p { font-size: 16px; color: #666; }
        </style>
    </head>
    <body>
        <h2>✅ Thanh toán thành công!</h2>
        <p>Bạn hãy chuyển về ứng dụng để có thể tiếp tục mua hàng</p>
        <script>
            setTimeout(function(){
                window.close();
            }, 5000);
        </script>
    </body>
    </html>";
                return Content(html, "text/html; charset=utf-8");
            }

            else
            {
                order.PaymentStatus = "Thất bại";
                await _context.SaveChangesAsync();

                var html = @"
            <!DOCTYPE html>
            <html lang='vi'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Thanh toán thất bại</title>
                <style>
                    body {
                        background: #fff7f7;
                        font-family: 'Segoe UI', sans-serif;
                        text-align: center;
                        padding-top: 100px;
                        color: #333;
                    }
                    h2 { color: #e74c3c; font-size: 28px; }
                    p { font-size: 16px; color: #666; }
                </style>
            </head>
            <body>
                <h2>❌ Thanh toán thất bại!</h2>
                <p>Bạn có thể quay lại ứng dụng để thử lại.</p>
                <script>
                    setTimeout(function(){
                        window.close();
                    }, 5000);
                </script>
            </body>
            </html>";
                return Content(html, "text/html; charset=utf-8");
            }
        }










    }

    // DTOs ======================================================

    public class CartAddDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartUpdateDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CheckoutDto
    {
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string SpecificAddress { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public string PaymentMethod { get; set; }
        public string? Notes { get; set; }

        public List<OrderDetailDto> OrderDetails { get; set; } = new();
        public decimal TotalPrice { get; set; }
    }

    public class OrderDetailDto
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Notes { get; set; }
    }
}
