using BookReviews.Models;
using BookReviews.Models.Dto;
using BookReviews.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookReviews.Controllers.Swaggger
{
    [ApiController]
    [Route("api/books")] 
    [Produces("application/json")]
    public class BooksApiController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IReviewService _reviewService;

        public BooksApiController(IBookService bookService, IReviewService reviewService)
        {
            _bookService = bookService;
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? genre, [FromQuery] int? year, [FromQuery] int? rating)
        {
            var list = await _bookService.GetAllAsync(genre, year, rating, currentUserId: null);
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
   
        public async Task<IActionResult> GetById(Guid id)
        {
            var vm = await _bookService.GetDetailsAsync(id);
            if (vm == null) return NotFound();
            return Ok(vm);
        }

        [HttpGet("{id:guid}/reviews")]
        public async Task<IActionResult> GetReviews(Guid id)
        {
            var reviews = await _reviewService.GetByBookAsync(id);
            return Ok(reviews);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] BookDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var book = await _bookService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
