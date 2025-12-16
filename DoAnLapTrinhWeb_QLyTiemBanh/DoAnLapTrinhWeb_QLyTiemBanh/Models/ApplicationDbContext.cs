
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace DoAnLapTrinhWeb_QLyTiemBanh.Models
{
    public class ApplicationDbContext: IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<UserCart> UserCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Thiết lập khóa chính và kiểu decimal chính xác cho Price
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price)
                      .HasColumnType("decimal(18,2)"); // tránh lỗi tràn số
            });
            modelBuilder.Entity<UserCart>()
        .HasMany(u => u.CartItems)
        .WithOne(c => c.UserCart)
        .HasForeignKey(c => c.UserCartId)
        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
