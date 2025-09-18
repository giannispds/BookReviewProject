namespace BookReviews.Models
{
    public class Book : BaseEntity
    {
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
        public int PublishedYear { get; set; }
        public string? Genre { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
