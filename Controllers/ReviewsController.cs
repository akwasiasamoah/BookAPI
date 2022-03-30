using BookApi_Project.Dtos;
using BookApi_Project.Models;
using BookApi_Project.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookApi_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : Controller
    {
        private IReviewerRepository _reviewerRepository;
        private IReviewRepository _reviewRepository;
        private IBookRepository _bookRepository;

        public ReviewsController(IReviewRepository reviewRepository, IBookRepository bookRepository, IReviewerRepository reviewerRepository)
        {
            _reviewRepository = reviewRepository;
            _bookRepository = bookRepository;
            _reviewerRepository = reviewerRepository;
        }

        //api/reviews
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof (IEnumerable<ReviewDto>))]
        public IActionResult GetReviews()
        {
            var reviews = _reviewRepository.GetReviews();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reviewsDto = new List<ReviewDto>();
            foreach(var review in reviews)
            {
                reviewsDto.Add(new ReviewDto()
                {
                    Id = review.Id,
                    Headline = review.Headline,
                    Rating = review.Rating,
                    ReviewText = review.ReviewText
                });
            }

            return Ok(reviewsDto);
        }

        //api/reviews/reviewId
        [HttpGet("{reviewId}", Name = "GetReview")]  
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof (ReviewDto))]
        public IActionResult GetReview(int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
                return NotFound();

            var review = _reviewRepository.GetReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reviewDto = new ReviewDto()
            {
                Id = review.Id,
                Headline = review.Headline,
                Rating = review.Rating,
                ReviewText = review.ReviewText
            };

            return Ok(reviewDto);
        }

        //TO DO - need to test it after I implemet IBook repository
        //api/reviews/books/bookId //using this
        //api/reviews/bookId/reviews
        [HttpGet("books/{bookId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof (IEnumerable<ReviewDto>))]
        public IActionResult GetReviewsOfABook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
                return NotFound();

            var reviews = _reviewRepository.GetReviewsOfABook(bookId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reviewsDto = new List<ReviewDto>();
            foreach(var review in reviews)
            {
                reviewsDto.Add(new ReviewDto()
                {
                    Id = review.Id,
                    Headline = review.Headline,
                    Rating = review.Rating,
                    ReviewText = review.ReviewText
                });
            }

            return Ok(reviewsDto);
        }

        //api/reviews/reviewId/book
        [HttpGet("{reviewId}/book")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof (BookDto))]
        public IActionResult GetBookOfAReview(int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
                NotFound();

            var book = _reviewRepository.GetBookOfAReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var bookDto = new BookDto()
            {
                Id = book.Id,
                Isbn = book.Isbn,
                Title = book.Title,
                DatePublished = book.DatePublished
            };

            return Ok(bookDto);
        }

        //api/reviews
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Review))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult CreateReview([FromBody]Review reviewToCreate)
        {
            if (reviewToCreate == null)
                return BadRequest(ModelState);

            if (!_reviewerRepository.ReviewerExists(reviewToCreate.Reviewer.Id))
                ModelState.AddModelError("", "Reviewer doesn't exist");
            if (!_bookRepository.BookExists(reviewToCreate.Book.Id))
                ModelState.AddModelError("", "Book doesn't exist");

            if (!ModelState.IsValid)
                return StatusCode(404, ModelState);

            reviewToCreate.Book = _bookRepository.GetBook(reviewToCreate.Book.Id);
            reviewToCreate.Reviewer = _reviewerRepository.GetReviewer(reviewToCreate.Reviewer.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_reviewRepository.CreateReview(reviewToCreate))
            {
                ModelState.AddModelError("", $"Something went wrong saving the review");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetReview", new { reviewId = reviewToCreate.Id }, reviewToCreate);
        }

        //api/reviews/reviewId
        [HttpPut("{reviewId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult UpdateReview(int reviewId, [FromBody]Review reviewToUpdate)
        {
            if (reviewToUpdate == null)
                return BadRequest(ModelState);

            if (reviewId != reviewToUpdate.Id)
                return BadRequest(ModelState);

            if (!_reviewRepository.ReviewExists(reviewId))
                ModelState.AddModelError("", "Review does not exist");

            if (!_reviewerRepository.ReviewerExists(reviewToUpdate.Reviewer.Id))
                ModelState.AddModelError("", "Reviewer doesn't exist");
            if (!_bookRepository.BookExists(reviewToUpdate.Book.Id))
                ModelState.AddModelError("", "Book doesn't exist");

            reviewToUpdate.Book = _bookRepository.GetBook(reviewToUpdate.Book.Id);
            reviewToUpdate.Reviewer = _reviewerRepository.GetReviewer(reviewToUpdate.Reviewer.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_reviewRepository.UpdateReview(reviewToUpdate))
            {
                ModelState.AddModelError("", $"Something went wrong updating the review");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        //api/reviews/reviewId
        [HttpDelete("{reviewId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult DeleteReview(int reviewId)
        {
            if (!_reviewRepository.ReviewExists(reviewId))
                return NotFound();

            var reviewToDelete = _reviewRepository.GetReview(reviewId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_reviewRepository.DeleteReview(reviewToDelete))
            {
                ModelState.AddModelError("", $"Something happened trying to deleting the review ");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
