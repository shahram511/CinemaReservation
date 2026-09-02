using CinemaReservation.Core.Entities;
using CinemaReservation.Core.Enums;
using Microsoft.EntityFrameworkCore;


namespace CinemaReservation.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // The Tables(DbSets)
        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationSeat> ReservationSeats { get; set; }

        // Configure the database schema using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration------
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);

                // Ensures no two users can have the same username
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Showtime configuration-------
            modelBuilder.Entity<Showtime>(entity =>
            {
                entity.ToTable("Showtimes");                
                entity.HasKey(e => e.Id);
                entity.Property(s => s.StartTime).IsRequired();                

                // Define the many to one relationship from showtime back to movie
                entity.HasOne(s => s.Movie)
                    .WithMany(m => m.Showtimes)
                    .HasForeignKey(s => s.MovieId)
                    .OnDelete(DeleteBehavior.Cascade); // delete the showtime if a movie is deleted

            });

            // Movie configuration-------------------
            modelBuilder.Entity<Movie>(entity =>
            {
                entity.ToTable("Movies");
                entity.HasKey(e => e.Id);
                entity.Property(m => m.Title).IsRequired().HasMaxLength(200);
                entity.Property(m => m.Description).IsRequired().HasMaxLength(1000);
                entity.Property(m => m.PosterUrl).IsRequired();
                entity.Property(m => m.Genre).IsRequired().HasMaxLength(100);
                entity.Property(m => m.DurationInMinutes).IsRequired();

                // Defien the 1 to many relationship between Movie and Showtime
                // If a movie is deleted, EF Core will automatically cascade the delete to its showtime
                entity.HasMany(m => m.Showtimes).WithOne(s => s.Movie).HasForeignKey(s => s.MovieId).OnDelete(DeleteBehavior.Cascade);
            });
            

            // Reservatin configuration------------------
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);

                // One to many: one user has many reservations
                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // dont delet reservations if a user is deleted
            });

            // ReservationSeat  configuration---------------
            modelBuilder.Entity<ReservationSeat>(entity =>
            {
                entity.HasKey(rs => rs.ID);

                // Apply the Filtered Index: Enforce uniqueness ONLY if the seate is not cancelled
                // NOTE: PostgreSQL requires double qoutes around coulmn names if they match reserved keywords
                entity.HasIndex(rs => new { rs.ShowtimeId, rs.SeatId })
                    .IsUnique()
                    .HasFilter("\"Status\"!=2");

                // many to one  relationship from reservationseat to reservation
                entity.HasOne(rs => rs.Reservation)
                    .WithMany(r => r.ReservationSeats)
                    .HasForeignKey(rs => rs.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade); // if a reservation is deleted cascade delete these junction rows

                // Define many to one relationship from reservationseat to seat
                entity.HasOne(rs => rs.Seat)
                    .WithMany(s => s.ReservationSeats)
                    .HasForeignKey(rs => rs.SeatId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Showtime>()
                    .WithMany()
                    .HasForeignKey(rs => rs.ShowtimeId)
                    .OnDelete(DeleteBehavior.Cascade); // restrict deletion : never delete a physical seat  just beacuse a reservation was cancelled


                // Concurrency Token mapping for PostgreSQL (Uses an internal hidden column called xmin)
                entity.Property(rs => rs.Version)
                    .IsRowVersion(); // Tell EF Core to use PostgreSQL's internal row versioning (xmin) for optimistic concuren
                    
            });
        

            // Seed data for initial admin Account
            var adminId = Guid.Parse("a1b2c3d4-e5f6-7a8b-9c0d-123456789abc");

            modelBuilder.Entity<User>().HasData(new User()
            {
                Id = adminId,
                Username = "shahram",
                Email = "shsharamazhgandi@gmail.com",
                Role = Enums.UserRole.Admin,
                PasswordHash = "$2a$12$hlNKXUY.A18uzOvU6UcXlujuHZA.vEEILXiQRWVBVErfNr0dMa8jG", // Use BCrypt to hash the actual password
                CreatedAt = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc)
            });

        }
    }
}              