using NotSoBoring.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotSoBoring.Domain.Models
{
    public class ApplicationContact
    {
        public long UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public long ContactId { get; set; }
        [ForeignKey("ContactId")]
        public ApplicationUser ContactUser { get; set; }

        public string ContactName { get; set; }
    }
}
