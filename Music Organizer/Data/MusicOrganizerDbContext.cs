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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=" + AppPaths.Database);
            }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AlbumEntity>().HasKey(a => a.AlbumId);
            modelBuilder.Entity<TrackEntity>().HasKey(t => t.TrackId);


            modelBuilder.Entity<TrackEntity>()
                .HasOne(t => t.Album)
                .WithMany(a => a.Tracks)
                .HasForeignKey(t => t.AlbumId);


            modelBuilder.Entity<AlbumEntity>()
                .Property(a => a.AlbumTitle)
                .IsRequired();


            modelBuilder.Entity<AlbumEntity>()
                .Property(a => a.ArtistName)
                .IsRequired();


            modelBuilder.Entity<TrackEntity>()
                .Property(t => t.Title)
                .IsRequired();
        }
    }
}
