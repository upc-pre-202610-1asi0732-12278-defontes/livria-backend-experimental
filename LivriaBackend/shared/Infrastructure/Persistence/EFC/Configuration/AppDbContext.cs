using LivriaBackend.communities.Domain.Model.Aggregates;
using LivriaBackend.communities.Domain.Model.ValueObjects; 
using LivriaBackend.commerce.Domain.Model.Aggregates;
using LivriaBackend.commerce.Domain.Model.Entities; 
using LivriaBackend.commerce.Domain.Model.ValueObjects; 
using LivriaBackend.users.Domain.Model.Aggregates;
using LivriaBackend.notifications.Domain.Model.Aggregates;
using LivriaBackend.notifications.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading; 
using System.Threading.Tasks;
using LivriaBackend.IAM.Domain.Model.Aggregates;
using LivriaBackend.wallet.Domain.Model.Aggregates;
using LivriaBackend.wallet.Domain.Model.ValueObjects;

namespace LivriaBackend.shared.Infrastructure.Persistence.EFC.Configuration
{
    /// <summary>
    /// Representa el contexto de la base de datos para la aplicación LivriaBackend.
    /// Hereda de <see cref="DbContext"/> y es responsable de la configuración del modelo de datos
    /// y la interacción con la base de datos utilizando Entity Framework Core.
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>Representa la colección de libros en la base de datos.</summary>
        public DbSet<Book> Books { get; set; }
        /// <summary>Representa la colección de reseñas de libros en la base de datos.</summary>
        public DbSet<Review> Reviews { get; set; }
        /// <summary>Representa la colección de ítems en los carritos de compra en la base de datos.</summary>
        public DbSet<CartItem> CartItems { get; set; }
        /// <summary>Representa la colección de órdenes de compra en la base de datos.</summary>
        public DbSet<Order> Orders { get; set; }
        /// <summary>Representa la colección de ítems dentro de las órdenes de compra en la base de datos.</summary>
        public DbSet<OrderItem> OrderItems { get; set; }

        /// <summary>Representa la colección de usuarios base en la base de datos.</summary>
        public DbSet<User> Users { get; set; }
        /// <summary>Representa la colección de clientes de usuario en la base de datos.</summary>
        public DbSet<UserClient> UserClients { get; set; }
        /// <summary>Representa la colección de administradores de usuario en la base de datos.</summary>
        public DbSet<UserAdmin> UserAdmins { get; set; }

        /// <summary>Representa la colección de comunidades en la base de datos.</summary>
        public DbSet<Community> Communities { get; set; }
        /// <summary>Representa la colección de publicaciones en las comunidades en la base de datos.</summary>
        public DbSet<Post> Posts { get; set; }
        /// <summary>Representa la tabla de unión entre usuarios clientes y comunidades en la base de datos.</summary>
        public DbSet<UserCommunity> UserCommunities { get; set; }

        /// <summary>Representa la colección de notificaciones en la base de datos.</summary>
        public DbSet<Notification> Notifications { get; set; }
        
        public DbSet<Identity> Identities { get; set; }

        /// <summary>Representa la colección de reacciones a publicaciones en la base de datos.</summary>
        public DbSet<PostReaction> PostReactions { get; set; }
        
        /// <summary>Representa la colección de comentarios a publicaciones en la base de datos.</summary>
        public DbSet<Comment> Comments { get; set; }

