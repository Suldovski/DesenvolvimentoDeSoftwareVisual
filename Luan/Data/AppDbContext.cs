using Microsoft.EntityFrameworkCore;
using Luan.Models;

namespace Luan.Data;

public class AppDbContext : DbContext
{
    public DbSet<ConsumoAgua> Consumos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        optionsBuilder.UseSqlite("Data Source=luan_dupla.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConsumoAgua>(entity =>
        {

            entity.HasKey(e => e.Id);


            entity.Property(e => e.Id)
                  .ValueGeneratedOnAdd();

            entity.HasIndex(e => new { e.Cpf, e.Mes, e.Ano })
                  .IsUnique();

            entity.Property(e => e.Cpf)
                  .HasMaxLength(14)
                  .IsRequired();

            entity.Property(e => e.Bandeira)
                  .HasMaxLength(20)
                  .IsRequired();
        });
    }
}
    