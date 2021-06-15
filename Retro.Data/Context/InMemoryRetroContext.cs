using System;
using Microsoft.EntityFrameworkCore;

namespace Retro.Data.Context
{
    public class InMemoryRetroContext : RetroContext
    {
        public InMemoryRetroContext () : base(CreateOptions())
        {}
        
        private static DbContextOptions<RetroContext> CreateOptions()
        {
            var builder = new DbContextOptionsBuilder<RetroContext>();
            builder.UseSqlite(Guid.NewGuid().ToString());
            return builder.Options;
        }
    }
}