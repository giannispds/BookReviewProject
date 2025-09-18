using BookReviews.Data;
using BookReviews.Models;
using BookReviews.Models.Dao;
using BookReviews.Models.Dto;
using BookReviews.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookReviews.Controllers
{
    [Authorize] 
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _dbContext;



        public ReviewsController(IReviewService reviewService, UserManager<User> userManager, AppDbContext dbContext)
        {
            _reviewService = reviewService;
            _userManager = userManager;
            _dbContext = dbContext;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(ReviewCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction("Index", "Book");
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "You must be logged in to submit a review.";
                return RedirectToAction("Index", "Book");
            }
            try
            {
                var review = await _reviewService.CreateAsync(dto, userId);
                TempData["Success"] = "Review submitted successfully!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
            }

            return RedirectToAction("Index", "Book");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(Guid reviewId, bool isUp, string? returnUrl)
        {
            var uid = _userManager.GetUserId(User);
            var fallback = Url.Action("Index", "Book")!;
            var back = (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) ? returnUrl : fallback;

            if (uid == null)
            {
                TempData["Error"] = "You must be logged in to vote.";
                return LocalRedirect(back);
            }

            var reviewExists = await _dbContext.Reviews.AnyAsync(r => r.Id == reviewId);
            if (!reviewExists)
            {
                TempData["Error"] = "Review not found.";
                return LocalRedirect(back);
            }

            var vote = await _dbContext.ReviewVotes
                .FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == uid);

            if (vote == null)
            {
                _dbContext.ReviewVotes.Add(new ReviewVote { ReviewId = reviewId, UserId = uid, IsUpvote = isUp });
                TempData[isUp ? "Success" : "Warning"] =
                    isUp ? "You liked this review." : "You disliked this review.";
            }
            else if (vote.IsUpvote == isUp)
            {
                _dbContext.ReviewVotes.Remove(vote);
                TempData["Info"] = "Your vote was removed.";
            }
            else
            {
                vote.IsUpvote = isUp;
                _dbContext.ReviewVotes.Update(vote);
                TempData[isUp ? "Success" : "Secondary"] =
                    isUp ? "Changed to Like." : "Changed to Dislike.";
            }

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                TempData["Error"] = "Could not save your vote.";
            }
            return LocalRedirect(back);
        }

    }
}
