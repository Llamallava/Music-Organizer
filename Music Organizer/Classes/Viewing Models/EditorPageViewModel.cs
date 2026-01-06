using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Music_Organizer.Data;

namespace Music_Organizer.Classes
{
    public sealed class EditorPageViewModel : INotifyPropertyChanged
    {
        private string _albumTitle;
        private string _artistName;
        private ImageSource _coverImage;
        private TrackTabViewModel _selectedTab;
        private readonly Music_Organizer.Lyrics.ILyricsProvider _lyricsProvider;
        private System.Threading.CancellationTokenSource _lyricsCts;
        public RelayCommand ToggleInterludeCommand { get; }


        public EditorPageViewModel(System.Guid albumId) : this(albumId, new Music_Organizer.Lyrics.DefaultLyricsProvider())
        {
        }
        public EditorPageViewModel(System.Guid albumId, Music_Organizer.Lyrics.ILyricsProvider lyricsProvider)
        {
            AlbumId = albumId;
            Tabs = new System.Collections.ObjectModel.ObservableCollection<TrackTabViewModel>();
            SaveCommand = new RelayCommand(Save);
            ToggleInterludeCommand = new RelayCommand(ToggleInterlude);

            _lyricsProvider = lyricsProvider;

            LoadAlbumAndTabs(albumId);
        }

        public Guid AlbumId
        {
            get;
        }

        public ObservableCollection<TrackTabViewModel> Tabs
        {
            get;
        }

        public TrackTabViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (ReferenceEquals(value, _selectedTab))
                    return;

