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
            public DbSet<InputConcentration> InputConcentrations { get; set; } = null!;
            public DbSet<Phase> Phases { get; set; } = null!;
            public DbSet<BaseForm> BaseForms { get; set; } = null!;
            public DbSet<FormingForm> FormingForms { get; set; } = null!;
            public DbSet<ConcentrationConstant> ConcentrationConstants { get; set; } = null!;
            public DbSet<Mechanisms> Mechanisms { get; set; } = null!;
            public DbSet<ReactionMechanism> ReactionMechanism { get; set; } = null!;
            public DbSet<Reaction> Reactions { get; set; } = null!;

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Data Source=compmodeling.db");
            }
        }
    }
}
