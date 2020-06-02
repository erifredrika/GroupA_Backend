﻿using HorrorMovieAPI.DB_Context;
using HorrorMovieAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HorrorMovieAPI.Services
{
    public class DirectorRepository : Repository<Director>, IDirectorRepository
    {
        private readonly HorrorContext _context;
        private readonly ILogger<DirectorRepository> _logger;
        public DirectorRepository(HorrorContext context, ILogger<DirectorRepository> logger):base(context,logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IPagedList<Director>> GetAll(string birthCountry,int? page, int pagesize, bool includeMovies)
        {
            IQueryable<Director> query = _context.Directors;

            _logger.LogInformation("Fetching all directors.");

            if (string.IsNullOrEmpty(birthCountry) == false)
            {
                _logger.LogInformation("Fetching with BirthCountry specified");
                query = query.Where(d => d.BirthCountry == birthCountry);
            }
            else
            {
                _logger.LogInformation("Fetching with BirthCountry NOT specified");
            }

            if (includeMovies)
            {
                query = query.Include(m => m.Movies);
            }

            query = query.OrderBy(d => d.LastName);
            await query.ToListAsync();
            return query.ToPagedList(page ?? 1, pagesize);
        }

        public async Task<Director> GetById(int id, bool includeMovies)
        {
            _logger.LogInformation($"Fetching director with the id: {id}.");
            var query= await _context.Directors.Where(d => d.Id == id).FirstOrDefaultAsync();

            if (includeMovies)
            {
                query = await _context.Directors.Where(d => d.Id == id).Include(m=>m.Movies).FirstOrDefaultAsync();
            }
          
            return query;
        }
    }
}