namespace BookReviews.Models.Dto
{
    public class BookListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
        public int PublishedYear { get; set; }
        public string Genre { get; set; } = default!;

        public List<ReviewListItemDto> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public ReviewCreateDto NewReview { get; set; } = new();

    }
    public class ReviewListItemDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = default!;
        public int Rating { get; set; }
        public string UserName { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public DateTime Created { get; set; }
        public int UpVotes { get; set; }      
        public int DownVotes { get; set; }
        public bool? MyVote { get; set; }
    }
}
