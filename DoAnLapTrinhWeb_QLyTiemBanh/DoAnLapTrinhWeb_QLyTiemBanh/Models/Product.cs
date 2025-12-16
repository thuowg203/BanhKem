using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required, StringLength(200)]
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "decimal(18,0)")]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Image { get; set; }
        public string? Badge { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
