using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // ✅ Bỏ [JsonIgnore], để gửi sang Flutter được
        public string? ImageUrl { get; set; }
        public string UserId { get; set; } = "";

        [System.Text.Json.Serialization.JsonIgnore]
        public Product? Product { get; set; }
        public int UserCartId { get; set; }
        
        [JsonIgnore] // tránh vòng lặp khi serialize
        public UserCart? UserCart { get; set; }
        [NotMapped] // Không lưu vào DB
        public string? Notes { get; set; }


    }
}
