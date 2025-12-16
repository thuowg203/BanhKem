using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string TenLoai { get; set; }
        public List<Product>? Product { get; set; }
    }
}
