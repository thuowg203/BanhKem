using Microsoft.AspNetCore.Mvc;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Areas.Admin.Controllers
{
    public class ChatController : Controller
    {
        [Area("Admin")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
