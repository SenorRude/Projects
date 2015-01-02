using ProjectTracker.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace ProjectTracker.Views
{
    public class EmployeeController : Controller
    {
        private ProjectTrackerEntities db = new ProjectTrackerEntities();

        // GET: /Employee/
        [Login.AdminOnlyFilter]
        public ActionResult Index()
        {
            return View(db.Employees.ToList());
        }

        // GET: /Employee/Details/5
        public ActionResult Details(int? id, bool isDeleting = false)
        {
            if (id == null)
                return RedirectToAction("Index");
            Employee employee = db.Employees.Find(id);
            if (employee == null)
                return HttpNotFound();
            if (isDeleting)
                ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            return View(employee);
        }

        // GET: /Employee/Votes/5
        public ActionResult Votes(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            Employee employee = db.Employees.Find(id);
            if (employee == null)
                return HttpNotFound();
            return View(employee);
        }

        // GET: /Employee/Delete/5
        [Login.AdminOnlyFilter]
        public ActionResult Delete(int? id)
        {
            return RedirectToAction("Details", new { id = id, isDeleting = true });
        }

        // POST: /Employee/Details/5
        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        [Login.AdminOnlyFilter]
        public ActionResult DeleteConfirmed(int id)
        {
            Employee employee = db.Employees.Find(id);
            db.Employees.Remove(employee);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: /Employee/Create
        [Login.AdminOnlyFilter]
        public ActionResult Create()
        {
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            SetViewBag();
            return View();
        }

        // POST: /Employee/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Login.AdminOnlyFilter]
        public ActionResult Create([Bind(Include="Username,Password,FName,LName,Role_Id")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                db.Employees.Add(employee);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = employee.Emp_Id });
            }
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            SetViewBag(employee);
            return View(employee);
        }

        // GET: /Employee/Edit/5
        [Login.AdminOnlyFilter]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            Employee employee = db.Employees.Find(id);
            if (employee == null)
                return HttpNotFound();
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            SetViewBag(employee);
            return View(employee);
        }

        // POST: /Employee/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Login.AdminOnlyFilter]
        public ActionResult Edit([Bind(Include="Emp_Id,Username,Password,FName,LName,Role_Id")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = employee.Emp_Id });
            }
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            SetViewBag(employee);
            return View(employee);
        }

        private void SetViewBag(Employee emp = null)
        {
            if (emp == null)
                ViewBag.Role_Id = new SelectList(db.Roles, "Role_Id", "Title");
            else
                ViewBag.Role_Id = new SelectList(db.Roles, "Role_Id", "Title", emp.Role_Id);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
