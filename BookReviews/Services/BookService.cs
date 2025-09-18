using BookReviews.Data;
using BookReviews.Models;
using BookReviews.Models.Dto;
using BookReviews.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookReviews.Services
{
    public interface IBookService
    {
        Task<Book> CreateAsync(BookDto dto);
        Task<Book> UpdateAsync(Guid Id,BookDto dto);
        Task<Book> GetByIdAsync(Guid id);
        Task<BookDetailsViewModel?> GetDetailsAsync(Guid id);
        Task<List<BookListItemDto>> GetAllAsync(string? genre, int? year, int? rating, string? currentUserId = null);
    }

    public class BookService : IBookService
    {
        private readonly AppDbContext _dbContext;
        private readonly IRepository _repo;


        public BookService(AppDbContext dbContext, IRepository repo)
        {
            _dbContext = dbContext;
            _repo = repo;
        }

        public async Task<Book> CreateAsync(BookDto dto)
        {
            var existingBook = await _dbContext.Books
                  .AsNoTracking()
                  .FirstOrDefaultAsync(m => m.Title == dto.Title);

            if (existingBook != null)
                throw new InvalidOperationException($"Book already exists (Id={existingBook.Id}).");

            var book = new Book
            {
                Title = dto.Title,
                Author = dto.Author,
                PublishedYear = dto.PublishedYear,
                Genre = dto.Genre,
            };

            return await _repo.CreateAsync(book);
        }

        public async Task<Book?> UpdateAsync(Guid id, BookDto dto)
        {
            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return null;

            book.Title = dto.Title?.Trim() ?? "";
            book.Author = dto.Author?.Trim() ?? "";
            book.PublishedYear = dto.PublishedYear;
            book.Genre = string.IsNullOrWhiteSpace(dto.Genre) ? null : dto.Genre.Trim();

            return await _repo.UpdateAsync(book);
        }

        public async Task<Book?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<BookListItemDto>> GetAllAsync(string? genre, int? year, int? rating, string? currentUserId = null)
        {
            var query = _dbContext.Books
                .Include(b => b.Reviews).ThenInclude(r => r.User)
                .Include(b => b.Reviews).ThenInclude(r => r.Votes)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(genre))
                query = query.Where(b => b.Genre == genre);

            if (year.HasValue)
                query = query.Where(b => b.PublishedYear == year);

            if (rating.HasValue)
                query = query.Where(b => b.Reviews.Any() &&
                                         Math.Round(b.Reviews.Average(r => r.Rating)) == rating);

            return await query
                .OrderBy(b => b.Title)
                .Select(b => new BookListItemDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    PublishedYear = b.PublishedYear,
                    Genre = b.Genre,
                    Reviews = b.Reviews
                        .OrderByDescending(r => r.Created)
                        .Select(r => new ReviewListItemDto
                        {
                            Id = r.Id,
                            Rating = r.Rating,
                            Content = r.Content,
                            UserName = r.User.UserName,
                            Created = r.Created,
                            UpVotes = r.Votes.Count(v => v.IsUpvote),
                            DownVotes = r.Votes.Count(v => !v.IsUpvote),
                            MyVote = currentUserId == null
                        ? (bool?)null
                        : r.Votes
                            .Where(v => v.UserId == currentUserId)
                            .Select(v => (bool?)v.IsUpvote) 
                            .FirstOrDefault()

                        }).ToList(),
                    AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = b.Reviews.Count,
                    NewReview = new ReviewCreateDto { BookId = b.Id }
                })
                .ToListAsync();
        }
        public async Task<BookDetailsViewModel?> GetDetailsAsync(Guid id)
        {
            var book = await _dbContext.Books
                .Include(b => b.Reviews).ThenInclude(r => r.User)
                .Include(b => b.Reviews).ThenInclude(r => r.Votes)   
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return null;

            return new BookDetailsViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                PublishedYear = book.PublishedYear,
                Genre = book.Genre,
                Reviews = book.Reviews
                    .OrderByDescending(r => r.Created)
                    .Select(r => new ReviewListItemDto
                    {
                        Id = r.Id,
                        Content = r.Content,
                        Rating = r.Rating,
                        UserName = r.User != null ? r.User.UserName : "(unknown)",
                        Created = r.Created
                    })
                    .ToList(),
                NewReview = new ReviewCreateDto { BookId = book.Id }
            };
        }
    }

}
