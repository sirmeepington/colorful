using DSharpPlus;
using Microsoft.AspNetCore.Mvc;

namespace Colorful.Web.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        [Route("~/about")]
        public IActionResult About()
        {
            return View();
        }

        [Route("~/invite")]
        public IActionResult Invite()
        {
            return Redirect("https://discord.com/api/oauth2/authorize?client_id=460034890432512021&permissions=268470272&scope=bot%20applications.commands");
        }
    }
}
