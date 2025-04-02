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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseIdentityAlwaysColumns();
            modelBuilder.UseSerialColumns();
        }

        public DbSet<GraphAPI.Models.node> node { get; set; } = default!;
        public DbSet<GraphAPI.Models.graph> graph { get; set; } = default!;
        public DbSet<GraphAPI.Models.nodetype> nodetype { get; set; } = default!;
        public DbSet<GraphAPI.Models.edge> edge { get; set; } = default!;
        public DbSet<GraphAPI.Models.edgetype> edgetype { get; set; } = default!;
    }
}
