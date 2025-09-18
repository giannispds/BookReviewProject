using System.ComponentModel.DataAnnotations;

namespace BookReviews.Models.Dto
{
    public class ReviewCreateDto
    {
        [Required]
        public Guid BookId { get; set; }
        [Required, Range(1, 5)] 
        public int Rating { get; set; }
        [Required, MaxLength(1000)]
        public string Content { get; set; } = default!;
    }
}
