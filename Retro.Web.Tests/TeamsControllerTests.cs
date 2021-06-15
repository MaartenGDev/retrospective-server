using NUnit.Framework;
using Retro.Data.Context;
using Retro.Web.Controllers;

namespace Retro.Web.Tests
{
    public class TeamsControllerTests
    {
        private RetroContext _context;

        [SetUp]
        public void Setup()
        {
            _context = new InMemoryRetroContext();
        }
    }
}