using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    public class UserCart
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        // ✅ Tên thuộc tính nên là CartItems để EF hiểu đúng mối quan hệ
        public ICollection<CartItem>? CartItems { get; set; } = new List<CartItem>();
    }
}
