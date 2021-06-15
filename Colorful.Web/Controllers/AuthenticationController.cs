using Colorful.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Colorful.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet("~/signin")]
        public IActionResult SignIn() {
            var user = HttpContext.User;
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var nickname = user.FindFirstValue(ClaimTypes.Name);

            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "Discord");
        }

        [HttpGet("~/signout")]
        [HttpPost("~/signout")]
        [Authorize]
        public IActionResult SignOutCurrentUser()
        {
            // Instruct the cookies middleware to delete the local cookie created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            return SignOut(new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
