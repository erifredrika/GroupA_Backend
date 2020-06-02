﻿using System.Net;
using AutoMapper;
using HorrorMovieAPI.Dto;
using HorrorMovieAPI.Models;
using HorrorMovieAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.Swagger.Annotations;
using PagedList;

namespace HorrorMovieAPI.Controllers
{
    [ApiController]
    [Route("api/v1.0/[controller]")]
    [Authorize]
    public class DirectorsController : ControllerBase
    {
        private readonly IDirectorRepository _repository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        public DirectorsController(IDirectorRepository repository, IMapper mapper, IUrlHelper urlHelper)
        {
            _repository = repository;
            _mapper = mapper;
            _urlHelper = urlHelper;
        }
        /// <summary>
        /// Gets all the directors with possible filtering by birthcountry and include of directed movies
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <param name="birthCountry"></param>
        /// <param name="includeMovies"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetAllDirectors")]
        public async Task<ActionResult<DirectorDTO[]>> GetAllDirectors(int? page,int pagesize=3, string birthCountry = "", bool includeMovies = false)
        {
            try
            {
                var results = await _repository.GetAll(birthCountry,page, pagesize, includeMovies);
                var links = CreateLinksForCollection(results);
                var toReturn = results.Select(x => ExpandSingleItem(x));
                return Ok(new
                {
                    value = toReturn,
                    links = links
                });
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Failed to retrieve directors. Exception thrown: {e.Message} ");
            }
            
        }
        /// <summary>
        /// Get a director by a specific id and possible include of directed movies
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includeMovies"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name ="GetDirectorById")]
        public async Task<ActionResult<DirectorDTO>> GetDirectorById(int id, bool includeMovies = false)
        {
            try
            {
                var result = await _repository.GetById(id, includeMovies);
                return Ok(ExpandSingleItem(result));
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Failed to retrieve director with id {id}. Exception thrown: {e.Message} ");
            }
            
        }
        /// <summary>
        /// Delete a director by unique id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}", Name ="DeleteDirectorById")]
        public async Task<IActionResult> DeleteDirectorById(int id)
        {
            try
            {
                var director = await _repository.GetById(id, false);

                if (director == null)
                {
                    return BadRequest($"Could not delete director. Director with Id {id} was not found.");
                }
                await _repository.Delete<Director>(id);

                return NoContent();
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Failed to delete the director. Exception thrown when attempting to delete data from the database: {e.Message}");
            }

        }
        /// <summary>
        /// Update directordetails by unique id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="directorForUpdateDto"></param>
        /// <returns></returns>
        [HttpPut("{id}", Name = "UpdateDirectorDetails")]
        public async Task<IActionResult> UpdateDirectorDetails(int id, [FromBody] DirectorForUpdateDTO directorForUpdateDto)
        {
            try
            {
                var directorFromRepo = await _repository.GetById(id, false);

                if (directorFromRepo == null)
                {
                    return BadRequest($"Could not update director. Director with Id {id} was not found.");
                }
                var directorForUpdate = _mapper.Map(directorForUpdateDto, directorFromRepo);

                await _repository.Update(directorForUpdate);

                return NoContent();
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Failed to update the director. Exception thrown when attempting to update data in the database: {e.Message}");
            }
        }
        /// <summary>
        /// Post a new director
        /// </summary>
        /// <param name="directorDto"></param>
        /// <returns></returns>
        [HttpPost(Name = "CreateDirector")]
        public async Task<IActionResult> CreateDirector([FromBody] DirectorForUpdateDTO directorDto)
        {
            try
            {
                var director = _mapper.Map<Director>(directorDto);

                var directorFromRepo = await _repository.Add(director);

                if (directorFromRepo != null)
                {
                    return CreatedAtAction(nameof(GetDirectorById), new { id = director.Id }, director);
                }
                return BadRequest("Failed to create director.");

            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError,
                    $"Failed to update the director. Exception thrown when attempting to update data in the database: {e.Message}");
            }
        }

        private dynamic ExpandSingleItem(Director director)
        {
            var links = GetLinks(director);
            DirectorDTO directorDto = _mapper.Map<DirectorDTO>(director);

            var resourceToReturn = directorDto.ToDynamic() as IDictionary<string, object>;
            resourceToReturn.Add("links", links);

            return resourceToReturn;
        }

        private IEnumerable<LinkDto> GetLinks(Director director)
        {
            var links = new List<LinkDto>();

            links.Add(
              new LinkDto(_urlHelper.Link(nameof(GetDirectorById), new { id = director.Id }),
              "self",
              "GET"));

            links.Add(
              new LinkDto(_urlHelper.Link(nameof(DeleteDirectorById), new { id = director.Id }),
              "delete",
              "DELETE"));

            links.Add(
               new LinkDto(_urlHelper.Link(nameof(UpdateDirectorDetails), new { id = director.Id }),
               "update",
               "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link(nameof(CreateDirector), null),
              "create",
              "POST"));
              
            return links;
        }

        private List<LinkDto> CreateLinksForCollection(IPagedList pageList)
        {
            var links = new List<LinkDto>();


            // self 
            links.Add(
             new LinkDto(_urlHelper.Link(nameof(GetAllDirectors), new
             {
                 pagesize = pageList.PageCount,
                 page = pageList.PageNumber
             }), "current page", "GET"));

            links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllDirectors), new
            {
                pagesize = pageList.PageCount,
                page = 1,
            }), "first", "GET"));

            links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllDirectors), new
            {
                pagesize = pageList.PageCount,
                page = pageList.PageCount,
            }), "last", "GET"));

            if (!pageList.IsLastPage)
            {
                links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllDirectors), new
                {
                    pagesize = pageList.PageCount,
                    page = pageList.PageNumber + 1,
                }), "next", "GET"));
            }

            if (!pageList.IsFirstPage)
            {
                links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllDirectors), new
                {
                    pagesize = pageList.PageCount,
                    page = pageList.PageNumber - 1,
                }), "previous", "GET"));
            }

            return links;
        }
    }
}