using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Retro.Data.Context;
using Retro.Data.Models;

namespace Retro.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class TeamMemberRolesController : Controller
    {
        private readonly RetroContext _dbContext;

        public TeamMemberRolesController(RetroContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IEnumerable<Role> Get()
        {
            return _dbContext.TeamMemberRoles;
        }
    }
}