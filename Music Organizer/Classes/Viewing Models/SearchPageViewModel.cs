using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Music_Organizer.Data;

namespace Music_Organizer.Classes.Viewing_Models
{
    public sealed class SearchPageViewModel : INotifyPropertyChanged
    {
        private readonly List<AlbumLite> _albums;
        private readonly List<TrackLite> _tracks;
        private readonly List<TrackReviewLite> _trackReviews;
        private readonly List<ConclusionLite> _conclusions;

        private string _songsMinScoreText;
        private string _songsMaxScoreText;
        private bool _songsIncludeInterludes;
        private string _songsTextFilter;
        private string _songsNotesFilter;
        private string _songsStatusText;

        private string _albumsMinConclusionScoreText;
        private string _albumsMinComputedScoreText;
        private string _albumsTextFilter;
        private string _albumsStatusText;

        private SongSearchRow _selectedSongResult;
        private AlbumSearchRow _selectedAlbumResult;

        public SearchPageViewModel()
        {
            SongResults = new ObservableCollection<SongSearchRow>();
            AlbumResults = new ObservableCollection<AlbumSearchRow>();

            ApplySongsFiltersCommand = new RelayCommand(ApplySongsFilters);
            ApplyAlbumsFiltersCommand = new RelayCommand(ApplyAlbumsFilters);

            _songsMinScoreText = "";
            _songsMaxScoreText = "";
            _songsIncludeInterludes = false;
            _songsTextFilter = "";
            _songsNotesFilter = "";

            _albumsMinConclusionScoreText = "";
            _albumsMinComputedScoreText = "";
            _albumsTextFilter = "";

            using var db = new MusicOrganizerDbContext();

            _albums = db.Albums
                .Select(a => new AlbumLite(a.AlbumId, a.AlbumTitle, a.ArtistName))
                .ToList();

            _tracks = db.Tracks
                .Select(t => new TrackLite(t.TrackId, t.AlbumId, t.TrackNumber, t.Title))
                .ToList();

            _trackReviews = db.TrackReviews
                .Select(r => new TrackReviewLite(r.TrackId, r.AlbumId, r.Score, r.Notes, r.IsInterlude))
                .ToList();

            _conclusions = db.AlbumConclusions
                .Select(c => new ConclusionLite(c.AlbumId, c.Score))
                .ToList();

            ApplySongsFilters();
            ApplyAlbumsFilters();
        }

        public ObservableCollection<SongSearchRow> SongResults { get; }

        public ObservableCollection<AlbumSearchRow> AlbumResults { get; }

        public RelayCommand ApplySongsFiltersCommand { get; }

        public RelayCommand ApplyAlbumsFiltersCommand { get; }

        public SongSearchRow SelectedSongResult
        {
            get => _selectedSongResult;
            set
            {
                if (ReferenceEquals(value, _selectedSongResult))
                    return;

                _selectedSongResult = value;
                OnPropertyChanged();
            }
        }

        public AlbumSearchRow SelectedAlbumResult
        {
            get => _selectedAlbumResult;
            set
            {
                if (ReferenceEquals(value, _selectedAlbumResult))
                    return;

                _selectedAlbumResult = value;
                OnPropertyChanged();
            }
        }

        public string SongsMinScoreText
        {
            get => _songsMinScoreText;
            set
            {
                if (value == _songsMinScoreText)
                    return;

                _songsMinScoreText = value;
                OnPropertyChanged();
            }
        }

        public string SongsMaxScoreText
        {
            get => _songsMaxScoreText;
            set
            {
                if (value == _songsMaxScoreText)
                    return;

                _songsMaxScoreText = value;
                OnPropertyChanged();
            }
        }

        public bool SongsIncludeInterludes
        {
            get => _songsIncludeInterludes;
            set
            {
                if (value == _songsIncludeInterludes)
                    return;

                _songsIncludeInterludes = value;
                OnPropertyChanged();
            }
        }

        public string SongsTextFilter
        {
            get => _songsTextFilter;
            set
            {
                if (value == _songsTextFilter)
                    return;

                _songsTextFilter = value;
                OnPropertyChanged();
            }
        }

        public string SongsNotesFilter
        {
            get => _songsNotesFilter;
            set
            {
                if (value == _songsNotesFilter)
                    return;

                _songsNotesFilter = value;
                OnPropertyChanged();
            }
        }

