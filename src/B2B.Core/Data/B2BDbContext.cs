using B2B.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace B2B.Core.Data
{
    public class B2BDbContext : DbContext
    {
        public B2BDbContext(DbContextOptions<B2BDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Agent> Agents => Set<Agent>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity table mappings and relationships
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Agent>(entity =>
            {
                entity.ToTable("Agent");
                entity.HasKey(e => e.AgentID);
                entity.Property(e => e.AgentName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("Item");
                entity.HasKey(e => e.ItemID);
                entity.Property(e => e.ItemName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Size).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.StockQuantity).IsRequired();
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");
                entity.HasKey(e => e.OrderID);
                entity.Property(e => e.OrderStatus).IsRequired().HasMaxLength(30);

                entity.HasOne(e => e.Agent)
                    .WithMany(a => a.Orders)
                    .HasForeignKey(e => e.AgentID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");
                entity.HasKey(e => e.ID);
                entity.Property(e => e.UnitAmount).HasPrecision(18, 2);

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderDetails)
                    .HasForeignKey(e => e.OrderID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Item)
                    .WithMany(i => i.OrderDetails)
                    .HasForeignKey(e => e.ItemID)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
