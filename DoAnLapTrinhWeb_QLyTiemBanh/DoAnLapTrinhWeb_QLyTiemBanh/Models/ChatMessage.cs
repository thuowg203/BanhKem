using System.ComponentModel.DataAnnotations;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;   // userId hoặc "admin@tiembanh.local"

        [Required]
        public string ReceiverId { get; set; } = string.Empty; // userId hoặc "admin@tiembanh.local"

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsFromAdmin { get; set; } = false; //
    }
}