        public string SongsStatusText
        {
            get => _songsStatusText;
            private set
            {
                if (value == _songsStatusText)
                    return;

                _songsStatusText = value;
                OnPropertyChanged();
            }
        }

        public string AlbumsMinConclusionScoreText
        {
            get => _albumsMinConclusionScoreText;
            set
            {
                if (value == _albumsMinConclusionScoreText)
                    return;

                _albumsMinConclusionScoreText = value;
                OnPropertyChanged();
            }
        }

        public string AlbumsMinComputedScoreText
        {
            get => _albumsMinComputedScoreText;
            set
            {
                if (value == _albumsMinComputedScoreText)
                    return;

                _albumsMinComputedScoreText = value;
                OnPropertyChanged();
            }
        }

        public string AlbumsTextFilter
        {
            get => _albumsTextFilter;
            set
            {
                if (value == _albumsTextFilter)
                    return;

                _albumsTextFilter = value;
                OnPropertyChanged();
            }
        }

        public string AlbumsStatusText
        {
            get => _albumsStatusText;
            private set
            {
                if (value == _albumsStatusText)
                    return;

                _albumsStatusText = value;
                OnPropertyChanged();
            }
        }

        private void ApplySongsFilters()
        {
            double? minScore = TryParseNullableDouble(SongsMinScoreText);
            double? maxScore = TryParseNullableDouble(SongsMaxScoreText);

            var textFilter = (SongsTextFilter ?? "").Trim();
            var notesFilter = (SongsNotesFilter ?? "").Trim();

            var albumDisplayById = _albums.ToDictionary(
                a => a.AlbumId,
                a => a.AlbumTitle + " - " + a.ArtistName
            );

            var trackTitleById = _tracks.ToDictionary(
                t => t.TrackId,
                t => t.Title
            );

            IEnumerable<TrackReviewLite> query = _trackReviews;

            query = query.Where(r => r.Score.HasValue);

            if (!SongsIncludeInterludes)
                query = query.Where(r => !r.IsInterlude);

            if (minScore.HasValue)
                query = query.Where(r => r.Score.Value >= minScore.Value);

            if (maxScore.HasValue)
                query = query.Where(r => r.Score.Value <= maxScore.Value);

            if (!string.IsNullOrWhiteSpace(textFilter))
            {
                query = query.Where(r =>
                {
                    var song = trackTitleById.TryGetValue(r.TrackId, out var title) ? title : "";
                    var album = albumDisplayById.TryGetValue(r.AlbumId, out var display) ? display : "";
                    return ContainsIgnoreCase(song, textFilter) || ContainsIgnoreCase(album, textFilter);
                });
            }

            if (!string.IsNullOrWhiteSpace(notesFilter))
            {
                query = query.Where(r =>
                {
                    var notes = r.Notes ?? "";
                    return ContainsIgnoreCase(notes, notesFilter);
                });
            }

            var list = query
                .OrderByDescending(r => r.Score.Value)
                .Take(500)
                .ToList();

            SongResults.Clear();

            for (int i = 0; i < list.Count; i++)
            {
                var r = list[i];

                SongResults.Add(new SongSearchRow
                {
                    Rank = i + 1,
                    AlbumId = r.AlbumId,
                    TrackId = r.TrackId,
                    SongTitle = trackTitleById.TryGetValue(r.TrackId, out var title) ? title : "Unknown track",
                    AlbumDisplay = albumDisplayById.TryGetValue(r.AlbumId, out var display) ? display : "Unknown album",
                    Score = r.Score.Value,
                    ScoreText = r.Score.Value.ToString("0.##", CultureInfo.InvariantCulture)
                });
            }

            SongsStatusText = "Showing " + list.Count.ToString(CultureInfo.InvariantCulture) + " results";
        }

