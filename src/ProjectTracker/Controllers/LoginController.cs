using ProjectTracker.Models;
using System.Web.Mvc;
using System.Web.Security;

namespace ProjectTracker.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private ProjectTrackerEntities db = new ProjectTrackerEntities();

        [HttpGet]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public ActionResult Index(Login login, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            if (!ModelState.IsValid)
                return View(login);

            foreach (Employee emp in db.Employees)
            {
                if (login.Username.Trim().ToLower() == emp.Username.ToLower() && login.Password == emp.Password)
                {
                    Models.Login.SetUserInfo(HttpContext, emp);
                    FormsAuthentication.SetAuthCookie(emp.Username, login.Remember);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }
            }
            
            ModelState.AddModelError(string.Empty, "The username or password provided is incorrect.");
            return View(login);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
	}
}