using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; } // Giá của sản phẩm tại thời điểm đặt hàng
        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
