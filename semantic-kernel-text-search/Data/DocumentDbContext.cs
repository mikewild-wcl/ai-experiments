using EntityFramework.Exceptions.SqlServer;
using Microsoft.EntityFrameworkCore;
using semantic_kernel_text_search.Data.Entities;

namespace semantic_kernel_text_search.Data;

public class DocumentDbContext(
    DbContextOptions<DocumentDbContext> options
    ) : DbContext(options)
{
    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentContent> DocumentContents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseExceptionProcessor();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255);
        });

        modelBuilder.Entity<DocumentContent>(entity =>
        {
            entity.ToTable("DocumentContents");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Embedding)
                .HasColumnType("vector(1536)")
                .IsRequired();

            entity.HasOne(d => d.Document).WithMany(p => p.Contents)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DocumentChunks_Documents");
        });
    }
}
