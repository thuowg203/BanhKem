namespace DoAnLapTrinhWeb_QLyTiemBanh.Services
{
    public interface IReviewService
    {
        Task<string> SubmitReviewAsync(int productId, string userId, string comment, int rating);
    }
}
