﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UdemyRealWorldUnitTest.Web.Models
{
    public partial class UdemyUnitTestContext : DbContext
    {
        public UdemyUnitTestContext()
        {
        }

        public UdemyUnitTestContext(DbContextOptions<UdemyUnitTestContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Product> Product { get; set; }

    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Color)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
