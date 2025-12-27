using FoodDelivery.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;

namespace FoodDelivery.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
        : IdentityDbContext<AppUser, IdentityRole, string>(options)
    {
        // 1. CÁC DBSSET
        public DbSet<Product> Products { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<OptionGroup> OptionGroups { get; set; }
        public DbSet<ProductOption> ProductOptions { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogCategory> BlogCategories { get; set; }
        public DbSet<EmailSetting> EmailSettings { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }

        public DbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; }

        // 2. CẤU HÌNH WARNINGS
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        // 3. CẤU HÌNH MODEL (GỘP TẤT CẢ VÀO ĐÂY)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Luôn gọi base đầu tiên cho Identity

            // A. SQL DEFAULT VALUES
            modelBuilder.Entity<Order>().Property(o => o.OrderDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            modelBuilder.Entity<Product>().Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            modelBuilder.Entity<Conversation>().Property(c => c.LastMessageAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            modelBuilder.Entity<ChatMessage>().Property(m => m.MessageType).HasDefaultValueSql("'text'");

            // B. INDEX & KEYS
            modelBuilder.Entity<Wishlist>().HasKey(w => new { w.UserId, w.ProductId });
            modelBuilder.Entity<Setting>().HasIndex(s => s.Key).IsUnique();

            // Cấu hình Index cho ProductReview (Đã gộp từ đoạn lỗi của bạn)
            modelBuilder.Entity<ProductReview>().HasIndex(r => r.ProductId);
            modelBuilder.Entity<ProductReview>().HasIndex(r => r.IsApproved);

            // C. SEED DATA - SETTINGS
            modelBuilder.Entity<Setting>().HasData(
                new Setting { Id = 1, Key = "store_name", Value = "Cơm Tấm Đêm", Description = "Tên hiển thị" },
                new Setting { Id = 2, Key = "hotline", Value = "0909123456", Description = "Hotline" },
                new Setting { Id = 3, Key = "shipping_fee", Value = "30000", Description = "Phí ship" },
                new Setting { Id = 4, Key = "min_order_value", Value = "50000", Description = "Min order" },
                new Setting { Id = 5, Key = "banner_url", Value = "/images/banner-tet.jpg", Description = "Banner" }
            );

            // SEED DATA - CATEGORIES
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Đồ Uống", Description = "Các loại nước uống", SortOrder = 0 },
                new Category { Id = 2, Name = "Đồ Ăn", Description = "Các món chính", SortOrder = 1 }
            );

            // SEED DATA - PRODUCT
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 100,
                    Name = "Trà Sữa Trân Châu",
                    Price = 20000,
                    StockQuantity = 50,
                    Description = "Ngon",
                    CategoryId = 1,
                    IsActive = true,
                    ImageUrl = "",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // SEED DATA - OPTIONS
            modelBuilder.Entity<OptionGroup>().HasData(
                new OptionGroup { Id = 1, ProductId = 100, Name = "Chọn Size", IsRequired = true, AllowMultiple = false },
                new OptionGroup { Id = 2, ProductId = 100, Name = "Thêm Topping", IsRequired = false, AllowMultiple = true }
            );

            modelBuilder.Entity<ProductOption>().HasData(
                new ProductOption { Id = 1, OptionGroupId = 1, Name = "Size M", PriceModifier = 0 },
                new ProductOption { Id = 2, OptionGroupId = 1, Name = "Size L", PriceModifier = 5000 },
                new ProductOption { Id = 3, OptionGroupId = 2, Name = "Trân châu đen", PriceModifier = 3000 },
                new ProductOption { Id = 4, OptionGroupId = 2, Name = "Thạch trái cây", PriceModifier = 3000 },
                new ProductOption { Id = 5, OptionGroupId = 2, Name = "Pudding trứng", PriceModifier = 5000 }
            );

            // SEED DATA - CHAT
            modelBuilder.Entity<Conversation>().HasData(
                new Conversation
                {
                    Id = 1,
                    SessionId = "session-test-01",
                    CustomName = "Khách hàng dùng thử",
                    EmailOrPhone = "0909123456",
                    StartedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastMessageAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsClosed = false
                }
            );

            // D. IDENTITY SEED DATA
            string roleId = "admin-role-id-001";
            string adminId = "admin-user-id-001";

            modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = roleId,
                Name = "Admin",
                NormalizedName = "ADMIN"
            });

            var adminUser = new AppUser
            {
                Id = adminId,
                UserName = "admin@gmail.com",
                NormalizedUserName = "ADMIN@GMAIL.COM",
                Email = "admin@gmail.com",
                NormalizedEmail = "ADMIN@GMAIL.COM",
                EmailConfirmed = true,
                FullName = "Hệ Thống Admin",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            PasswordHasher<AppUser> ph = new();
            adminUser.PasswordHash = ph.HashPassword(adminUser, "Admin@123");

            modelBuilder.Entity<AppUser>().HasData(adminUser);

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = roleId,
                UserId = adminId
            });

            // SEED DATA - EMAIL SETTING
            modelBuilder.Entity<EmailSetting>().HasData(
                new EmailSetting { Id = 1, Host = "smtp.gmail.com", Port = 587, Email = "admin@example.com", Password = "change_me" }
            );
        }
    }
}