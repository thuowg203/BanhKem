using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DoAnLapTrinhWeb_QLyTiemBanh.Models.Enums;
namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        public string RecipientName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại người nhận")]
        public string RecipientPhone { get; set; }
        public DateTime OrderDate { get; set; }
        [Precision(18, 2)]
        public decimal TotalPrice { get; set; } // Tổng giá trị đơn hàng
        // ----- ĐỊA CHỈ NHẬN HÀNG (lấy từ form) -----
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể.")]
        [StringLength(255)]
        public string SpecificAddress { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn quận.")]
        public string District { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phường.")]
        public string Ward { get; set; }

        // ----- THỜI GIAN NHẬN HÀNG (lấy từ form, có thể null) -----
        [Required(ErrorMessage = "Vui lòng chọn ngày nhận hàng.")]
        public DateTime DeliveryDateTime { get; set; }

        public string? Notes { get; set; }

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public bool IsDelivered { get; set; }
        public bool IsAvailable { get; set; }
        //phuong thức thanh toán
        public string PaymentMethod { get; set; }

        public string PaymentStatus { get; set; } = "Chưa thanh toán";
        public string? ExternalOrderId { get; set; }
        public OrderStatus OrderStatus { get; set; } = OrderStatus.ChoXacNhan;

    }
}