        private void ApplyAlbumsFilters()
        {
            double? minConclusion = TryParseNullableDouble(AlbumsMinConclusionScoreText);
            double? minComputed = TryParseNullableDouble(AlbumsMinComputedScoreText);
            var textFilter = (AlbumsTextFilter ?? "").Trim();

            var albumDisplayById = _albums.ToDictionary(
                a => a.AlbumId,
                a => a.AlbumTitle + " - " + a.ArtistName
            );

            var conclusionByAlbum = _conclusions.ToDictionary(
                c => c.AlbumId,
                c => c.Score
            );

            var computedByAlbum = _trackReviews
                .Where(r => r.Score.HasValue && !r.IsInterlude)
                .GroupBy(r => r.AlbumId)
                .Select(g => new
                {
                    AlbumId = g.Key,
                    TracksUsed = g.Count(),
                    Avg = g.Average(x => x.Score.Value)
                })
                .ToDictionary(x => x.AlbumId, x => new { x.Avg, x.TracksUsed });

            var rows = new List<AlbumSearchRow>();

            foreach (var album in _albums)
            {
                var display = albumDisplayById.TryGetValue(album.AlbumId, out var d) ? d : "Unknown album";

                if (!string.IsNullOrWhiteSpace(textFilter) && !ContainsIgnoreCase(display, textFilter))
                    continue;

                var conclusionScore = conclusionByAlbum.TryGetValue(album.AlbumId, out var cs) ? cs : null;

                double? computedScore = null;
                int tracksUsed = 0;

                if (computedByAlbum.TryGetValue(album.AlbumId, out var computed))
                {
                    computedScore = computed.Avg;
                    tracksUsed = computed.TracksUsed;
                }

                if (minConclusion.HasValue)
                {
                    if (!conclusionScore.HasValue || conclusionScore.Value < minConclusion.Value)
                        continue;
                }

                if (minComputed.HasValue)
                {
                    if (!computedScore.HasValue || computedScore.Value < minComputed.Value)
                        continue;
                }

                rows.Add(new AlbumSearchRow
                {
                    Rank = 0,
                    AlbumId = album.AlbumId,
                    AlbumDisplay = display,
                    ConclusionScore = conclusionScore,
                    ConclusionScoreText = conclusionScore.HasValue
                        ? conclusionScore.Value.ToString("0.##", CultureInfo.InvariantCulture)
                        : "",
                    ComputedScore = computedScore,
                    ComputedScoreText = computedScore.HasValue
                        ? computedScore.Value.ToString("0.##", CultureInfo.InvariantCulture)
                        : "",
                    TracksUsed = tracksUsed,
                    TracksUsedText = tracksUsed > 0 ? tracksUsed.ToString(CultureInfo.InvariantCulture) : ""
                });
            }

            rows = rows
                .OrderByDescending(r => r.ConclusionScore ?? -1)
                .ThenByDescending(r => r.ComputedScore ?? -1)
                .Take(500)
                .ToList();

            AlbumResults.Clear();

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];

                AlbumResults.Add(new AlbumSearchRow
                {
                    Rank = i + 1,
                    AlbumId = r.AlbumId,
                    AlbumDisplay = r.AlbumDisplay,
                    ConclusionScore = r.ConclusionScore,
                    ConclusionScoreText = r.ConclusionScoreText,
                    ComputedScore = r.ComputedScore,
                    ComputedScoreText = r.ComputedScoreText,
                    TracksUsed = r.TracksUsed,
                    TracksUsedText = r.TracksUsedText
                });
            }

            AlbumsStatusText = "Showing " + rows.Count.ToString(CultureInfo.InvariantCulture) + " results";
        }

        private static bool ContainsIgnoreCase(string haystack, string needle)
        {
            if (haystack == null || needle == null)
                return false;

            return haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static double? TryParseNullableDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                return v;

            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private sealed record AlbumLite(Guid AlbumId, string AlbumTitle, string ArtistName);

        private sealed record TrackLite(Guid TrackId, Guid AlbumId, int TrackNumber, string Title);

        private sealed record TrackReviewLite(Guid TrackId, Guid AlbumId, double? Score, string Notes, bool IsInterlude);

        private sealed record ConclusionLite(Guid AlbumId, double? Score);


    }

    public sealed class SongSearchRow
    {
        public int Rank { get; init; }
        public Guid AlbumId { get; init; }
        public Guid TrackId { get; init; }
        public string SongTitle { get; init; }
        public string AlbumDisplay { get; init; }
        public double Score { get; init; }
        public string ScoreText { get; init; }
    }

    public sealed class AlbumSearchRow
    {
        public int Rank { get; init; }
        public Guid AlbumId { get; init; }
        public string AlbumDisplay { get; init; }
        public double? ConclusionScore { get; init; }
        public string ConclusionScoreText { get; init; }
        public double? ComputedScore { get; init; }
        public string ComputedScoreText { get; init; }
        public int TracksUsed { get; init; }
        public string TracksUsedText { get; init; }
    }
}
