﻿using AutoMapper;
using Castle.Core.Logging;
using HorrorMovieAPI.Controllers;
using HorrorMovieAPI.DB_Context;
using HorrorMovieAPI.Models;
using HorrorMovieAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Xunit;

namespace HorrorMovieAPI.Tests.ControllerTests
{
    public class ActorControllerTests
    {
        private readonly Mock<IActorRepository> _mockRepo;
        private readonly Mock<HorrorContext> _mockContext;
        private readonly Mock<IUrlHelper> _urlHelper;
        private readonly Mock<IMapper> _mapper;
        private readonly ActorsController _controller;
        private readonly Mock<ILogger> _logger;
        
       
        public ActorControllerTests()
        {
            _urlHelper = new Mock<IUrlHelper>();
            _logger = new Mock<ILogger>();
            _mockContext = new Mock<HorrorContext>();
            _mapper = new Mock<IMapper>();
            _mockRepo = new Mock<IActorRepository>();            
            _controller = new ActorsController(_urlHelper.Object, _mockRepo.Object, _mapper.Object);
        }

        [Fact]
        public void Actor_GetAll_ReturnsObject()
        {
            var result = _controller.GetAll();

           Assert.NotNull(result);
        }

        [Fact]
        public void Director_ReturnsExactNumberOfActors()
        {
            List<Actor> actors = new List<Actor>()
            {
                new Actor() {
                    Id = 1,
                    FirstName = "Ryan",
                    LastName = "Gosling",
                    DOB = new DateTime(1980-11-12)
                },
                new Actor() {
                    Id = 2,
                    FirstName = "Jhonny",
                    LastName = "Depp",
                    DOB = new DateTime(1963-06-9)
                }
            };
            _mockRepo.Setup(repo => repo.GetAll("", true))
                .ReturnsAsync(actors);

            var result = _controller.GetAll();

            Assert.Equal(2, actors.Count);

        }

    }
}
