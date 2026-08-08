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

    public virtual DbSet<TblAuditLog> TblAuditLogs { get; set; }

    public virtual DbSet<TblBu> TblBus { get; set; }

    public virtual DbSet<TblCard> TblCards { get; set; }

    public virtual DbSet<TblCardRegistrationOtp> TblCardRegistrationOtps { get; set; }

    public virtual DbSet<TblPermission> TblPermissions { get; set; }

    public virtual DbSet<TblRole> TblRoles { get; set; }

    public virtual DbSet<TblRolePermission> TblRolePermissions { get; set; }

    public virtual DbSet<TblStaffUser> TblStaffUsers { get; set; }

    public virtual DbSet<TblTerminal> TblTerminals { get; set; }

    public virtual DbSet<TblTopUp> TblTopUps { get; set; }

    public virtual DbSet<TblTransaction> TblTransactions { get; set; }

    public virtual DbSet<TblUserRole> TblUserRoles { get; set; }

    public virtual DbSet<TblViewerUser> TblViewerUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblAuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId);

            entity.ToTable("Tbl_AuditLog");

            entity.HasIndex(e => e.Action, "IX_Tbl_AuditLog_Action");

            entity.HasIndex(e => e.CreatedDateTime, "IX_Tbl_AuditLog_CreatedDateTime");

            entity.HasIndex(e => e.FeatureName, "IX_Tbl_AuditLog_FeatureName");

            entity.HasIndex(e => e.UserId, "IX_Tbl_AuditLog_UserId");

            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.CreatedDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.EntityName).HasMaxLength(100);
            entity.Property(e => e.FeatureName).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(100);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.UserType).HasMaxLength(20);
        });

        modelBuilder.Entity<TblBu>(entity =>
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

            entity.HasIndex(e => e.CardNum, "IX_Tbl_Card_CardNum").IsUnique();

            entity.HasIndex(e => e.CardNum, "UQ__Tbl_Card__9B6B7CD1DA8BE5F0").IsUnique();

            entity.HasIndex(e => e.MobileNo, "UX_Tbl_Card_MobileNo_Active")
                .IsUnique()
                .HasFilter("([DeleteFlag]=(0) AND [MobileNo] IS NOT NULL)");

            entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CardNum).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MobileNo).HasMaxLength(20);
            entity.Property(e => e.OwnerName).HasMaxLength(100);
        });

        modelBuilder.Entity<TblCardRegistrationOtp>(entity =>
        {
            entity.HasKey(e => e.OtpId).HasName("PK__Tbl_Card__3143C4A337CE20E7");

            entity.ToTable("Tbl_CardRegistrationOtp");

            entity.HasIndex(e => e.CreatedByUserId, "IX_Tbl_CardRegistrationOtp_CreatedByUserId");

            entity.HasIndex(e => e.ExpiresAt, "IX_Tbl_CardRegistrationOtp_ExpiresAt");

            entity.HasIndex(e => e.PhoneNumber, "IX_Tbl_CardRegistrationOtp_PhoneNumber");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.MaxAttemptCount).HasDefaultValue(5);
            entity.Property(e => e.OtpCodeHash).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Purpose).HasMaxLength(50);
            entity.Property(e => e.VerifiedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblPermission>(entity =>
        {
            entity.HasKey(e => e.PermissionId);

            entity.ToTable("Tbl_Permission");

            entity.HasIndex(e => e.FeatureName, "IX_Tbl_Permission_FeatureName");

            entity.HasIndex(e => e.IsActive, "IX_Tbl_Permission_IsActive");

            entity.HasIndex(e => e.PermissionCode, "UX_Tbl_Permission_PermissionCode_Active")
                .IsUnique()
                .HasFilter("([DeleteFlag]=(0))");

            entity.Property(e => e.ActionName).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.FeatureName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PermissionCode).HasMaxLength(100);
            entity.Property(e => e.PermissionName).HasMaxLength(150);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblRole>(entity =>
        {
            entity.HasKey(e => e.RoleId);

            entity.ToTable("Tbl_Role");

            entity.HasIndex(e => e.RoleCode, "UX_Tbl_Role_RoleCode_Active")
                .IsUnique()
                .HasFilter("([DeleteFlag]=(0))");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleCode).HasMaxLength(50);
            entity.Property(e => e.RoleName).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblRolePermission>(entity =>
        {
            entity.HasKey(e => e.RolePermissionId);

            entity.ToTable("Tbl_RolePermission");

            entity.HasIndex(e => e.PermissionId, "IX_Tbl_RolePermission_PermissionId");

            entity.HasIndex(e => e.RoleId, "IX_Tbl_RolePermission_RoleId");

            entity.HasIndex(e => new { e.RoleId, e.PermissionId }, "UX_Tbl_RolePermission_Role_Permission_Active")
                .IsUnique()
                .HasFilter("([DeleteFlag]=(0))");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Permission).WithMany(p => p.TblRolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_RolePermission_Tbl_Permission");

            entity.HasOne(d => d.Role).WithMany(p => p.TblRolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_RolePermission_Tbl_Role");
        });

        modelBuilder.Entity<TblStaffUser>(entity =>
        {
            entity.HasKey(e => e.StaffUserId);

            entity.ToTable("Tbl_StaffUser");

            entity.HasIndex(e => e.UserName, "UX_Tbl_StaffUser_UserName_Active")
                .IsUnique()
                .HasFilter("([DeleteFlag]=(0))");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.PasswordSalt).HasMaxLength(500);
            entity.Property(e => e.PhoneNo).HasMaxLength(20);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(100);
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

        modelBuilder.Entity<TblTopUp>(entity =>
        {
            entity.HasKey(e => e.TopUpId);

            entity.ToTable("Tbl_TopUp");

            entity.HasIndex(e => e.CardId, "IX_Tbl_TopUp_CardId");

            entity.HasIndex(e => e.TopUpDate, "IX_Tbl_TopUp_TopUpDate");

            entity.HasIndex(e => e.TopUpNo, "IX_Tbl_TopUp_TopUpNo").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Remark).HasMaxLength(250);
            entity.Property(e => e.TopUpDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TopUpNo).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Card).WithMany(p => p.TblTopUps)
                .HasForeignKey(d => d.CardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TopUp_Card");
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

        modelBuilder.Entity<TblUserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId);

            entity.ToTable("Tbl_UserRole");

            entity.HasIndex(e => e.RoleId, "IX_Tbl_UserRole_RoleId");

            entity.HasIndex(e => e.UserId, "IX_Tbl_UserRole_UserId");

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "UX_Tbl_UserRole_User_Role_Active")
                .IsUnique()
                .HasFilter("([DeleteFlag]=(0))");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_UserRole_Tbl_Role");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_UserRole_Tbl_StaffUser");
        });

        modelBuilder.Entity<TblViewerUser>(entity =>
        {
            entity.HasKey(e => e.ViewerUserId);

            entity.ToTable("Tbl_ViewerUser");

            entity.HasIndex(e => e.PhoneNo, "UX_Tbl_ViewerUser_PhoneNo_Active")
                .IsUnique()
                .HasFilter("([DeleteFlag]=(0))");

            entity.HasIndex(e => e.UserName, "UX_Tbl_ViewerUser_UserName_Active")
                .IsUnique()
                .HasFilter("([DeleteFlag]=(0))");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.PasswordSalt).HasMaxLength(500);
            entity.Property(e => e.PhoneNo).HasMaxLength(20);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
