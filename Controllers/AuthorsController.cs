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
    public class AuthorsController : Controller
    {
        private IAuthorRepository _authorRepository;
        private IBookRepository _bookRepository;
        private ICountryRepository _countryRepository;

        public AuthorsController(IAuthorRepository authorRepository, IBookRepository bookRepository, ICountryRepository countryRepository)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
            _countryRepository = countryRepository;
        }

        //api/authors
        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof (IEnumerable<AuthorDto>))]
        public IActionResult GetAuthors()
        {
            var authors = _authorRepository.GetAuthors();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authorsDto = new List<AuthorDto>();
            foreach(var author in authors)
            {
                authorsDto.Add(new AuthorDto()
                {
                    Id = author.Id,
                    FirstName = author.FirstName,
                    LastName = author.LastName
                });
            }

            return Ok(authorsDto);
        }

        //api/authors/authorId
        [HttpGet("{authorId}", Name = "GetAuthor")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof (AuthorDto))]
        public IActionResult GetAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
                return NotFound();

            var author = _authorRepository.GetAuthor(authorId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authorDto = new AuthorDto() 
            { 
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            };

            return Ok(authorDto);
        }

        //api/authors/authorId/books
        [HttpGet("{authorId}/books")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof (IEnumerable<BookDto>))]
        public IActionResult GetBooksByAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
                return NotFound();

            var books = _authorRepository.GetBooksByAuthor(authorId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var booksDto = new List<BookDto>();
            foreach(var book in books)
            {
                booksDto.Add(new BookDto()
                {
                    Id = book.Id,
                    Isbn = book.Isbn,
                    Title = book.Title,
                    DatePublished = book.DatePublished
                });
            }

            return Ok(booksDto);
        }

        // TO DO - Retest after we implement the book repository
        //api/authors/bookId/authors
        [HttpGet("{bookId}/authors")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]

        public IActionResult GetAuthorsOfABook(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
                return NotFound();

            var authors = _authorRepository.GetAuthorsOfABook(bookId);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var authorsDto = new List<AuthorDto>();
            foreach(var author in authors)
            {
                authorsDto.Add(new AuthorDto() 
                { 
                    Id = author.Id,
                    FirstName = author.FirstName,
                    LastName = author.LastName
                });
            }
            return Ok(authorsDto);
        }

        //api/authors
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Author))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult CreateAuthor([FromBody]Author authorToCreate)
        {
            if (authorToCreate == null)
                return BadRequest(ModelState);

            if (!_countryRepository.CountryExists(authorToCreate.Country.Id))
            {
                ModelState.AddModelError("", "Country doesn't exist");
                return StatusCode(404, ModelState);
            }
            authorToCreate.Country = _countryRepository.GetCountry(authorToCreate.Country.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_authorRepository.CreateAuthor(authorToCreate))
            {
                ModelState.AddModelError("", $"Something went wrong saving {authorToCreate.FirstName} {authorToCreate.LastName}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetAuthor", new { authorId = authorToCreate.Id }, authorToCreate);
        }

        //api/authors/authorId
        [HttpPut("{authorId}")]        
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult UpdateAuthor(int authorId, [FromBody]Author updatedAuthorInfo)
        {
            if (updatedAuthorInfo == null)
                return BadRequest(ModelState);

            if (authorId != updatedAuthorInfo.Id)
                return BadRequest(ModelState);

            if (!_authorRepository.AuthorExists(authorId))
                ModelState.AddModelError("", "Author doesn't exist");

            if (!_countryRepository.CountryExists(updatedAuthorInfo.Country.Id))
                ModelState.AddModelError("", "Country doesn't exist");

            if (!ModelState.IsValid)
                return StatusCode(404, ModelState);

            updatedAuthorInfo.Country = _countryRepository.GetCountry(updatedAuthorInfo.Country.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_authorRepository.UpdateAuthor(updatedAuthorInfo))
            {
                ModelState.AddModelError("", $"Something went wrong updating {updatedAuthorInfo.FirstName} {updatedAuthorInfo.LastName}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        //api/authors/authorId
        [HttpDelete("{authorId}")]
        [ProducesResponseType(204)] //no content
        [ProducesResponseType(400)] //Bad Request
        [ProducesResponseType(404)] //not found
        [ProducesResponseType(409)] // conflict
        [ProducesResponseType(500)] // server error
        public IActionResult DeleteAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
                return NotFound();

            var authorToDelete = _authorRepository.GetAuthor(authorId);

            if(_authorRepository.GetBooksByAuthor(authorId).Count() > 0)
            {
                ModelState.AddModelError("", $"Author {authorToDelete.FirstName} {authorToDelete.LastName} cannot be deleted because it is used by at least one book");
                StatusCode(409, ModelState);
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!_authorRepository.DeleteAuthor(authorToDelete))
            {
                ModelState.AddModelError("", $"Something happened trying delete {authorToDelete.FirstName} {authorToDelete.LastName}");
                StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
