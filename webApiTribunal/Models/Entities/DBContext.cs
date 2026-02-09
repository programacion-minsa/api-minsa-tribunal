using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace webApiTribunal.Models.Entities;

public partial class DBContext : DbContext
{
    public DBContext()
    {
    }

    public DBContext(DbContextOptions<DBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppsAccessLog> AppsAccessLog { get; set; }

    public virtual DbSet<AppsAccessToken> AppsAccessToken { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppsAccessLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AppsAccessLog_PK");

            entity.Property(e => e.AppApiKey)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.AppName)
                .HasMaxLength(512)
                .IsUnicode(false);
            entity.Property(e => e.RequestDate).HasColumnType("datetime");
            entity.Property(e => e.RequestId)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.ResponseMessage)
                .HasMaxLength(2500)
                .IsUnicode(false);
            entity.Property(e => e.UserAgent)
                .HasMaxLength(2500)
                .IsUnicode(false);
            entity.Property(e => e.UserIpAddress)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AppsAccessToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AppsAccessToken_PK");

            entity.Property(e => e.AppDescription)
                .HasMaxLength(1500)
                .IsUnicode(false);
            entity.Property(e => e.AppName)
                .HasMaxLength(512)
                .IsUnicode(false);
            entity.Property(e => e.AppToken)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
