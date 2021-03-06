//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProjectTracker.Models
{
    using System.ComponentModel.DataAnnotations;
    
    public partial class ProjectEdit
    {
        [Required,
         Display(Name = "Edit ID")]
        public int Edit_Id { get; set; }

        [Required,
         Display(Name = "Project ID")]
        public int Project_Id { get; set; }

        [Required,
         Display(Name = "Editor ID")]
        public int Editor_Id { get; set; }

        [Required,
         Display(Name = "Edit Date")]
        public System.DateTime Date { get; set; }
    
        public virtual Employee Editor { get; set; }
        public virtual Project Project { get; set; }
    }
}
