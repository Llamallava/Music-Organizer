using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Music_Organizer.Data;

namespace Music_Organizer.Classes.Viewing_Models
{
    public sealed class StatsPageViewModel
    {
        public StatsPageViewModel()
        {
            TopAlbumsByConclusionScore = new ObservableCollection<RankedAlbumRow>();
            TopAlbumsByComputedScore = new ObservableCollection<RankedAlbumRow>();
            TopSongsByScore = new ObservableCollection<RankedSongRow>();

            Load();
        }

        public ObservableCollection<RankedAlbumRow> TopAlbumsByConclusionScore { get; }

        public ObservableCollection<RankedAlbumRow> TopAlbumsByComputedScore { get; }

        public ObservableCollection<RankedSongRow> TopSongsByScore { get; }

        private void Load()
        {
            using var db = new MusicOrganizerDbContext();

            // Load base metadata
            var albums = db.Albums
                .Select(a => new
                {
                    a.AlbumId,
                    a.AlbumTitle,
                    a.ArtistName
                })
                .ToList();

            var tracks = db.Tracks
                .Select(t => new
                {
                    t.TrackId,
                    t.AlbumId,
                    t.TrackNumber,
                    t.Title
                })
                .ToList();

            var trackReviews = db.TrackReviews
                .Select(r => new
                {
                    r.TrackId,
                    r.AlbumId,
                    r.Score
                })
                .ToList();

            var conclusions = db.AlbumConclusions
                .Select(c => new
                {
                    c.AlbumId,
                    c.Score
                })
                .ToList();

            // Index helpers
            var albumDisplayById = albums.ToDictionary(
                a => a.AlbumId,
                a => a.AlbumTitle + " - " + a.ArtistName
            );

            // 1) Top 10 albums by conclusion score (user entered)
            var topConclusion = conclusions
                .Where(c => c.Score.HasValue)
                .Select(c => new
                {
                    c.AlbumId,
                    Score = c.Score.Value
                })
                .OrderByDescending(x => x.Score)
                .Take(10)
                .ToList();

            TopAlbumsByConclusionScore.Clear();

            for (int i = 0; i < topConclusion.Count; i++)
            {
                var row = topConclusion[i];

                TopAlbumsByConclusionScore.Add(new RankedAlbumRow
                {
                    Rank = i + 1,
                    AlbumId = row.AlbumId,
                    AlbumDisplay = albumDisplayById.TryGetValue(row.AlbumId, out var display)
                        ? display
                        : "Unknown album",
                    Score = row.Score,
                    ScoreText = row.Score.ToString("0.##", CultureInfo.InvariantCulture),
                    TracksUsed = 0,
                    TracksUsedText = ""
                });
            }

            // 2) Top 10 albums by computed score (avg of song scores)
            // Only include tracks that actually have a score.
            var computed = trackReviews
                .Where(r => r.Score.HasValue)
                .GroupBy(r => r.AlbumId)
                .Select(g => new
                {
                    AlbumId = g.Key,
                    TracksUsed = g.Count(),
                    Avg = g.Average(x => x.Score.Value)
                })
                .OrderByDescending(x => x.Avg)
                .Take(10)
                .ToList();

            TopAlbumsByComputedScore.Clear();

            for (int i = 0; i < computed.Count; i++)
            {
                var row = computed[i];

                TopAlbumsByComputedScore.Add(new RankedAlbumRow
                {
                    Rank = i + 1,
                    AlbumId = row.AlbumId,
                    AlbumDisplay = albumDisplayById.TryGetValue(row.AlbumId, out var display)
                        ? display
                        : "Unknown album",
                    Score = row.Avg,
                    ScoreText = row.Avg.ToString("0.##", CultureInfo.InvariantCulture),
                    TracksUsed = row.TracksUsed,
                    TracksUsedText = row.TracksUsed.ToString(CultureInfo.InvariantCulture)
                });
            }

            // 3) Top 10 songs by score
            // Join reviews to track title + album display
            var trackTitleById = tracks.ToDictionary(
                t => t.TrackId,
                t => t.Title
            );

            var topSongs = trackReviews
                .Where(r => r.Score.HasValue)
                .Select(r => new
                {
                    r.TrackId,
                    r.AlbumId,
                    Score = r.Score.Value
                })
                .OrderByDescending(x => x.Score)
                .Take(10)
                .ToList();

            TopSongsByScore.Clear();

            for (int i = 0; i < topSongs.Count; i++)
            {
                var row = topSongs[i];

                TopSongsByScore.Add(new RankedSongRow
                {
                    Rank = i + 1,
                    TrackId = row.TrackId,
                    AlbumId = row.AlbumId,
                    SongTitle = trackTitleById.TryGetValue(row.TrackId, out var title)
                        ? title
                        : "Unknown track",
                    AlbumDisplay = albumDisplayById.TryGetValue(row.AlbumId, out var display)
                        ? display
                        : "Unknown album",
                    Score = row.Score,
                    ScoreText = row.Score.ToString("0.##", CultureInfo.InvariantCulture)
                });
            }
        }
    }
}
