using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyModel;

namespace Music_Organizer
{
    /// <summary>
    /// Interaction logic for AddAlbumPage.xaml
    /// </summary>
    public partial class AddAlbumPage : Page
    {
        private readonly AlbumMetadataFetcher _fetcher;
        private CancellationTokenSource _cts;
        private readonly AlbumLibraryService _library;
        private FetchedAlbumData _lastFetched;
        private IReadOnlyList<ReleaseCandidate> _lastCandidates;
        private int _altIndex;

        public AddAlbumPage()
        {
            InitializeComponent();
            _fetcher = new AlbumMetadataFetcher();
            _library = new AlbumLibraryService();
        }

        private async void Fetch_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            StatusText.Text = "Searching...";
            CoverImage.Source = null;
            TracksList.ItemsSource = null;
            AlternativesList.ItemsSource = null;
            AltStatus.Text = "";
            _lastFetched = null;

            try
            {
                var artist = ArtistBox.Text?.Trim();
                var album = AlbumBox.Text?.Trim();

                _lastCandidates = await _fetcher.SearchCandidatesAsync(artist, album, _cts.Token);

                if (_lastCandidates == null || _lastCandidates.Count == 0)
                {
                    StatusText.Text = "No matches found.";
                    return;
                }

                _altIndex = 1;

                StatusText.Text = "Fetching best match...";
                var best = _lastCandidates[0];

                var data = await _fetcher.FetchByReleaseIdAsync(best.Id, _cts.Token);

                data = new FetchedAlbumData
                {
                    AlbumTitle = best.Title,
                    ArtistName = string.IsNullOrWhiteSpace(best.ArtistCredit) ? artist : best.ArtistCredit,
                    ReleaseMbid = best.Id,
                    Tracks = data.Tracks,
                    CoverBytes = data.CoverBytes,
                    CoverImage = data.CoverImage
                };

                _lastFetched = new FetchedAlbumData
                {
                    AlbumTitle = best.Title,
                    ArtistName = !string.IsNullOrWhiteSpace(best.ArtistCredit)
                    ? best.ArtistCredit
                    : ArtistBox.Text?.Trim(),

                    ReleaseMbid = best.Id,
                    Tracks = data.Tracks,
                    CoverBytes = data.CoverBytes,
                    CoverImage = data.CoverImage
                };

                StatusText.Text = $"{data.AlbumTitle} — {data.ArtistName}";
                CoverImage.Source = data.CoverImage;
                TracksList.ItemsSource = data.Tracks;
            }
            catch (OperationCanceledException)
            {
                StatusText.Text = "Canceled.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Failed: " + ex.Message;
            }
        }
        private async void AlternativesList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AlternativesList.SelectedItem is not AlternativeDisplay selected)
                return;

            if (string.IsNullOrWhiteSpace(selected.ReleaseId))
                return;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            StatusText.Text = "Fetching selected release...";
            CoverImage.Source = null;
            TracksList.ItemsSource = null;
            _lastFetched = null;

            try
            {
                var data = await _fetcher.FetchByReleaseIdAsync(selected.ReleaseId, _cts.Token);

                var candidate = _lastCandidates.FirstOrDefault(c => c.Id == selected.ReleaseId);

                var title = candidate != null && !string.IsNullOrWhiteSpace(candidate.Title)
                    ? candidate.Title
                    : AlbumBox.Text?.Trim();

                var artist = candidate != null && !string.IsNullOrWhiteSpace(candidate.ArtistCredit)
                    ? candidate.ArtistCredit
                    : ArtistBox.Text?.Trim();

                data = new FetchedAlbumData
                {
                    AlbumTitle = title,
                    ArtistName = artist,
                    ReleaseMbid = selected.ReleaseId,
                    Tracks = data.Tracks,
                    CoverBytes = data.CoverBytes,
                    CoverImage = data.CoverImage
                };

                _lastFetched = new FetchedAlbumData
                {
                    AlbumTitle = title ?? "",
                    ArtistName = artist ?? "",
                    ReleaseMbid = selected.ReleaseId,
                    Tracks = data.Tracks,
                    CoverBytes = data.CoverBytes,
                    CoverImage = data.CoverImage
                };

                StatusText.Text = $"{data.AlbumTitle} — {data.ArtistName}";
                CoverImage.Source = data.CoverImage;
                TracksList.ItemsSource = data.Tracks;
            }
            catch (OperationCanceledException)
            {
                StatusText.Text = "Canceled.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Failed: " + ex.Message;
            }
        }


        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_lastFetched == null)
            {
                StatusText.Text = "Nothing to save.";
                return;
            }

            if (string.IsNullOrWhiteSpace(_lastFetched.AlbumTitle) ||
                string.IsNullOrWhiteSpace(_lastFetched.ArtistName))
            {
                StatusText.Text = "Save failed: missing album title or artist.";
                return;
            }

            try
            {
                StatusText.Text = "Saving...";
                await _library.SaveFetchedAlbumAsync(_lastFetched);
                StatusText.Text = "Saved.";
            }
            catch (Exception ex)
            {
                var msg = ex.Message;

                if (ex.InnerException != null)
                    msg += " | Inner: " + ex.InnerException.Message;

                if (ex.InnerException?.InnerException != null)
                    msg += " | Inner2: " + ex.InnerException.InnerException.Message;

                StatusText.Text = "Save failed: " + msg;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ReviewsPage());
        }

        private async void ShowNext3_Click(object sender, RoutedEventArgs e)
        {
            if (_lastCandidates == null || _lastCandidates.Count == 0)
            {
                AltStatus.Text = "Fetch an album first.";
                return;
            }

            if (_altIndex >= _lastCandidates.Count)
            {
                AltStatus.Text = "No more results.";
                return;
            }

            var take = 3;
            var slice = _lastCandidates.Skip(_altIndex).Take(take).ToList();
            _altIndex += slice.Count;

            AltStatus.Text = "Loading track counts...";

            var list = new List<AlternativeDisplay>();

            foreach (var c in slice)
            {
                int trackCount = 0;

                try
                {
                    trackCount = await _fetcher.FetchTrackCountAsync(c.Id, _cts.Token);
                }
                catch
                {
                    trackCount = 0;
                }

                var line1 = c.Title;
                var line2 =
                    (string.IsNullOrWhiteSpace(c.ArtistCredit) ? "" : c.ArtistCredit) +
                    (string.IsNullOrWhiteSpace(c.Date) ? "" : " • " + c.Date) +
                    (trackCount > 0 ? " • " + trackCount.ToString() + " tracks" : "");

                list.Add(new AlternativeDisplay
                {
                    ReleaseId = c.Id,
                    Line1 = line1,
                    Line2 = line2
                });
            }

            AlternativesList.ItemsSource = list;
            AltStatus.Text = "Double-click a result to select it.";
        }

    }
}
