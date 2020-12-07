﻿using AutoMapper;
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
  /// Endpoint used to interact with the Authors in the book store's database
  /// </summary>
  [Route("api/[controller]")]
  [ApiController]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public class AuthorsController : ControllerBase
  {
    //dependency injection
    private readonly IAuthorRepository _authorRepository;
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;

    public AuthorsController(IAuthorRepository authorRepository, ILoggerService logger,
      IMapper mapper)
    {
      _authorRepository = authorRepository;
      _logger = logger;
      _mapper = mapper;
    }

    /// <summary>
    /// Get all authors
    /// </summary>
    /// <returns>List of authors</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuthors()
    {
      try
      {
        _logger.LogInfo("Attempted GetAuthors()");
        var authors = await _authorRepository.FindAll();
        var response = _mapper.Map<IList<AuthorDTO>>(authors);

        _logger.LogInfo("Successfully got all authors");

        return Ok(response);
      }
      catch (Exception e)
      {
        //first
        _logger.LogError($"{e.Message} - {e.InnerException}");
        return StatusCode(500, "Something went wrong. Please contact the administrator");
      }
    }

    /// <summary>
    /// Get an author by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>An author's record</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuthor(int id)
    {
      try
      {
        _logger.LogInfo($"Attempted to get author with id: {id}");
        var author = await _authorRepository.FindById(id);
        if (author == null)
        {
          _logger.LogWarn($"Author with id: {id} was not found");
          return NotFound();
        }
        var response = _mapper.Map<AuthorDTO>(author);

        _logger.LogInfo($"Successfully got author with id: {id}");

        return Ok(response);
      }
      catch (Exception e)
      {
        //second
        return InternalError($"{e.Message} - {e.InnerException}");
      }
    }

    /// <summary>
    /// Creates an author
    /// </summary>
    /// <param name="authorDTO"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
    {
      try
      {
        _logger.LogWarn($"Author submission attempted");
        if (authorDTO == null)
        {
          _logger.LogWarn($"Empty request was submitted");
          return BadRequest(ModelState);
        }
        if (!ModelState.IsValid)
        {
          _logger.LogWarn("Author data was incomplete");
          return BadRequest(ModelState);
        }

        var author = _mapper.Map<Author>(authorDTO);
        var isSuccess = await _authorRepository.Create(author);

        if (!isSuccess)
        {
          return InternalError("Author creation failed");
        }

        _logger.LogWarn("Author created");
        return Created("Create", new { author });
      }
      catch (Exception e)
      {
        return InternalError($"{e.Message} - {e.InnerException}");
      }
    }

    /// <summary>
    /// Updates an author
    /// </summary>
    /// <param name="id"></param>
    /// <param name="author"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO authorDTO)
    {
      try
      {
        _logger.LogWarn($"Author update attempted id: {id}");

        if (id <= 0 || authorDTO == null || id != authorDTO.Id)
        {
          return BadRequest();
        }
        var isExist = await _authorRepository.IsExist(id);

        if (!isExist)
        {
          _logger.LogWarn($"Author with id: {id} was not found");
          return NotFound();
        }

        if (!ModelState.IsValid)
        {
          _logger.LogWarn("Author data was incomplete");
          return BadRequest(ModelState);
        }

        var author = _mapper.Map<Author>(authorDTO);
        var isSuccess = await _authorRepository.Update(author);

        if (!isSuccess)
        {
          return InternalError("Author update failed");
        }

        _logger.LogWarn("Author updated");
        return NoContent();
      }
      catch (Exception e)
      {
        return InternalError($"{e.Message} - {e.InnerException}");
      }
    }

    /// <summary>
    /// Removes an author by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id)
    {
      try
      {
        _logger.LogWarn($"Author update attempted id: {id}");

        if (id <= 0)
        {
          return BadRequest();
        }

        var isExist = await _authorRepository.IsExist(id);

        if (!isExist)
        {
          _logger.LogWarn($"Author with id: {id} was not found");
          return NotFound();
        }
         
        var author = await _authorRepository.FindById(id);
       
        var isSuccess = await _authorRepository.Delete(author);

        if (!isSuccess)
        {
          return InternalError("Author delete failed");
        }

        _logger.LogWarn($"Author with id: {id} deleted");
        return NoContent();
      }
      catch (Exception e)
      {
        return InternalError($"{e.Message} - {e.InnerException}");
      }
    }

    private ObjectResult InternalError(string message)
    {
      _logger.LogError(message);
      return StatusCode(500, "Something went wrong. Please contact the administrator");
    }
  }
}
