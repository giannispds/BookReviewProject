using BookReviews.Models.Dao;
using System.ComponentModel.DataAnnotations;

namespace BookReviews.Models
{
    public class Review : BaseEntity
    {
        [Required, MaxLength(1000)]
        public string Content { get; set; } = default!;
        [Range(1, 5)]
        public int Rating { get; set; }
        public Guid BookId { get; set; }
        public Book Book { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;
        public ICollection<ReviewVote> Votes { get; set; } = new List<ReviewVote>();
    }
}
