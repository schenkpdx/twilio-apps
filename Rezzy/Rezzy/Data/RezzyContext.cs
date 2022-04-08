#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rezzy.Models;

namespace Rezzy.Data
{
    public class RezzyContext : DbContext
    {
        public RezzyContext (DbContextOptions<RezzyContext> options)
            : base(options)
        {
        }

        public DbSet<Rezzy.Models.Reservation> Reservation { get; set; }
    }
}
