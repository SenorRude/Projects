namespace ProjectTracker.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Web;
    using System.Web.Mvc;

    public class Login
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Keep me logged in")]
        public bool Remember { get; set; }

        public virtual Employee Employee { get; set; }

        public static int GetUserId(HttpContextBase context)
        {
            if (context.Session["ID"] == null)
                SetUserInfo(context);
            return (int)context.Session["ID"];
        }

        public static int GetUserRoleId(HttpContextBase context)
        {
            if (context.Session["Role"] == null)
                SetUserInfo(context);
            return (int)context.Session["Role"];
        }

        public static bool DoesUserIdMatch(HttpContextBase context, int employeeId)
        {
            return GetUserId(context) == employeeId;
        }

        public static bool IsContributor(HttpContextBase context)
        {
            return GetUserRoleId(context) == (int)Role.Enum.Contributor;
        }

        public static bool IsAdmin(HttpContextBase context)
        {
            return GetUserRoleId(context) == (int)Role.Enum.Admin;
        }

        public static bool IsAmbassador(HttpContextBase context)
        {
            return GetUserRoleId(context) == (int)Role.Enum.Ambassador;
        }

        public static bool IsMentor(HttpContextBase context)
        {
            return GetUserRoleId(context) == (int)Role.Enum.Mentor;
        }

        private static void SetUserInfo(HttpContextBase context)
        {
            using (ProjectTrackerEntities db = new ProjectTrackerEntities())
            {
                foreach (Employee emp in db.Employees)
                {
                    if (emp.Username == context.User.Identity.Name) // Find logged-in user
                    {
                        SetUserInfo(context, emp);
                        break;
                    }
                }
            }
        }

        public static void SetUserInfo(HttpContextBase context, Employee emp)
        {
            context.Session["ID"] = emp.Emp_Id;
            context.Session["Role"] = emp.Role_Id;
        }

        public class BlockContributorFilter : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                if (Login.IsContributor(filterContext.HttpContext))
                    filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);
                return;
            }
        }

        public class AdminOnlyFilter : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                if (!Login.IsAdmin(filterContext.HttpContext))
                    filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);
                return;
            }
        }

        public class AdminOrAmbassadorOnlyFilter : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                if (!Login.IsAdmin(filterContext.HttpContext) && !Login.IsAmbassador(filterContext.HttpContext))
                    filterContext.Result = new HttpStatusCodeResult(System.Net.HttpStatusCode.Unauthorized);
                return;
            }
        }
    }
}