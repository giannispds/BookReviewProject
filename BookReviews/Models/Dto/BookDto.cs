using System.ComponentModel.DataAnnotations;

namespace BookReviews.Models.Dto
{
    public class BookDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = default!;

        [Required, MaxLength(120)]
        public string Author { get; set; } = default!;

        [Range(0, 2100)]
        public int PublishedYear { get; set; }

        [MaxLength(60)]
        public string? Genre { get; set; }
    }
}
