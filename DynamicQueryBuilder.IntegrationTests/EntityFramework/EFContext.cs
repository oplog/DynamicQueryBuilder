﻿using DynamicQueryBuilder.IntegrationTests.EntityFramework.SampleModels;

using Microsoft.EntityFrameworkCore;

namespace DynamicQueryBuilder.IntegrationTests.EntityFramework
{
    public class EFContext : DbContext
    {
        public EFContext()
        {
            ChangeTracker.LazyLoadingEnabled = false;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=maestro_test;Username=maestro-user;Password=test123456");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaseModel>().HasKey(x => x.Id);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Order> Orders { get; set; }
    }
}
