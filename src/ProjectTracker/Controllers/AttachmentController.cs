using ProjectTracker.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjectTracker.Controllers
{
    public class AttachmentController : Controller
    {
        private ProjectTrackerEntities db = new ProjectTrackerEntities();

        // GET: /Attachment/5
        public PartialViewResult List(int p, bool isEditMode = false)
        {
            Project project = db.Projects.Find(p);
            if (project == null)
                return PartialView(new List<Attachment>());
            IQueryable<Attachment> files = db.Attachments.Where(file => (file.Project_Id == p));
            ViewBag.IsEditMode = isEditMode;
            return PartialView(files.OrderBy(file => file.Name).ToList());
        }

        // GET: /Attachment/Download/5
        public FileResult Download(int? id)
        {
            if (id == null)
                throw new HttpException(404, "Attachment not found");
            Attachment attachment = db.Attachments.Find(id);
            if (attachment == null)
                throw new HttpException(404, "Attachment not found");
            Response.AddHeader("Content-Disposition", "attachment; filename=" + attachment.Name);
            string path = Attachment.GetPath(Server, attachment.Project_Id, attachment.Name);
            return File(path, attachment.Content_Type);
        }

        // POST: /Attachment/Delete/5
        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return HttpNotFound();
            Attachment attachment = db.Attachments.Find(id);
            if (attachment == null)
                return HttpNotFound();
            FileInfo file = new FileInfo(Attachment.GetPath(Server, attachment.Project_Id, attachment.Name));
            file.Delete();
            db.Attachments.Remove(attachment);
            db.SaveChanges();
            return RedirectToAction("List", new { p = attachment.Project_Id, isEditMode = true });
        }
    }
}