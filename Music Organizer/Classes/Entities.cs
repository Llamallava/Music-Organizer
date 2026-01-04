using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Music_Organizer.Classes
{
    public sealed class AlbumEntity
    {
        public Guid AlbumId { get; set; }
        public string AlbumTitle { get; set; }
        public string ArtistName { get; set; }
        public string CoverFileName { get; set; }

        public List<TrackEntity> Tracks { get; set; }
    }

    public sealed class TrackEntity
    {
        public Guid TrackId { get; set; }
        public Guid AlbumId { get; set; }

        public int TrackNumber { get; set; }
        public string Title { get; set; }

        public AlbumEntity Album { get; set; }
    }

    public sealed class MusicOrganizerDbContext : DbContext
    {
        public DbSet<AlbumEntity> Albums { get; set; }
        public DbSet<TrackEntity> Tracks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={AppPaths.Database}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AlbumEntity>()
                .HasKey(a => a.AlbumId);

            modelBuilder.Entity<TrackEntity>()
                .HasKey(t => t.TrackId);

            modelBuilder.Entity<TrackEntity>()
                .HasOne(t => t.Album)
                .WithMany(a => a.Tracks)
                .HasForeignKey(t => t.AlbumId);

            modelBuilder.Entity<AlbumEntity>()
                .HasIndex(a => new { a.AlbumTitle, a.ArtistName })
                .IsUnique(false);
        }
    }
}
