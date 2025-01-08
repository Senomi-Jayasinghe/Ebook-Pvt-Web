using E_Book_Pvt_Website.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace E_Book_Pvt_Website.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Define DbSets for your tables
        public DbSet<Book> Book { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Author> Author { get; set; }
        public DbSet<BookCategory> BookCategory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder); // Ensure base method is called



            modelBuilder.Entity<IdentityUserLogin<string>>()
                .HasKey(l => l.UserId); // Define primary key for IdentityUserLogin

            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasKey(r => new { r.UserId, r.RoleId }); // Composite key for IdentityUserRole

            modelBuilder.Entity<IdentityUserClaim<string>>()
                .HasKey(c => c.Id); // Primary key for IdentityUserClaim

            modelBuilder.Entity<IdentityRoleClaim<string>>()
                .HasKey(rc => rc.Id); // Primary key for IdentityRoleClaim

            modelBuilder.Entity<IdentityUserToken<string>>()
                .HasKey(t => new { t.UserId, t.LoginProvider, t.Name }); // Composite key for IdentityUserToken


            modelBuilder.Entity<BookCategory>()
                .HasKey(bc => new { bc.category_id, bc.book_id }); // Define composite primary key
            modelBuilder.Entity<OrderBook>()
                .HasKey(bc => new { bc.order_id, bc.book_id }); // Define composite primary key
            modelBuilder.Entity<BookFeedback>()
                .HasKey(bc => new { bc.book_id, bc.customer_id, bc.feedback }); // Define composite primary key
        }

        public DbSet<Category> Category { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderBook> OrderBook { get; set; }
        public DbSet<BookFeedback> BookFeedback { get; set; }

    }
}
