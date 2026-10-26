using InvoiceFlow.Domain.Entities;
using InvoiceFlow.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Infrastructure.Persistence;

public sealed class InvoiceDbContext : DbContext
{
    public InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();

    public DbSet<ExtractionResult> ExtractionResults => Set<ExtractionResult>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Document configuration
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.HasMany(x => x.Extractions)
                .WithOne(x => x.Document)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Invoice)
                .WithOne(x => x.Document)
                .HasForeignKey<Invoice>(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ExtractionResult configuration
        modelBuilder.Entity<ExtractionResult>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Vendor)
                .HasMaxLength(255);

            entity.Property(x => x.InvoiceNumber)
                .HasMaxLength(100);

            entity.Property(x => x.Currency)
                .HasMaxLength(3);

            entity.Property(x => x.Total)
                .HasConversion<MoneyConverter>();
        });

        // Approved Invoice configuration
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Vendor)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(x => x.Total)
                .HasConversion<MoneyConverter>();

            entity.HasIndex(x => x.DocumentId)
                .IsUnique();
        });
    }
}