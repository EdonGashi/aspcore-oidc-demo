using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet("~/login")]
        public ActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "AuthServer");
        }

        [HttpGet("~/logout"), HttpPost("~/logout")]
        public ActionResult SignOut()
        {
            // is redirected from the identity provider after a successful authorization flow and
            // to redirect the user agent to the identity provider to sign out.
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, "AuthServer");
        }
    }
}