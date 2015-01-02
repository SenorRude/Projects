using ProjectTracker.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;

namespace ProjectTracker.Views
{
    [Login.BlockContributorFilter]
    public class SchoolController : Controller
    {
        private ProjectTrackerEntities db = new ProjectTrackerEntities();

        // GET: /School/
        public ActionResult Index()
        {
            return View(db.Schools.ToList());
        }

        // GET: /School/Details/5
        public ActionResult Details(int? id, bool isDeleting = false)
        {
            if (id == null)
                return RedirectToAction("Index");
            School school = db.Schools.Find(id);
            if (school == null)
                return HttpNotFound();
            if (isDeleting)
                ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            return View(school);
        }

        // GET: /School/Delete/5
        [Login.AdminOnlyFilter]
        public ActionResult Delete(int? id)
        {
            return RedirectToAction("Details", new { id = id, isDeleting = true });
        }

        // POST: /School/Details/5
        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        [Login.AdminOnlyFilter]
        public ActionResult DeleteConfirmed(int id)
        {
            School school = db.Schools.Find(id);
            if (school.Employees.Count > 0)
            {
                school = db.Schools.Include(s => s.Employees).Single(s => (s.School_Id == id));
                school.Employees = new List<Employee>(); // Remove any ambassador/mentor assignments
                db.SaveChanges();
            }
            db.Schools.Remove(school);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: /School/Add
        [Login.AdminOnlyFilter]
        public ActionResult Add()
        {
            SetAmbassadorAndMentorChoices();
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            return View();
        }

        // POST: /School/Add
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Login.AdminOnlyFilter]
        public ActionResult Add([Bind(Include = @"Name,Short_Name,
                                                  Contact_Name,Contact_Title,Email,Phone,Fax,
                                                  Street,City,State,Zip")] School school)
        {
            if (ModelState.IsValid)
            {
                school.Employees = Helpers.GetSelectedEmployeesForSchool(Request, db.Employees);
                db.Schools.Add(school);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = school.School_Id });
            }
            SetAmbassadorAndMentorChoices();
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            return View(school);
        }

        // GET: /School/Edit/5
        [Login.AdminOrAmbassadorOnlyFilter]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            School school = db.Schools.Find(id);
            if (school == null)
                return HttpNotFound();
            SetAmbassadorAndMentorChoices(
                Helpers.GetEmployeeIDs(school.Employees, Role.Enum.Ambassador),
                Helpers.GetEmployeeIDs(school.Employees, Role.Enum.Mentor));
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            return View(school);
        }

        // POST: /School/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Login.AdminOrAmbassadorOnlyFilter]
        public ActionResult Edit([Bind(Include=@"School_Id,Name,Short_Name,
                                                 Contact_Name,Contact_Title,Email,Phone,Fax,
                                                 Street,City,State,Zip")] School school)
        {
            if (ModelState.IsValid)
            {
                db.Entry(school).State = EntityState.Modified;
                db.SaveChanges();
                school = db.Schools.Include(s => s.Employees).Single(s => (s.School_Id == school.School_Id));
                school.Employees = Helpers.GetSelectedEmployeesForSchool(Request, db.Employees);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = school.School_Id });
            }
            SetAmbassadorAndMentorChoices(
                Helpers.GetEmployeeIDs(school.Employees, Role.Enum.Ambassador),
                Helpers.GetEmployeeIDs(school.Employees, Role.Enum.Mentor));
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            return View(school);
        }

        private void SetAmbassadorAndMentorChoices(
            List<int> selectedAmbassadorIDs = null,
            List<int> selectedMentorIDs = null)
        {
            DbQuery<Employee> ambassadors = (DbQuery<Employee>)db.Employees
                .Where(e => e.Role_Id == (int)Role.Enum.Ambassador)
                .OrderBy(e => e.FName);
            DbQuery<Employee> mentors = (DbQuery<Employee>)db.Employees
                .Where(e => e.Role_Id == (int)Role.Enum.Mentor)
                .OrderBy(e => e.FName);
            if (selectedAmbassadorIDs == null)
                ViewBag.Ambassadors = new MultiSelectList(ambassadors, "Emp_Id", "FullName");
            else
                ViewBag.Ambassadors = new MultiSelectList(ambassadors, "Emp_Id", "FullName", selectedAmbassadorIDs);
            if (selectedMentorIDs == null)
                ViewBag.Mentors = new MultiSelectList(mentors, "Emp_Id", "FullName");
            else
                ViewBag.Mentors = new MultiSelectList(mentors, "Emp_Id", "FullName", selectedMentorIDs);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
