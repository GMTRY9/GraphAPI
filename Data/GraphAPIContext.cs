using Microsoft.EntityFrameworkCore;
using GraphAPI.Models;

namespace GraphAPI.Data
{
    public class GraphAPIContext : DbContext
    {
        public GraphAPIContext (DbContextOptions<GraphAPIContext> options)
            : base(options)
        {
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseIdentityAlwaysColumns();
            modelBuilder.UseSerialColumns();

            modelBuilder.Entity<node>(entity =>
            {
                entity.HasKey(e => e.nodeid);
                entity.Property(e => e.name);
                entity.Property(e => e.classification);
                entity.Property(e => e.copyrightowner);
                entity.Property(e => e.version);
                entity.Property(e => e.payload).HasColumnType("jsonb");
            });

            modelBuilder.Entity<graph>(entity =>
            {
                entity.HasKey(e => e.graphid);
                entity.Property(e => e.name);
            });

            modelBuilder.Entity<nodetype>(entity =>
            {
                entity.HasKey(e => e.nodetypeid);
                entity.Property(e => e.name);
                entity.Property(e => e.fields).HasColumnType("jsonb");
                entity.Property(e => e.settings).HasColumnType("jsonb");
            });
        }

        public DbSet<GraphAPI.Models.node> node { get; set; } = default!;
        public DbSet<GraphAPI.Models.graph> graph { get; set; } = default!;
        public DbSet<GraphAPI.Models.nodetype> nodetype { get; set; } = default!;
        public DbSet<GraphAPI.Models.edge> edge { get; set; } = default!;
        public DbSet<GraphAPI.Models.edgetype> edgetype { get; set; } = default!;
    }
}