        /// <summary>Representa la colección de movimientos de billetera en la base de datos.</summary>
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AppDbContext"/>.
        /// </summary>
        /// <param name="options">Las opciones de configuración para este contexto de base de datos.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Configura el modelo de datos que se utilizará para crear el esquema de la base de datos.
        /// Este método se llama una vez cuando se crea la instancia del contexto.
        /// </summary>
        /// <param name="modelBuilder">El constructor de modelos que se utiliza para configurar el modelo de la base de datos.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
                entity.Property(u => u.Display).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<UserClient>(entity =>
            {
                entity.ToTable("userclients");
                entity.Property(uc => uc.Icon).IsRequired(false);
                entity.Property(uc => uc.Phrase);
                entity.Property(uc => uc.Subscription);
                entity.Property(uc => uc.Wallet)
                    .IsRequired()
                    .HasColumnType("decimal(10, 2)")
                    .HasDefaultValue(0m);

                entity.HasBaseType<User>();

                entity.HasMany(uc => uc.FavoriteBooks)
                    .WithMany()
                    .UsingEntity(j => j.ToTable("user_favorite_books"));

                entity.HasMany(uc => uc.ExclusionBooks)
                    .WithMany()
                    .UsingEntity(j => j.ToTable("user_exclusion_books"));

                entity.HasMany(uc => uc.ReadBooks)
                    .WithMany()
                    .UsingEntity<Dictionary<string, object>>(
                        "user_read_books",
                        j => j.HasOne<Book>().WithMany().HasForeignKey("ReadBooksId"),
                        j => j.HasOne<UserClient>().WithMany().HasForeignKey("UserClientId"),
                        j => j.HasKey("ReadBooksId", "UserClientId"));
                
            });

            modelBuilder.Entity<UserAdmin>(entity =>
            {
                entity.ToTable("useradmins");
                entity.Property(ua => ua.AdminAccess).IsRequired();
                entity.Property(ua => ua.SecurityPin).HasMaxLength(255);
                entity.Property(ua => ua.Capital) 
                    .IsRequired()
                    .HasColumnType("decimal(10, 2)") 
                    .HasDefaultValue(500m);
                entity.HasBaseType<User>();
            });
            
            modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("books");
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Id).IsRequired().ValueGeneratedOnAdd();
                entity.Property(b => b.Title).IsRequired();
                entity.Property(b => b.Description).HasMaxLength(1000);
                entity.Property(b => b.Author).IsRequired().HasMaxLength(100);
                entity.Property(b => b.SalePrice).IsRequired().HasColumnType("decimal(10, 2)");
                entity.Property(b => b.PurchasePrice).IsRequired().HasColumnType("decimal(10, 2)");
                entity.Property(b => b.Stock).IsRequired();
                entity.Property(b => b.Cover);
                entity.Property(b => b.Genre).HasMaxLength(50);
                entity.Property(b => b.Language).HasMaxLength(50);
                entity.Property(b => b.IsActive).IsRequired().HasDefaultValue(true);
                
                entity.HasQueryFilter(b => b.IsActive);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("reviews");
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
                entity.Property(r => r.Content).IsRequired().HasMaxLength(1000);
                entity.Property(r => r.Username).IsRequired().HasMaxLength(100);

                entity.Property(r => r.BookId).IsRequired();
                entity.HasOne(r => r.Book)
                      .WithMany() 
                      .HasForeignKey(r => r.BookId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.Property(r => r.UserClientId).IsRequired();
                entity.HasOne(r => r.UserClient)
                      .WithMany() 
                      .HasForeignKey(r => r.UserClientId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("cart_items");
                entity.HasKey(ci => ci.Id);
                entity.Property(ci => ci.Id).IsRequired().ValueGeneratedOnAdd();
                entity.Property(ci => ci.Quantity).IsRequired();

                entity.Property(ci => ci.BookId).IsRequired();
                entity.HasOne(ci => ci.Book)
                      .WithMany()
                      .HasForeignKey(ci => ci.BookId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(ci => ci.UserClientId).IsRequired();
                entity.HasOne(ci => ci.UserClient)
                      .WithMany()
                      .HasForeignKey(ci => ci.UserClientId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(ci => new { ci.BookId, ci.UserClientId }).IsUnique();
                entity.HasQueryFilter(ci => ci.Book.IsActive);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).IsRequired().ValueGeneratedOnAdd();
                entity.Property(o => o.Code).IsRequired().HasMaxLength(6);
                entity.HasIndex(o => o.Code).IsUnique();

                entity.Property(o => o.UserClientId).IsRequired();
                entity.HasOne(o => o.UserClient)
                    .WithMany() 
                    .HasForeignKey(o => o.UserClientId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(o => o.UserEmail).IsRequired().HasMaxLength(100);
                entity.Property(o => o.UserPhone).IsRequired().HasMaxLength(20);
                entity.Property(o => o.UserFullName).IsRequired().HasMaxLength(255);
                entity.Property(o => o.RecipientName).IsRequired().HasMaxLength(255);
                entity.Property(o => o.Status).IsRequired().HasMaxLength(255);
                entity.Property(o => o.PaymentMethod).IsRequired().HasMaxLength(20).HasDefaultValue("external");
                entity.Property(o => o.IsDelivery).IsRequired();
                entity.Property(o => o.Total).IsRequired().HasColumnType("decimal(10, 2)");
                entity.Property(o => o.Date).IsRequired();


                entity.OwnsOne(o => o.Shipping, shipping =>
                {
                    shipping.Property(s => s.Address).IsRequired().HasMaxLength(255).HasColumnName("ShippingAddress");
                    shipping.Property(s => s.City).IsRequired().HasMaxLength(100).HasColumnName("ShippingCity");
                    shipping.Property(s => s.District).IsRequired().HasMaxLength(100).HasColumnName("ShippingDistrict");
                    shipping.Property(s => s.Reference).HasMaxLength(500).HasColumnName("ShippingReference");
                    shipping.Property(s => s.Price).HasColumnName("ShippingPrice");
                });


                entity.HasMany(o => o.Items)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.Id).IsRequired().ValueGeneratedOnAdd();

                entity.Property(oi => oi.BookId).IsRequired();
                entity.Property(oi => oi.BookTitle).IsRequired().HasMaxLength(255);
                entity.Property(oi => oi.BookAuthor).IsRequired().HasMaxLength(100);
                entity.Property(oi => oi.BookPrice).IsRequired().HasColumnType("decimal(10, 2)");
                entity.Property(oi => oi.BookCover);

                entity.Property(oi => oi.Quantity).IsRequired();
                entity.Property(oi => oi.ItemTotal).IsRequired().HasColumnType("decimal(10, 2)");

                entity.Property(oi => oi.OrderId).IsRequired();

            });


          
            modelBuilder.Entity<Community>(entity =>
            {
                entity.ToTable("communities");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).IsRequired().HasMaxLength(500);
                entity.Property(c => c.Type)
                      .IsRequired()
                      .HasConversion<string>(); 
                entity.Property(c => c.Image);
                entity.Property(c => c.Banner);

            });

            
            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("posts");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
                entity.Property(p => p.Username).IsRequired().HasMaxLength(50);
                entity.Property(p => p.Content).IsRequired().HasMaxLength(2000);
                entity.Property(p => p.Img);
                entity.Property(p => p.CreatedAt).IsRequired();
                entity.Property(p => p.CommunityId).IsRequired();
                entity.Property(p => p.UserId).IsRequired();
                entity.HasOne(p => p.Community)
                      .WithMany() 
                      .HasForeignKey(p => p.CommunityId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade); 
                entity.HasOne(p => p.UserClient)
                      .WithMany() 
                      .HasForeignKey(p => p.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); 
            });

            
            modelBuilder.Entity<UserCommunity>(entity =>
            {
                entity.ToTable("user_communities");
                entity.HasKey(uc => new { uc.UserClientId, uc.CommunityId });

                entity.Property(uc => uc.JoinedDate).IsRequired(); 

                entity.HasOne(uc => uc.UserClient)
                      .WithMany(u => u.UserCommunities)
                      .HasForeignKey(uc => uc.UserClientId)
                      .OnDelete(DeleteBehavior.Cascade); 

                entity.HasOne(uc => uc.Community)
                      .WithMany(c => c.UserCommunities)
                      .HasForeignKey(uc => uc.CommunityId)
                      .OnDelete(DeleteBehavior.Cascade); 
            });

            
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Id).IsRequired().ValueGeneratedOnAdd();

                entity.Property(n => n.CreatedAt).IsRequired();
                entity.Property(n => n.Title).IsRequired().HasMaxLength(100);
                entity.Property(n => n.Content).IsRequired().HasMaxLength(500);

                entity.Property(n => n.Type)
                      .IsRequired()
                      .HasConversion<string>(); 
            });
            
            modelBuilder.Entity<Identity>(entity =>
            {
                entity.ToTable("identities");

                entity.ToTable("Identities");
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(i => i.UserId)
                    .IsRequired();
                entity.HasIndex(i => i.UserId).IsUnique();

                entity.Property(i => i.UserName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.OwnsOne(i => i.HashedPassword, ph =>
                {
                    ph.Property(p => p.HashedValue)
                        .HasColumnName("hashed_password")
                        .IsRequired();
                });

                entity.HasIndex(i => i.UserName).IsUnique();
            });
            
            modelBuilder.Entity<PostReaction>(entity =>
            {
                entity.ToTable("post_reactions");
                entity.HasKey(pr => pr.Id);
                entity.Property(pr => pr.Id).IsRequired().ValueGeneratedOnAdd();
                
                entity.Property(pr => pr.UserId).IsRequired();
                entity.Property(pr => pr.PostId).IsRequired();
                
                entity.Property(pr => pr.Type)
                      .IsRequired()
                      .HasConversion<int>(); 

                entity.Property(pr => pr.UpdatedAt).IsRequired();
                
                entity.HasIndex(pr => new { pr.UserId, pr.PostId }).IsUnique();
                
                entity.HasOne<Post>()
                      .WithMany()
                      .HasForeignKey(pr => pr.PostId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<UserClient>()
                    .WithMany()
                    .HasForeignKey(pr => pr.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("comments");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
                
                entity.Property(c => c.PostId).IsRequired();
                entity.Property(c => c.UserId).IsRequired();
                
                entity.Property(c => c.Username).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Content).IsRequired().HasMaxLength(1000);
                entity.Property(c => c.Img).IsRequired();
                
                entity.Property(c => c.CreatedAt).IsRequired();
                
                entity.HasOne<Post>()
                      .WithMany()
                      .HasForeignKey(c => c.PostId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<UserClient>()
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WalletTransaction>(entity =>
            {
                entity.ToTable("wallet_transactions");
                entity.HasKey(wt => wt.Id);
                entity.Property(wt => wt.Id).IsRequired().ValueGeneratedOnAdd();
                entity.Property(wt => wt.UserClientId).IsRequired();
                entity.Property(wt => wt.Amount).IsRequired().HasColumnType("decimal(10, 2)");
                entity.Property(wt => wt.Type).IsRequired().HasConversion<string>();
                entity.Property(wt => wt.Status).IsRequired().HasConversion<string>();
                entity.Property(wt => wt.Reference).IsRequired().HasMaxLength(100);
                entity.Property(wt => wt.ProofUrl).IsRequired().HasMaxLength(500);
                entity.Property(wt => wt.OrderId).IsRequired(false);
                entity.Property(wt => wt.CreatedAt).IsRequired();
                entity.Property(wt => wt.ProcessedAt).IsRequired(false);
                entity.Property(wt => wt.AdminNote).IsRequired(false).HasMaxLength(500);

                entity.HasOne<UserClient>()
                    .WithMany()
                    .HasForeignKey(wt => wt.UserClientId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        /// <summary>
        /// Sobrescribe SaveChangesAsync para manejar automáticamente las propiedades CreatedAt y UpdatedAt
        /// para entidades que las tengan (asumiendo que heredan de una clase base como AuditableEntity
        /// o que estas propiedades existen).
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<dynamic>()) 
            {
                
                if (entry.State == EntityState.Added && entry.Metadata.FindProperty("CreatedAt") != null)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    if (entry.Metadata.FindProperty("UpdatedAt") != null)
                        entry.Property("UpdatedAt").CurrentValue = null;
                }
                else if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}