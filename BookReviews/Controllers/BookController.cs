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
    public class BookController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IReviewService _reviewService;
        private readonly AppDbContext _dbContext;
        private readonly UserManager<User> _userManager;



        public BookController(IBookService bookService, IReviewService reviewService, AppDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _bookService = bookService;
            _reviewService = reviewService;
            _userManager = userManager;
        }

        private static List<BookListItemDto> _books = new List<BookListItemDto>();

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AdminCrud()
        {
            var books = await _bookService.GetAllAsync(null, null, null);
            return View(books);
        }
        public async Task<IActionResult> Index(string? genre, int? year, int? rating)
        {
            var currentUserId = _userManager.GetUserId(User);
            var books = await _bookService.GetAllAsync(genre, year, rating, currentUserId);
            return View(books);
        }

        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(BookDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                var book = await _bookService.CreateAsync(dto);

                if (book == null)
                {
                    ModelState.AddModelError(string.Empty, "Book already exists.");
                    return View(dto);
                }

                return RedirectToAction(nameof(AdminCrud));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Something went wrong: " + ex.Message);
                return View(dto);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var book = await _bookService.GetByIdAsync(id);
            if (book == null) return NotFound();

            var dto = new BookDto
            {
                Title = book.Title,
                Author = book.Author,
                PublishedYear = book.PublishedYear,
                Genre = book.Genre
            };

            ViewBag.Id = id;
            return View(dto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BookDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Id = id;
                return View(dto);
            }

            try
            {
                var updated = await _bookService.UpdateAsync(id, dto);
                if (updated == null) return NotFound();

                TempData["Success"] = "Book updated.";
                return RedirectToAction(nameof(AdminCrud)); 
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Something went wrong: " + ex.Message);
                ViewBag.Id = id;
                return View(dto);
            }
        }
    }
}
