using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Music_Organizer.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace Music_Organizer.Classes.Viewing_Models
{
    public sealed class StatsPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private RankedAlbumRow _topConclusionWinner;
        public RankedAlbumRow TopConclusionWinner
        {
            get => _topConclusionWinner;
            private set
            {
                _topConclusionWinner = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TopConclusionWinner)));
            }
        }

        private RankedAlbumRow _topComputedWinner;
        public RankedAlbumRow TopComputedWinner
        {
            get => _topComputedWinner;
            private set
            {
                _topComputedWinner = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TopComputedWinner)));
            }
        }

        public StatsPageViewModel()
        {
            TopAlbumsByConclusionScore = new ObservableCollection<RankedAlbumRow>();
            TopAlbumsByComputedScore = new ObservableCollection<RankedAlbumRow>();
            TopSongsByScore = new ObservableCollection<RankedSongRow>();

            TopAlbumsByWords = new ObservableCollection<RankedAlbumWordCountRow>();
            TopSongsByWords = new ObservableCollection<RankedSongWordCountRow>();
            TotalWordsWrittenText = "0";

            Load();
        }

        public ObservableCollection<RankedAlbumRow> TopAlbumsByConclusionScore { get; }
        public ObservableCollection<RankedAlbumRow> TopAlbumsByComputedScore { get; }
        public ObservableCollection<RankedSongRow> TopSongsByScore { get; }

        public ObservableCollection<RankedAlbumWordCountRow> TopAlbumsByWords { get; }
        public ObservableCollection<RankedSongWordCountRow> TopSongsByWords { get; }

        private string _totalWordsWrittenText;
        public string TotalWordsWrittenText
        {
            get => _totalWordsWrittenText;
            private set
            {
                _totalWordsWrittenText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalWordsWrittenText)));
            }
        }

        private void Load()
        {
            using var db = new MusicOrganizerDbContext();

            var albums = db.Albums
                .Select(a => new
                {
                    a.AlbumId,
                    a.AlbumTitle,
                    a.ArtistName,
                    CoverImagePath = Path.Combine(AppPaths.Covers, a.CoverFileName)
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
                    r.Score,
                    r.IsInterlude
                })
                .ToList();

            var conclusions = db.AlbumConclusions
                .Select(c => new
                {
                    c.AlbumId,
                    c.Score
                })
                .ToList();

            // Notes used for word counts
            var conclusionNotes = db.AlbumConclusions
                .Select(c => new
                {
                    c.AlbumId,
                    c.Notes
                })
                .ToList();

            var trackReviewNotes = db.TrackReviews
                .Select(r => new
                {
                    r.AlbumId,
                    r.TrackId,
                    r.Notes,
                    r.IsInterlude
                })
                .ToList();

            var albumDisplayById = albums.ToDictionary(
                a => a.AlbumId,
                a => a.AlbumTitle + " - " + a.ArtistName
            );

            var albumCoverPathById = albums.ToDictionary(
                a => a.AlbumId,
                a => a.CoverImagePath
            );

            ImageSource TryLoadCover(Guid albumId)
            {
                if (!albumCoverPathById.TryGetValue(albumId, out var path))
                    return null;

                if (string.IsNullOrWhiteSpace(path))
                    return null;

                if (!File.Exists(path))
                    return null;

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(path, UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }

            static int CountWords(string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                    return 0;

                return Regex.Matches(text, @"[\p{L}\p{N}]+").Count;
            }

            // -----------------------------
            // 1) Top 10 albums by conclusion score
            // -----------------------------
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
                    TracksUsedText = "",
                    CoverImage = TryLoadCover(row.AlbumId)
                });
            }

            TopConclusionWinner = TopAlbumsByConclusionScore.FirstOrDefault();

            // -----------------------------
            // 2) Top 10 albums by computed score (avg of song scores), excluding interludes
            // -----------------------------
            var computed = trackReviews
                .Where(r => r.Score.HasValue && !r.IsInterlude)
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
                    TracksUsedText = row.TracksUsed.ToString(CultureInfo.InvariantCulture),
                    CoverImage = TryLoadCover(row.AlbumId)
                });
            }

            TopComputedWinner = TopAlbumsByComputedScore.FirstOrDefault();

            // -----------------------------
            // 3) Top 10 songs by score (excluding interludes)
            // -----------------------------
            var trackTitleById = tracks.ToDictionary(
                t => t.TrackId,
                t => t.Title
            );

            var topSongs = trackReviews
                .Where(r => r.Score.HasValue && !r.IsInterlude)
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

            // -----------------------------
            // WORD COUNT AGGREGATION (used by multiple modules)
            // Album total = conclusion notes + ALL track review notes for that album
            // -----------------------------
            var wordsByAlbumFromTracks = trackReviewNotes
                .GroupBy(x => x.AlbumId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(v => CountWords(v.Notes))
                );

            var wordsByAlbumFromConclusion = conclusionNotes
                .ToDictionary(
                    x => x.AlbumId,
                    x => CountWords(x.Notes)
                );

            var totalWordsByAlbum = albums
                .Select(a =>
                {
                    wordsByAlbumFromTracks.TryGetValue(a.AlbumId, out var trackWords);
                    wordsByAlbumFromConclusion.TryGetValue(a.AlbumId, out var conclusionWords);

                    return new
                    {
                        a.AlbumId,
                        TotalWords = trackWords + conclusionWords
                    };
                })
                .ToList();

            // -----------------------------
            // 4) Top 5 albums by words written
            // -----------------------------
            var topAlbumWords = totalWordsByAlbum
                .OrderByDescending(x => x.TotalWords)
                .Take(5)
                .ToList();

            TopAlbumsByWords.Clear();

            for (int i = 0; i < topAlbumWords.Count; i++)
            {
                var row = topAlbumWords[i];

                TopAlbumsByWords.Add(new RankedAlbumWordCountRow
                {
                    Rank = i + 1,
                    AlbumId = row.AlbumId,
                    AlbumDisplay = albumDisplayById.TryGetValue(row.AlbumId, out var display)
                        ? display
                        : "Unknown album",
                    WordCount = row.TotalWords,
                    WordCountText = row.TotalWords.ToString(CultureInfo.InvariantCulture),
                    CoverImage = TryLoadCover(row.AlbumId)
                });
            }

            // -----------------------------
            // 5) Top 5 songs by words written
            // (keeps your earlier choice: exclude interludes)
            // -----------------------------
            var topSongWords = trackReviewNotes
                .Where(x => !x.IsInterlude)
                .Select(x => new
                {
                    x.TrackId,
                    x.AlbumId,
                    Words = CountWords(x.Notes)
                })
                .OrderByDescending(x => x.Words)
                .Take(5)
                .ToList();

            TopSongsByWords.Clear();

            for (int i = 0; i < topSongWords.Count; i++)
            {
                var row = topSongWords[i];

                TopSongsByWords.Add(new RankedSongWordCountRow
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
                    WordCount = row.Words,
                    WordCountText = row.Words.ToString(CultureInfo.InvariantCulture)
                });
            }

            // -----------------------------
            // 6) Grand total words written
            // -----------------------------
            var grandTotal = totalWordsByAlbum.Sum(x => x.TotalWords);
            TotalWordsWrittenText = grandTotal.ToString(CultureInfo.InvariantCulture);
        }
    }
}
