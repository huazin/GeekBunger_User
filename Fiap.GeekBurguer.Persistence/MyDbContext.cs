using Fiap.GeekBurguer.Domain.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fiap.GeekBurguer.Persistence
{
    public class MyDbContext : IdentityDbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet<User> User { get; set; }
        public DbSet<Restriction> Restriction { get; set; }
    }
}