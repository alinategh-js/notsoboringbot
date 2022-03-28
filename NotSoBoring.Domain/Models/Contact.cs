using NotSoBoring.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotSoBoring.Domain.Models
{
    public class Contact
    {
        [ForeignKey("User")]
        public long UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey("ContactUser")]
        public long ContactId { get; set; }
        public ApplicationUser ContactUser { get; set; }

        public string ContactName { get; set; }
    }
}
