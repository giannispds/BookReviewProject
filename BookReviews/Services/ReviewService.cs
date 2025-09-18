using BookReviews.Data;
using BookReviews.Models;
using BookReviews.Models.Dto;
using BookReviews.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookReviews.Services
{
    public interface IReviewService
    {
        Task<Review?> CreateAsync(ReviewCreateDto dto, string userId);
        Task<List<ReviewListItemDto>> GetByBookAsync(Guid bookId);

    }

    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _dbContext;
        private readonly IRepository _repo;

        public ReviewService(AppDbContext dbContext, IRepository repo)
        {
            _dbContext = dbContext;
            _repo = repo;
        }

        public async Task<Review?> CreateAsync(ReviewCreateDto dto, string userId)
        {
            var bookExists = await _dbContext.Books.AnyAsync(b => b.Id == dto.BookId);
            if (!bookExists)
                throw new InvalidOperationException("Book not found.");

            var userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
                throw new InvalidOperationException("User not found. Please log in again.");

            var already = await _dbContext.Reviews
                .AnyAsync(r => r.BookId == dto.BookId && r.UserId == userId);
            if (already)
                throw new InvalidOperationException("You have already submitted a review for this book.");

            var review = new Review
            {
                BookId = dto.BookId,
                Rating = dto.Rating,
                Content = dto.Content,
                UserId = userId
            };

            return await _repo.CreateAsync(review);
        }

        public async Task<List<ReviewListItemDto>> GetByBookAsync(Guid bookId)
        {
            return await _dbContext.Reviews
                .Where(r => r.BookId == bookId)
                .Include(r => r.User)
                .OrderByDescending(r => r.Created)
                .Select(r => new ReviewListItemDto
                {
                    Id = r.Id,
                    Content = r.Content,
                    Rating = r.Rating,
                    UserName = r.User.UserName,
                    Created = r.Created
                })
                .ToListAsync();
        }
    }
}
