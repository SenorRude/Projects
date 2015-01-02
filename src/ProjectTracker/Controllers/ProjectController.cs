using PagedList;
using ProjectTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ProjectTracker.Controllers
{
    public class ProjectController : Controller
    {
        private ProjectTrackerEntities db = new ProjectTrackerEntities();

        // GET: /Project/
        public ActionResult Index(int? p, int? ps,
                                  int? usr, int? sch, int? stat,
                                  string q, string auth, string sort,
                                  int? my, int? mysch, int? arch)
        {
            IQueryable<Project> projects = db.Projects;
            Employee thisEmp = db.Employees.Find(Login.GetUserId(HttpContext));
            ViewBag.ThisEmployee = thisEmp;

            // Keyword search
            if (!string.IsNullOrWhiteSpace(q))
                projects = projects.Where(proj => (proj.Title.Contains(q)
                                                || proj.Short_Description.Contains(q)
                                                || proj.Description.Contains(q)
                                                || proj.Justification.Contains(q)
                                                || proj.Notes.Contains(q)));
            // Author contains
            if (!string.IsNullOrWhiteSpace(auth))
                projects = projects.Where(proj => (proj.Author.FName.Contains(auth)
                                                || proj.Author.LName.Contains(auth)
                                                || proj.Author.Username.Contains(auth)));

            // My Ideas, My Schools, Show Archived
            if (Convert.ToBoolean(my))
                projects = projects.Where(proj => (proj.Author_Id == thisEmp.Emp_Id));
            if (Convert.ToBoolean(mysch)
                && (thisEmp.Role.Is(Role.Enum.Ambassador) || thisEmp.Role.Is(Role.Enum.Mentor)))
            {
                if (thisEmp != null)
                {
                    List<int> schoolIDs = thisEmp.Schools.Select(s => s.School_Id).ToList();
                    projects = projects.Where(proj => schoolIDs.Contains(proj.School_Id));
                }
            }
            if (!Convert.ToBoolean(arch))
                projects = projects.Where(proj => (proj.Status_Id != (int)Status.Enum.Archived)); // LINQ rejects Status.Is()

            // Specific user, school, and/or status
            ViewBag.stat = new SelectList(db.Status, "Status_Id", "Name");
            ViewBag.sch = new SelectList(db.Schools, "School_Id", "Short_Name");
            if (usr != null)
                projects = projects.Where(proj => (proj.Author_Id == usr));
            if (sch != null)
                projects = projects.Where(proj => (proj.School_Id == sch));
            if (stat != null)
                projects = projects.Where(proj => (proj.Status_Id == stat));

            // Column heading URLs
            ViewBag.SortVotesUrl = GetSortUrl("v");
            ViewBag.SortTitleUrl = GetSortUrl("t");
            ViewBag.SortCreatedUrl = GetSortUrl("c");
            ViewBag.SortEditedUrl = GetSortUrl("e", true);

            // Sorting
            switch (sort)
            {
                case "v":  // Votes
                    projects = projects.OrderBy(proj => proj.Voters.Count);
                    break;
                case "vd": // Votes descending 
                    projects = projects.OrderByDescending(proj => proj.Voters.Count);
                    break;
                case "t":  // Title
                    projects = projects.OrderBy(proj => proj.Title);
                    break;
                case "td": // Title descending
                    projects = projects.OrderByDescending(proj => proj.Title);
                    break;
                case "c":  // Created
                    projects = projects.OrderBy(proj => proj.Submit_Date);
                    break;
                case "cd": // Created descending
                    projects = projects.OrderByDescending(proj => proj.Submit_Date);
                    break;
                case "a":  // Author
                    projects = projects.OrderBy(proj => proj.Author.FName);
                    break;
                case "ad": // Author descending
                    projects = projects.OrderByDescending(proj => proj.Author.FName);
                    break;
                case "e":  // Edited
                    projects = projects.AsEnumerable().OrderBy(
                            proj => (proj.LastEdit != null ? proj.LastEdit.Date : proj.Submit_Date)).AsQueryable();
                    break;
                default:   // Edited descending
                    projects = projects.AsEnumerable().OrderByDescending(
                        proj => (proj.LastEdit != null ? proj.LastEdit.Date : proj.Submit_Date)).AsQueryable();
                    break;
            }

            // Pagination
            switch (ps) // Page size
            {
                case 5:
                case 10:
                case 25:
                case 50:
                    break;
                default:
                    ps = 10;
                    break;
            }
            int pgCount = (int)Math.Ceiling(projects.Count() / (float)ps);
            if (p > pgCount)
                p = pgCount;
            SelectList psList = new SelectList(new SelectListItem[]
            {
                new SelectListItem { Value = "5", Text = "5" },
                new SelectListItem { Value = null, Text = "10" },
                new SelectListItem { Value = "25", Text = "25" },
                new SelectListItem { Value = "50", Text = "50" }
            }, "Value", "Text", selectedValue: (ps == 10 ? string.Empty : ps.ToString()));
            ViewBag.ps = psList;
            
            return View(projects.ToPagedList(p ?? 1, ps ?? 10));
        }

        // GET: /Project/Archive
        [Login.AdminOnlyFilter]
        public ActionResult Archive()
        {
            return RedirectToAction("Index", new { stat = (int)Status.Enum.Archived, arch = 1, opt = 1 });
        }

        public int Vote(int p)
        {
            Project project = db.Projects.Include(proj => proj.Voters).Single(proj => (proj.Project_Id == p));
            if (project == null)
                throw new HttpException("Couldn't find project");
            Employee thisEmp = db.Employees.Find(Login.GetUserId(HttpContext));
            if (thisEmp == null)
                throw new HttpException("Couldn't find logged in user");
            
            if (project.Voters.Contains(thisEmp))
                project.Voters.Remove(thisEmp);
            else
                project.Voters.Add(thisEmp);
            db.Entry(project).State = EntityState.Modified;
            db.SaveChanges();
            return project.VoteCount;
        }

        // GET: /Project/Votes/5
        public ActionResult Votes(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            Project project = db.Projects.Find(id);
            if (project == null)
                return HttpNotFound();
            return View(project);
        }

        // GET: /Project/Details/5
        public ActionResult Details(int? id, bool isDeleting = false)
        {
            if (id == null)
                return RedirectToAction("Index");
            Project project = db.Projects.Find(id);
            if (project == null)
                return HttpNotFound();
            ViewBag.CanEdit = db.Employees.Find(Login.GetUserId(HttpContext)).CanEdit(project);
            if (isDeleting)
                ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            return View(project);
        }

        // GET: /Project/History/5
        public ActionResult History(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            Project project = db.Projects.Find(id);
            if (project == null)
                return HttpNotFound();
            ViewBag.ProjectID = project.Project_Id;
            ViewBag.ProjectTitle = project.Title;
            return View(project.Edits.OrderByDescending(e => e.Date));
        }

        // GET: /Project/Delete/5
        [Login.AdminOnlyFilter]
        public ActionResult Delete(int? id)
        {
            return RedirectToAction("Details", new { id = id, isDeleting = true });
        }

        // POST: /Project/Details/5
        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        [Login.AdminOnlyFilter]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects
                .Include(p => p.Attachments)
                .Include(p => p.Edits)
                .Include(p => p.Voters)
                .Single(p => (p.Project_Id == id));
            project.DeleteDependents(Server, db);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: /Project/Submit
        public ActionResult Submit()
        {
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            return View(new Project());
        }

        // POST: /Project/Submit
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(
            [Bind(Include = "Title,Short_Description,Description,Justification,Notes")] Project project,
            IEnumerable<HttpPostedFileBase> attachments)
        {
            ModelState.Remove("Status"); // God DAMN stop including Status when I don't say so
            ModelState.Remove("attachments");
            if (ModelState.IsValid)
            {
                project.Author_Id = Login.GetUserId(HttpContext);
                project.School_Id = School.UNASSIGNED_ID;
                project.Status_Id = (int)Status.Enum.Submitted;
                project.Submit_Date = DateTime.Now; // ### Potential timezone issue
                db.Projects.Add(project);
                db.SaveChanges();
                if (ProcessAttachments(project, attachments) > 0)
                    db.SaveChanges();
                return RedirectToAction("Details", new { id = project.Project_Id });
            }
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            return View(project);
        }

        // GET: /Project/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");
            Project project = db.Projects.Find(id);
            if (project == null)
                return HttpNotFound();
            bool canEdit = db.Employees.Find(Login.GetUserId(HttpContext)).CanEdit(project);
            if (!canEdit)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            SetViewBagForSchoolChoices(project);
            SetViewBagForStatusChoices(project);
            return View(project);
        }

        // POST: /Project/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = @"Project_Id,
                              Title,Short_Description,Description,Justification,
                              Notes,Status_Id,School_Id,
                              Author_Id,Submit_Date")] Project project,
            IEnumerable<HttpPostedFileBase> attachments)
        {
            bool canEdit = db.Employees.Find(Login.GetUserId(HttpContext)).CanEdit(project);
            if (!canEdit)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                project = db.Projects
                    .Include(p => p.Edits)
                    .Include(p => p.Attachments)
                    .Single(p => (p.Project_Id == project.Project_Id));
                project.Edits.Add(new ProjectEdit
                {
                    Project_Id = project.Project_Id,
                    Editor_Id = Login.GetUserId(HttpContext),
                    Date = DateTime.Now // ### Potential timezone issue
                });
                ProcessAttachments(project, attachments);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = project.Project_Id });
            }
            ViewBag.CancelUrl = Helpers.GetCancelUrl(Request, Url);
            SetViewBagForSchoolChoices(project);
            SetViewBagForStatusChoices(project);
            return View(project);
        }

        private int ProcessAttachments(Project project, IEnumerable<HttpPostedFileBase> attachments)
        {
            if (attachments.Count() == 0)
                return 0;
            int uploadCount = 0;
            foreach (HttpPostedFileBase upload in attachments)
            {
                if (upload == null || upload.ContentLength == 0)
                    continue;
                string fileName = Path.GetFileName(upload.FileName);
                string projectDir = Server.MapPath("~/App_Data/Attachments/" + project.Project_Id + "/");
                if (!Directory.Exists(projectDir))
                    Directory.CreateDirectory(projectDir);
                string fileSavePath = (projectDir + fileName);
                upload.SaveAs(fileSavePath);
                Attachment attachment = new Attachment
                {
                    Name = fileName,
                    Project_Id = project.Project_Id,
                    Uploader_Id = Login.GetUserId(HttpContext),
                    Upload_Date = DateTime.Now,
                    Content_Type = upload.ContentType
                };
                project.Attachments.Add(attachment);
                ++uploadCount;
            }
            return uploadCount;
        }

        private void SetViewBagForSchoolChoices(Project project = null)
        {
            if (project == null)
                ViewBag.School_Id = new SelectList(db.Schools, "School_Id", "Name");
            else
                ViewBag.School_Id = new SelectList(db.Schools, "School_Id", "Name", project.School_Id);
        }

        private void SetViewBagForStatusChoices(Project project = null)
        {
            if (project == null)
                ViewBag.Status_Id = new SelectList(db.Status, "Status_Id", "Name");
            else
                ViewBag.Status_Id = new SelectList(db.Status, "Status_Id", "Name", project.Status_Id);
        }

        private string GetSortUrl(string sortHeadingName, bool isDefaultSort = false)
        {
            // NameValueCollection.ToString() doesn't return a proper query string. However,
            // HttpUtility.ParseQueryString() actually returns an HttpValueCollection (private type), and
            // HttpValueCollection.ToString() DOES return a proper query string.
            System.Collections.Specialized.NameValueCollection qs =
                System.Web.HttpUtility.ParseQueryString(Request.QueryString.ToString());

            switch (sortHeadingName)
            {
                case "v":  // Votes
                    qs["sort"] = (qs["sort"] == sortHeadingName ? "vd" : "v");
                    break;
                case "t":  // Title
                    qs["sort"] = (qs["sort"] == sortHeadingName ? "td" : "t");
                    break;
                case "c":  // Created
                    qs["sort"] = (qs["sort"] == sortHeadingName ? "cd" : "c");
                    break;
                default:   // Edited
                    if (qs["sort"] == "e")
                        qs.Remove("sort");
                    else
                        qs["sort"] = "e";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(qs.ToString()))
                return Url.Action("Index") + "?" + qs.ToString();
            else
                return Url.Action("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
