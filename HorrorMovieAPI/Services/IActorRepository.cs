﻿using HorrorMovieAPI.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HorrorMovieAPI.Services
{
    public interface IActorRepository : IRepository<Actor>
    {
        Task<IPagedList<Actor>> GetAll(string firstname, int? page, int pagesize, params string[] including);
        Task<Actor> GetById(int id, bool includeMovies);
    }
}