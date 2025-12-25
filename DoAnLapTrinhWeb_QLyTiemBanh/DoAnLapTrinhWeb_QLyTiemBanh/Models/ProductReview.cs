using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    public class ProductReview
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        [StringLength(1000)]
        public string Comment { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsPositive { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
