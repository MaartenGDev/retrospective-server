using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Retro.Data.Context;
using Retro.Data.Models;

namespace Retro.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CommentCategoriesController : ControllerBase {
        private readonly RetroContext _dbContext;

        public CommentCategoriesController(RetroContext retroContext)
        {
            _dbContext = retroContext;
        }
        
        [HttpGet]
        public IEnumerable<CommentCategory> Get()
        {
            return _dbContext.CommentCategories;
        }
    }
}