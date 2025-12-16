using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    public class ShoppingCart
    {
        public Product? Product { get; set; }
        [Range(1, 1000)]
        public int Quantity { get; set; }
        [JsonPropertyName("items")]
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }
        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }

    }
}
