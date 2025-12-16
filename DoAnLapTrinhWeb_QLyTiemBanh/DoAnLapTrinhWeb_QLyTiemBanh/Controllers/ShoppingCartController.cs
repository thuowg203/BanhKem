using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DoAnLapTrinhWeb_QLyTiemBanh.Extensions;
using DoAnLapTrinhWeb_QLyTiemBanh.Models;
using DoAnLapTrinhWeb_QLyTiemBanh.Repositories;
using DoAnLapTrinhWeb_QLyTiemBanh.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Newtonsoft.Json;
using DoAnLapTrinhWeb_QLyTiemBanh.Models.Enums;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace DoAnLapTrinhWeb_QLyTiemBanh.Controllers
{
   
    public class ShoppingCartController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IProductRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartRepository _cartRepository;
        private readonly IEmailSender _emailService;

        public ShoppingCartController(
            IProductRepository productRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailService,
            IConfiguration configuration,
            ICartRepository cartRepository)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _configuration = configuration;
            _cartRepository = cartRepository;
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return NotFound();

            // Nếu user đã đăng nhập → chỉ lưu vào DB
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var dbCart = await _context.UserCarts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.UserId == user.Id);

                    if (dbCart == null)
                    {
                        dbCart = new UserCart { UserId = user.Id };
                        _context.UserCarts.Add(dbCart);
                        await _context.SaveChangesAsync();
                    }

                    // Kiểm tra sản phẩm có tồn tại chưa
                    var existing = dbCart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                    if (existing != null)
                    {
                        existing.Quantity += quantity;
                    }
                    else
                    {
                        dbCart.CartItems.Add(new CartItem
                        {
                            ProductId = productId,
                            Quantity = quantity,
                            Price = product.Price,
                            Name = product.Name,
                            ImageUrl = product.Image,
                            UserId = user.Id,
                            UserCartId = dbCart.Id
                        });
                    }

                    await _context.SaveChangesAsync();

                    // ✅ XÓA SESSION CART để không bao giờ bị trùng
                    HttpContext.Session.Remove("Cart");

                    return RedirectToAction("Index");
                }
            }

            // Nếu chưa đăng nhập → dùng Session tạm
            var sessionCart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            sessionCart.AddItem(new CartItem
            {
                ProductId = productId,
                Quantity = quantity,
                Price = product.Price,
                Name = product.Name,
                ImageUrl = product.Image
            });
            HttpContext.Session.SetObjectAsJson("Cart", sessionCart);

            return RedirectToAction("Index");
        }



        //  Hiển thị giỏ hàng
        public async Task<IActionResult> Index()
        {
            var sessionCart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

            // Nếu user đăng nhập → lấy giỏ từ DB
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var dbCart = await _context.UserCarts
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                        .FirstOrDefaultAsync(c => c.UserId == user.Id);

                    if (dbCart == null)
                    {
                        dbCart = new UserCart { UserId = user.Id };
                        _context.UserCarts.Add(dbCart);
                        await _context.SaveChangesAsync();
                    }

                    sessionCart = new ShoppingCart
                    {
                        Items = dbCart.CartItems.ToList()
                    };

                    // clear product tránh lỗi JSON cycle
                    foreach (var it in sessionCart.Items)
                        it.Product = null;

                    HttpContext.Session.SetObjectAsJson("Cart", sessionCart);
                }
            }

            var cart = sessionCart ?? new ShoppingCart();
            cart.Items ??= new List<CartItem>();

            // Cập nhật thông tin sản phẩm đầy đủ (ảnh, giá, tên)
            foreach (var it in cart.Items)
            {
                var prod = await _productRepository.GetByIdAsync(it.ProductId);
                if (prod != null)
                {
                    it.Product = prod;
                    if (string.IsNullOrEmpty(it.ImageUrl))
                        it.ImageUrl = prod.Image;
                }
            }

            return View(cart);
        }

        // ✅ Xóa sản phẩm khỏi giỏ
        [HttpPost]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.RemoveItem(productId);

            foreach (var it in cart.Items)
                it.Product = null;

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var dbCart = await _context.UserCarts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.UserId == user.Id);

                    if (dbCart != null)
                    {
                        var item = dbCart.CartItems.FirstOrDefault(i => i.ProductId == productId);
                        if (item != null)
                        {
                            _context.CartItems.Remove(item);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            return RedirectToAction("Index");
        }

        // ✅ Cập nhật số lượng sản phẩm
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int change)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity += change;
                if (item.Quantity <= 0)
                    cart.RemoveItem(productId);
            }

            foreach (var it in cart.Items)
                it.Product = null;

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var dbCart = await _context.UserCarts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.UserId == user.Id);

                    if (dbCart != null)
                    {
                        var dbItem = dbCart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                        if (dbItem != null)
                        {
                            var sessionItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                            if (sessionItem == null)
                                _context.CartItems.Remove(dbItem);
                            else
                                dbItem.Quantity = sessionItem.Quantity;

                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // ✅ Thanh toán
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var user = await _userManager.GetUserAsync(User);

            foreach (var item in cart.Items)
            {
                item.Product = await _context.Products
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);
            }

            ViewBag.Cart = cart;
            return View(new Order
            {
                RecipientName = user?.FullName,
                RecipientPhone = user?.PhoneNumber
            });
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Checkout(Order model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");
            
            model.UserId = user.Id;
            ModelState.Remove(nameof(model.UserId));

            // 🔹 Lấy giỏ hàng từ DB
            var dbCart = await _context.UserCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (dbCart == null || !dbCart.CartItems.Any())
            {
                ModelState.AddModelError("", "Giỏ hàng của bạn đang trống.");
                return View(model);
            }
            var cart = dbCart.CartItems.Select((c, index) => new
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                Price = c.Price,
                Notes = Request.Form[$"OrderDetails[{index}].Notes"].ToString().Trim(),
                Name = c.Name,
                CategoryName = c.Product?.Category?.TenLoai
            }).ToList();
            for (int i = 0; i < cart.Count; i++)
            {
                var item = cart[i];
                var cate = item.CategoryName?.Trim().ToLower();
                if ((cate?.Contains("bánh sinh nhật") == true || cate?.Contains("bánh kem") == true)
                    && string.IsNullOrWhiteSpace(item.Notes))
                {
                    ModelState.AddModelError($"OrderDetails[{i}].Notes",
                        $"Vui lòng nhập tên & tuổi cho bánh sinh nhật '{item.Name}'.");
                }
            }


            if (model.DeliveryDateTime == null)
            {
                ModelState.AddModelError(nameof(model.DeliveryDateTime), "Vui lòng chọn thời gian nhận hàng.");
            }
            else if (model.DeliveryDateTime < DateTime.Now.AddHours(3))
            {
                ModelState.AddModelError(nameof(model.DeliveryDateTime),
                    "Thời gian nhận hàng phải sau thời điểm đặt ít nhất 3 tiếng.");
            }

            // ✅ 3️⃣ Kiểm tra địa chỉ
            if (string.IsNullOrWhiteSpace(model.SpecificAddress))
                ModelState.AddModelError(nameof(model.SpecificAddress), "Vui lòng nhập địa chỉ cụ thể.");

            // ✅ 4️⃣ Kiểm tra tên và số điện thoại
            if (string.IsNullOrWhiteSpace(model.RecipientName))
                ModelState.AddModelError(nameof(model.RecipientName), "Vui lòng nhập họ tên người nhận.");
            if (string.IsNullOrWhiteSpace(model.RecipientPhone))
                ModelState.AddModelError(nameof(model.RecipientPhone), "Vui lòng nhập số điện thoại.");

            if (!ModelState.IsValid)
            {
                // 🔍 Ghi log toàn bộ lỗi để tìm nguyên nhân reload
                var allErrors = ModelState
                    .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();

                Console.WriteLine("Checkout Validation Errors:");
                foreach (var err in allErrors)
                    Console.WriteLine(err);

                // 👀 Hiển thị lỗi ngay trên View để dễ xem
                ViewBag.Errors = allErrors;
                ViewBag.Cart = new ShoppingCart { Items = dbCart.CartItems.ToList() };

                return View(model);
            }


            // 🔹 Tính tổng đơn hàng
            var shippingFee = 30000m;
            var subtotal = dbCart.CartItems.Sum(i => i.Price * i.Quantity);
            var total = subtotal + shippingFee;

            // 🔹 Tạo Order (chờ thanh toán nếu VNPAY)
            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                TotalPrice = total,
                DeliveryDateTime = model.DeliveryDateTime,
                RecipientName = model.RecipientName,
                RecipientPhone = model.RecipientPhone,
                District = model.District,
                Ward = model.Ward,
                SpecificAddress = model.SpecificAddress,
                Notes = model.Notes,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = model.PaymentMethod == "VNPAY" ? "Đang chờ thanh toán" : "Chưa thanh toán",
                IsAvailable = false,
                IsDelivered = false
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // cần để có order.Id

            // 🔹 Thêm chi tiết sản phẩm
            foreach (var item in dbCart.CartItems)
            {
                _context.OrderDetails.Add(new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Notes = item.Notes
                });
            }
            await _context.SaveChangesAsync();

            // 🔹 Nếu chọn VNPAY → chuyển sang cổng thanh toán
            if (model.PaymentMethod == "VNPAY")
            {
                string vnp_Returnurl = _configuration["VnPay:ReturnUrlWeb"];
                string vnp_Url = _configuration["VnPay:BaseUrl"];
                string vnp_TmnCode = _configuration["VnPay:TmnCode"];
                string vnp_HashSecret = _configuration["VnPay:HashSecret"];

                VnPayLibrary vnpay = new VnPayLibrary();
                vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                vnpay.AddRequestData("vnp_Amount", ((long)order.TotalPrice * 100).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toán đơn hàng #{order.Id}");
                vnpay.AddRequestData("vnp_OrderType", "billpayment");
                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString());

                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                return Redirect(paymentUrl);
            }

            // 🔹 Nếu là tiền mặt → lưu luôn và xóa giỏ hàng
            _context.CartItems.RemoveRange(dbCart.CartItems);
            _context.UserCarts.Remove(dbCart);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderComfirmation", new { id = order.Id });
        }

        [AllowAnonymous]
        public async Task<IActionResult> VnPayReturn()
        {
            var vnpayData = Request.Query;
            VnPayLibrary vnpay = new VnPayLibrary();
            foreach (var item in vnpayData)
                vnpay.AddResponseData(item.Key, item.Value);

            string vnp_HashSecret = _configuration["VnPay:HashSecret"];
            bool checkSignature = vnpay.ValidateSignature(vnpayData["vnp_SecureHash"], vnp_HashSecret);

            string orderIdStr = vnpay.GetResponseData("vnp_TxnRef");
            string responseCode = vnpay.GetResponseData("vnp_ResponseCode");

            if (!checkSignature || !int.TryParse(orderIdStr, out int orderId))
            {
                ViewBag.Message = "Dữ liệu không hợp lệ hoặc đã bị thay đổi.";
                return View("PaymentResult");
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                ViewBag.Message = "Không tìm thấy đơn hàng.";
                return View("PaymentResult");
            }

            if (responseCode == "00") // ✅ Thanh toán thành công
            {
                order.PaymentStatus = "Đã thanh toán";

                // Xóa giỏ hàng của user
                var cart = await _context.UserCarts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == order.UserId);

                if (cart != null)
                {
                    _context.CartItems.RemoveRange(cart.CartItems);
                    _context.UserCarts.Remove(cart);
                }

                await _context.SaveChangesAsync();
                ViewBag.Message = $"Thanh toán thành công cho đơn #{order.Id}";
            }
            else
            {
                order.PaymentStatus = "Thất bại";
                order.OrderStatus = OrderStatus.DaHuy;
                await _context.SaveChangesAsync();
                ViewBag.Message = $"Thanh toán thất bại hoặc bị hủy. Mã đơn: #{order.Id}";
            }

            return View("PaymentResult", order);
        }



        //[Authorize]
        //public async Task<IActionResult> VnPayPayment(int id)
        //{
        //    var order = await _context.Orders.FindAsync(id);
        //    if (order == null) return NotFound();

        //    string vnp_Returnurl = _configuration["VnPay:ReturnUrl"];
        //    string vnp_Url = _configuration["VnPay:BaseUrl"];
        //    string vnp_TmnCode = _configuration["VnPay:TmnCode"];
        //    string vnp_HashSecret = _configuration["VnPay:HashSecret"];

        //    VnPayLibrary vnpay = new VnPayLibrary();
        //    vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        //    vnpay.AddRequestData("vnp_Command", "pay");
        //    vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
        //    vnpay.AddRequestData("vnp_Amount", ((int)order.TotalPrice * 100).ToString());
        //    vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        //    vnpay.AddRequestData("vnp_CurrCode", "VND");
        //    vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
        //    vnpay.AddRequestData("vnp_Locale", "vn");
        //    vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toán đơn hàng #{order.Id}");
        //    vnpay.AddRequestData("vnp_OrderType", "billpayment");
        //    vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
        //    vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString());

        //    string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

        //    return Redirect(paymentUrl);
        //}

        //[AllowAnonymous]
        //public IActionResult VnPayReturn()
        //{
        //    var vnpayData = Request.Query;
        //    VnPayLibrary vnpay = new VnPayLibrary();
        //    foreach (var item in vnpayData)
        //        vnpay.AddResponseData(item.Key, item.Value);

        //    string vnp_HashSecret = _configuration["VnPay:HashSecret"];
        //    bool checkSignature = vnpay.ValidateSignature(vnpayData["vnp_SecureHash"], vnp_HashSecret);

        //    string orderId = vnpay.GetResponseData("vnp_TxnRef");
        //    string responseCode = vnpay.GetResponseData("vnp_ResponseCode");

        //    if (!checkSignature)
        //    {
        //        ViewBag.Message = "Chữ ký không hợp lệ hoặc dữ liệu bị thay đổi.";
        //        return View("PaymentResult", null);
        //    }

        //    var order = _context.Orders.Include(o => o.OrderDetails)
        //                               .ThenInclude(od => od.Product)
        //                               .FirstOrDefault(o => o.Id.ToString() == orderId);

        //    if (order == null)
        //    {
        //        ViewBag.Message = "Không tìm thấy đơn hàng.";
        //        return View("PaymentResult", null);
        //    }

        //    if (responseCode == "00")
        //    {
        //        order.PaymentStatus = "Đã thanh toán";
        //        _context.SaveChanges();
        //        ViewBag.Message = "Thanh toán thành công!";
        //    }
        //    else
        //    {
        //        order.PaymentStatus = "Thanh toán thất bại";
        //        _context.SaveChanges();
        //        ViewBag.Message = "Thanh toán thất bại hoặc bị hủy.";
        //    }

        //    return View("PaymentResult", order);
        //}

        [Authorize]
        public async Task<IActionResult> OrderComfirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }


    }
}
