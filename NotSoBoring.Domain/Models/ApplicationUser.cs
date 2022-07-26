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

        [Display(Name = "Nickname")]
        public string Nickname { get; set; }
        [Display(Name = "Age")]
        public int? Age { get; set; }
        [Display(Name = "Gender")]
        public GenderTypes? Gender { get; set; }
        [Display(Name = "Photo")]
        public string Photo { get; set; } // file_id of telegram servers

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
