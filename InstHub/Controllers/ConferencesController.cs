using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InstHub.Controllers
{
    public class ConferencesController : Controller
    {
        public IActionResult UserConference()
        {
            return View();
        }

        public IActionResult UserConferences()
        {
            return View();
        }
    }
}
