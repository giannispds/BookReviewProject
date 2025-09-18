using BookReviews.Data;
using BookReviews.Models;
using BookReviews.Models.Dao;
using BookReviews.Models.Dto;
using BookReviews.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookReviews.Controllers.Api
{
    [ApiController]
    [Route("api/reviews")] 
    [Produces("application/json")]
    public class ReviewsApiController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _db;

        public ReviewsApiController(IReviewService reviewService, UserManager<User> userManager, AppDbContext db)
        {
            _reviewService = reviewService;
            _userManager = userManager;
            _db = db;
        }

        [HttpPost]
        [Authorize]     
        public async Task<IActionResult> Create([FromBody] ReviewCreateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var review = await _reviewService.CreateAsync(dto, userId);
                if (review == null) return NotFound(new { message = "Book not found." });
                return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _db.Reviews.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();
            return Ok(r);
        }

        public record VoteDto(bool IsUp);

        [Authorize]
        [HttpPost("{id:guid}/vote")]
        public async Task<IActionResult> Vote(Guid id, [FromBody] VoteDto body)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var review = await _db.Reviews.FindAsync(id);
            if (review == null) return NotFound(new { message = "Review not found." });

            var vote = await _db.ReviewVotes
                .FirstOrDefaultAsync(v => v.ReviewId == id && v.UserId == userId);

            if (vote == null)
            {
                _db.ReviewVotes.Add(new ReviewVote { ReviewId = id, UserId = userId, IsUpvote = body.IsUp });
            }
            else if (vote.IsUpvote == body.IsUp)
            {
                _db.ReviewVotes.Remove(vote); 
            }
            else
            {
                vote.IsUpvote = body.IsUp;
                _db.ReviewVotes.Update(vote);
            }

            await _db.SaveChangesAsync();

            var counts = await _db.ReviewVotes
                .Where(v => v.ReviewId == id)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    upVotes = g.Count(x => x.IsUpvote),
                    downVotes = g.Count(x => !x.IsUpvote)
                })
                .FirstOrDefaultAsync() ?? new { upVotes = 0, downVotes = 0 };

            return Ok(counts);
        }
    }
}