                _selectedTab = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanToggleInterlude));

                LoadLyricsForSelectedTab();
            }
        }


        public string AlbumTitle
        {
            get => _albumTitle;
            private set
            {
                if (value == _albumTitle)
                {
                    return;
                }

                _albumTitle = value;
                OnPropertyChanged();
            }
        }

        public string ArtistName
        {
            get => _artistName;
            private set
            {
                if (value == _artistName)
                {
                    return;
                }

                _artistName = value;
                OnPropertyChanged();
            }
        }

        public ImageSource CoverImage
        {
            get => _coverImage;
            private set
            {
                if (Equals(value, _coverImage))
                {
                    return;
                }

                _coverImage = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SaveCommand
        {
            get;
        }

        private void LoadAlbumAndTabs(Guid albumId)
        {
            using var db = new MusicOrganizerDbContext();

            var album = db.Albums.FirstOrDefault(a => a.AlbumId == albumId);
            if (album == null)
            {
                AlbumTitle = "Album not found";
                ArtistName = "";
                CoverImage = null;
                Tabs.Clear();
                SelectedTab = null;
                return;
            }

            AlbumTitle = album.AlbumTitle;
            ArtistName = album.ArtistName;

            var coverPath = Path.Combine(AppPaths.Covers, album.CoverFileName);
            CoverImage = File.Exists(coverPath) ? LoadImage(coverPath) : null;

            var tracks = db.Tracks
                .Where(t => t.AlbumId == albumId)
                .OrderBy(t => t.TrackNumber)
                .Select(t => new
                {
                    t.TrackId,
                    t.TrackNumber,
                    t.Title
                })
                .ToList();

            var existingTrackReviews = db.TrackReviews
                .Where(r => r.AlbumId == albumId)
                .ToList();

            var existingConclusion = db.AlbumConclusions
                .FirstOrDefault(c => c.AlbumId == albumId);

            Tabs.Clear();

            foreach (var t in tracks)
            {
                var display = t.TrackNumber.ToString() + ". " + t.Title;

                var tab = new TrackTabViewModel(
                    t.TrackId,
                    false,
                    display,
                    t.Title);

                var review = existingTrackReviews.FirstOrDefault(r => r.TrackId == t.TrackId);
                if (review != null)
                {
                    tab.Notes = review.Notes ?? "";
                    tab.ScoreText = review.Score.HasValue
                        ? review.Score.Value.ToString(CultureInfo.InvariantCulture)
                        : "";
                    tab.IsInterlude = review.IsInterlude;
                    if (tab.IsInterlude)
                        tab.ScoreText = "";
                }

                Tabs.Add(tab);
            }

            var conclusionTab = new TrackTabViewModel(null, true, "Conclusion", "");

            if (existingConclusion != null)
            {
                conclusionTab.Notes = existingConclusion.Notes ?? "";
                conclusionTab.ScoreText = existingConclusion.Score.HasValue
                    ? existingConclusion.Score.Value.ToString(CultureInfo.InvariantCulture)
                    : "";
            }

            Tabs.Add(conclusionTab);

            if (Tabs.Count > 0)
            {
                SelectedTab = Tabs[0];
            }
        }

        private void Save()
        {
            using var db = new MusicOrganizerDbContext();

            foreach (var tab in Tabs)
            {
                var parsedScore = ParseNullableScore(tab.ScoreText);

                if (tab.IsConclusion)
                {
                    var existing = db.AlbumConclusions.FirstOrDefault(c => c.AlbumId == AlbumId);
                    if (existing == null)
                    {
                        db.AlbumConclusions.Add(new AlbumConclusionEntity
                        {
                            AlbumId = AlbumId,
                            Notes = tab.Notes ?? "",
                            Score = parsedScore
                        });
                    }
                    else
                    {
                        existing.Notes = tab.Notes ?? "";
                        existing.Score = parsedScore;
                    }

                    continue;
                }

                if (!tab.TrackId.HasValue)
                {
                    continue;
                }

                var trackId = tab.TrackId.Value;

                var existingTrackReview = db.TrackReviews
                    .FirstOrDefault(r => r.AlbumId == AlbumId && r.TrackId == trackId);

                if (existingTrackReview == null)
                {
                    db.TrackReviews.Add(new TrackReviewEntity
                    {
                        TrackReviewId = Guid.NewGuid(),
                        AlbumId = AlbumId,
                        TrackId = trackId,
                        Notes = tab.Notes ?? "",
                        IsInterlude = tab.IsInterlude,
                        Score = tab.IsInterlude ? null : parsedScore
                    });
                }
                else
                {
                    existingTrackReview.Notes = tab.Notes ?? "";
                    existingTrackReview.IsInterlude = tab.IsInterlude;

                    existingTrackReview.Score = tab.IsInterlude
                        ? null
                        : parsedScore;
                }
            }

            db.SaveChanges();
        }

        private static double? ParseNullableScore(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            return null;
        }

        private static BitmapImage LoadImage(string path)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = 400;
            image.EndInit();
            image.Freeze();
            return image;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async void LoadLyricsForSelectedTab()
        {
            _lyricsCts?.Cancel();
            _lyricsCts = new System.Threading.CancellationTokenSource();

            var token = _lyricsCts.Token;

            var tab = _selectedTab;
            if (tab == null)
                return;

            if (tab.IsConclusion)
            {
                tab.Lyrics = "";
                return;
            }

            if (string.IsNullOrWhiteSpace(tab.TrackTitle) || string.IsNullOrWhiteSpace(ArtistName))
            {
                tab.Lyrics = "";
                return;
            }

            tab.Lyrics = "Loading lyrics...";

            try
            {
                var lyrics = await _lyricsProvider.GetLyricsAsync(tab.TrackTitle, ArtistName, token);

                if (token.IsCancellationRequested)
                    return;

                tab.Lyrics = lyrics;
            }
            catch (System.OperationCanceledException)
            {
                // ignore
            }
            catch
            {
                tab.Lyrics = "Lyrics unavailable.";
            }
        }
        public bool CanToggleInterlude
        {
            get
            {
                if (SelectedTab == null)
                    return false;

                return !SelectedTab.IsConclusion;
            }
        }

        private void ToggleInterlude()
        {
            if (SelectedTab == null)
                return;

            if (SelectedTab.IsConclusion)
                return;

            SelectedTab.IsInterlude = !SelectedTab.IsInterlude;

            if (SelectedTab.IsInterlude)
            {
                // Interludes cannot be scored.
                SelectedTab.ScoreText = "";
            }
        }

    }
}
