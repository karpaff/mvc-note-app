using Microsoft.EntityFrameworkCore;
namespace WebApplication1.Models
{ 
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Note>()
                .HasMany(n => n.Categories)
                .WithMany(c => c.Notes)
                .UsingEntity(j => j.ToTable("NoteCategories"));
        }
    }
}
