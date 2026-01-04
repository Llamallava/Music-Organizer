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

namespace Music_Organizer
{
    /// <summary>
    /// Interaction logic for AddAlbumPage.xaml
    /// </summary>
    public partial class AddAlbumPage : Page
    {
        private readonly AlbumMetadataFetcher _fetcher;
        private CancellationTokenSource _cts;

        public AddAlbumPage()
        {
            InitializeComponent();
            _fetcher = new AlbumMetadataFetcher();
        }

        private async void Fetch_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            StatusText.Text = "Fetching...";
            CoverImage.Source = null;
            TracksList.ItemsSource = null;

            try
            {
                var artist = ArtistBox.Text?.Trim();
                var album = AlbumBox.Text?.Trim();

                var data = await _fetcher.FetchAsync(artist, album, _cts.Token);

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
    }
}
