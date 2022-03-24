using NotSoBoring.Core.Enums;

namespace NotSoBoring.Domain.DTOs
{
    public class MatchRequest
    {
        public long UserId { get; set; }
        public GenderTypes? PreferredGender { get; set; }
        public bool IsCancelled { get; set; } = false;
    }
}