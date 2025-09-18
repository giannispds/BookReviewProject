namespace BookReviews.Models.Dto
{
    public class BookDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
        public int PublishedYear { get; set; }
        public string? Genre { get; set; }
        public List<ReviewListItemDto> Reviews { get; set; } = new();
        public ReviewCreateDto NewReview { get; set; } = new();
    }
}
