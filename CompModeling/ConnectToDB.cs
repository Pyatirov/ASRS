using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompModeling
{
    class ConnectToDB
    {
        public class ApplicationContext : DbContext
        {
            public DbSet<InputConcentrations> InputConcentrations { get; set; } = null!;
            public DbSet<BaseForms> BaseForms { get; set; } = null!;
            public DbSet<FormingForms> FormingForms { get; set; } = null!;
            public DbSet<ConcentrationConstants> ConcentrationConstants { get; set; } = null!;

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Data Source=compmodeling.db");
                //commentary from PC
            }
        }
    }
}
