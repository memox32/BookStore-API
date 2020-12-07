using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_API.Controllers
{
  /// <summary>
  /// Interacts with the Books table
  /// </summary>
  [Route("api/[controller]")]
  [ApiController]
  public class BooksController : ControllerBase
  {
    //dependency injection
    private readonly IBookRepository _bookRepository;
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;

    public BooksController(IBookRepository bookRepository, ILoggerService logger,
      IMapper mapper)
    {
      _bookRepository = bookRepository;
      _logger = logger;
      _mapper = mapper;
    }

    /// <summary>
    /// Get all books
    /// </summary>
    /// <returns>List of books</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBooks()
    {
      var location = GetControllerActionNames();

      try
      {
        _logger.LogInfo($"{location}: attempted call");
        var books = await _bookRepository.FindAll();
        var response = _mapper.Map<IList<BookDTO>>(books);

        _logger.LogInfo($"{location}: successful");

        return Ok(response);
      }
      catch (Exception e)
      {
        //first
        //_logger.LogError($"{e.Message} - {e.InnerException}");
        return InternalError($"{location}: {e.Message} - {e.InnerException}");
      }
    }

    /// <summary>
    /// Gets a book by id
    /// </summary>
    /// <returns>A book record</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBook(int id)
    {
      var location = GetControllerActionNames();

      try
      {
        _logger.LogInfo($"{location}: attempted call for id: {id}");
        var book = await _bookRepository.FindById(id);

        if(book == null)
        {
          _logger.LogInfo($"{location}: failed to retrive record with id: {id}");
          return NotFound();
        }

        var response = _mapper.Map<IList<BookDTO>>(book);

        _logger.LogInfo($"{location}: successfully got record with id: {id}");

        return Ok(response);
      }
      catch (Exception e)
      {
        return InternalError($"{location}: {e.Message} - {e.InnerException}");
      }
    }

    /// <summary>
    /// Creates a book
    /// </summary>
    /// <param name="bookDTO"></param>
    /// <returns>Book object</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
    {
      var location = GetControllerActionNames();

      try
      {
        _logger.LogWarn($"{location}: submission attempted");
        if (bookDTO == null)
        {
          _logger.LogWarn($"{location}: empty request was submitted");
          return BadRequest(ModelState);
        }
        if (!ModelState.IsValid)
        {
          _logger.LogWarn($"{location}: data was incomplete");
          return BadRequest(ModelState);
        }

        var book = _mapper.Map<Book>(bookDTO);
        var isSuccess = await _bookRepository.Create(book);

        if (!isSuccess)
        {
          return InternalError($"{location}: creation failed");
        }

        _logger.LogWarn($"{location}: created");
        return Created("Create", new { book });
      }
      catch (Exception e)
      {
        return InternalError($"{e.Message} - {e.InnerException}");
      }
    }

    /// <summary>
    /// Updates an Book
    /// </summary>
    /// <param name="id"></param>
    /// <param name="book"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
    {
      var location = GetControllerActionNames();

      try
      {
        _logger.LogWarn($"{location}: update attempted id: {id}");

        if (id <= 0 || bookDTO == null || id != bookDTO.Id)
        {
          return BadRequest();
        }
        var isExist = await _bookRepository.IsExist(id);

        if (!isExist)
        {
          _logger.LogWarn($"{location}: with id: {id} was not found");
          return NotFound();
        }

        if (!ModelState.IsValid)
        {
          _logger.LogWarn($"{location}: data was incomplete");
          return BadRequest(ModelState);
        }

        var Book = _mapper.Map<Book>(bookDTO);
        var isSuccess = await _bookRepository.Update(Book);

        if (!isSuccess)
        {
          return InternalError($"{location}: update failed");
        }

        _logger.LogWarn($"{location}: updated");
        return NoContent();
      }
      catch (Exception e)
      {
        return InternalError($"{e.Message} - {e.InnerException}");
      }
    }

    /// <summary>
    /// Removes a book by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
      var location = GetControllerActionNames();

      try
      {
        _logger.LogInfo($"{location}: delete attempted on record with id: {id}");

        if (id <= 0)
        {
          _logger.LogWarn($"{location}: delete failed with bad data - id: {id}");
          return BadRequest();
        }

        var isExist = await _bookRepository.IsExist(id);

        if (!isExist)
        {
          _logger.LogWarn($"{location}: with id: {id} was not found");
          return NotFound();
        }

        var book = await _bookRepository.FindById(id);

        var isSuccess = await _bookRepository.Delete(book);

        if (!isSuccess)
        {
          return InternalError($"{location}: delete failed");
        }

        _logger.LogWarn($"{location}: with id: {id} deleted");
        return NoContent();
      }
      catch (Exception e)
      {
        return InternalError($"{e.Message} - {e.InnerException}");
      }
    }

    private string GetControllerActionNames()
    {
      var controller = ControllerContext.ActionDescriptor.ControllerName;
      var action = ControllerContext.ActionDescriptor.ActionName;

      return $"{controller} - {action}";
    }

    private ObjectResult InternalError(string message)
    {
      _logger.LogError(message);
      return StatusCode(500, "Something went wrong. Please contact the administrator");
    }
  }
}
