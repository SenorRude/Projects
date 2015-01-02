using ProjectTracker.Models;
using System.Linq;
using System.Web.Mvc;

namespace ProjectTracker.Views
{
    [Login.BlockContributorFilter]
    public class StatusController : Controller
    {
        private ProjectTrackerEntities db = new ProjectTrackerEntities();

        // GET: /Status/
        public ActionResult Index()
        {
            return View(db.Status.ToList());
        }

        // GET: /Status/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            Status status = db.Status.Find(id);
            if (status == null)
                return HttpNotFound();
            return View(status);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}