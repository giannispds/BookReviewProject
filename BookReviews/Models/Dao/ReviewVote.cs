using BookReviews.Models.Dao;

namespace BookReviews.Models
{
    public class ReviewVote : BaseEntity
    {
        public Guid ReviewId { get; set; }
        public Review Review { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;
        public bool IsUpvote { get; set; }
    }
}
