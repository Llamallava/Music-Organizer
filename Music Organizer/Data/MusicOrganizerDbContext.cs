using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Music_Organizer.Data
{
    public sealed class MusicOrganizerDbContext : DbContext
    {
        public DbSet<AlbumEntity> Albums { get; set; }

        public DbSet<TrackEntity> Tracks { get; set; }

        public DbSet<TrackReviewEntity> TrackReviews { get; set; }

        public DbSet<AlbumConclusionEntity> AlbumConclusions { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=" + AppPaths.Database);
            }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AlbumEntity>().HasKey(a => a.AlbumId);
            modelBuilder.Entity<TrackEntity>().HasKey(t => t.TrackId);

            modelBuilder.Entity<TrackReviewEntity>().HasKey(r => r.TrackReviewId);
            modelBuilder.Entity<AlbumConclusionEntity>().HasKey(c => c.AlbumId);

            modelBuilder.Entity<TrackEntity>()
                .HasOne(t => t.Album)
                .WithMany(a => a.Tracks)
                .HasForeignKey(t => t.AlbumId);

            modelBuilder.Entity<TrackReviewEntity>()
                .HasOne(r => r.Album)
                .WithMany(a => a.TrackReviews)
                .HasForeignKey(r => r.AlbumId);

            modelBuilder.Entity<TrackReviewEntity>()
                .HasOne(r => r.Track)
                .WithOne(t => t.Review)
                .HasForeignKey<TrackReviewEntity>(r => r.TrackId);

            modelBuilder.Entity<AlbumConclusionEntity>()
                .HasOne(c => c.Album)
                .WithOne(a => a.Conclusion)
                .HasForeignKey<AlbumConclusionEntity>(c => c.AlbumId);

            modelBuilder.Entity<TrackReviewEntity>()
                .HasIndex(r => new
                {
                    r.AlbumId,
                    r.TrackId
                })
                .IsUnique();

            modelBuilder.Entity<AlbumEntity>()
                .Property(a => a.AlbumTitle)
                .IsRequired();

            modelBuilder.Entity<AlbumEntity>()
                .Property(a => a.ArtistName)
                .IsRequired();

            modelBuilder.Entity<TrackEntity>()
                .Property(t => t.Title)
                .IsRequired();

            modelBuilder.Entity<TrackReviewEntity>()
                .Property(r => r.IsInterlude)
                .HasDefaultValue(false);

        }

    }
}
