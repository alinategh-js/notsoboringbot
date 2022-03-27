using NotSoBoring.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotSoBoring.Core.Models
{
    public class ApplicationUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public string UniqueId { get; set; }

        [Display(Name = "نام مستعار")]
        public string Nickname { get; set; }
        [Display(Name = "سن")]
        public int? Age { get; set; }
        [Display(Name = "جنسیت")]
        public GenderTypes? Gender { get; set; }
        [Display(Name = "عکس")]
        public string Photo { get; set; } // file_id of telegram servers
    }
}
