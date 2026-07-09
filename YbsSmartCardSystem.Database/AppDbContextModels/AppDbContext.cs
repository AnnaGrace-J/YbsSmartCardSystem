using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblBus> TblBus { get; set; }

    public virtual DbSet<TblCard> TblCards { get; set; }

    public virtual DbSet<TblTerminal> TblTerminals { get; set; }

    public virtual DbSet<TblTransaction> TblTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblBus>(entity =>
        {
            entity.HasKey(e => e.BusId).HasName("PK__Tbl_Bus__6A0F60B5CD2F452C");

            entity.ToTable("Tbl_Bus");

            entity.HasIndex(e => e.BusNo, "UQ__Tbl_Bus__6A0F3A401D8FBE4D").IsUnique();

            entity.Property(e => e.BusLicense).HasMaxLength(50);
            entity.Property(e => e.BusNo).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<TblCard>(entity =>
        {
            entity.HasKey(e => e.CardId).HasName("PK__Tbl_Card__55FECDAE92BF02E6");

            entity.ToTable("Tbl_Card");

            entity.HasIndex(e => e.CardNum, "UQ__Tbl_Card__9B6B7CD1DA8BE5F0").IsUnique();

            entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CardNum).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MobileNo).HasMaxLength(20);
            entity.Property(e => e.OwnerName).HasMaxLength(100);
        });

        modelBuilder.Entity<TblTerminal>(entity =>
        {
            entity.HasKey(e => e.TerminalId).HasName("PK__Tbl_Term__6A7262A9048CE6EA");

            entity.ToTable("Tbl_Terminal");

            entity.HasIndex(e => e.TerminalSerialNo, "UQ__Tbl_Term__8B909E83BAEF313A").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TerminalSerialNo).HasMaxLength(100);

            entity.HasOne(d => d.Bus).WithMany(p => p.TblTerminals)
                .HasForeignKey(d => d.BusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Terminal_Bus");
        });

        modelBuilder.Entity<TblTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__Tbl_Tran__55433A6BC2287FDE");

            entity.ToTable("Tbl_Transaction");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransactionDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TransactionNo).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Card).WithMany(p => p.TblTransactions)
                .HasForeignKey(d => d.CardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaction_Card");

            entity.HasOne(d => d.Terminal).WithMany(p => p.TblTransactions)
                .HasForeignKey(d => d.TerminalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaction_Terminal");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
