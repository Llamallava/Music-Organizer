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

            StatusText.Text = "Fetching...";
            CoverImage.Source = null;
            TracksList.ItemsSource = null;
            _lastFetched = null;

            try
            {
                var artist = ArtistBox.Text?.Trim();
                var album = AlbumBox.Text?.Trim();

                var data = await _fetcher.FetchAsync(artist, album, _cts.Token);

                _lastFetched = data;

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
                StatusText.Text = "Nothing to save. Fetch an album first.";
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
                StatusText.Text = "Save failed: " + ex.Message;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ReviewsPage());
        }
    }
}
