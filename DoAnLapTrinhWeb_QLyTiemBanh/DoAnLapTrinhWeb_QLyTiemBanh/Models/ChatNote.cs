namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    public class ChatNote
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string AdminId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
