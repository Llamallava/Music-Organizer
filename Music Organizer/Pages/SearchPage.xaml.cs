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
using Music_Organizer.Classes.Viewing_Models;
namespace Music_Organizer.Pages
{
    /// <summary>
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        public SearchPage()
        {
            InitializeComponent();
            DataContext = new SearchPageViewModel();
        }
        private void SongResult_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not SearchPageViewModel vm)
                return;

            var row = vm.SelectedSongResult;
            if (row == null)
                return;

            NavigationService?.Navigate(new EditorPage(row.AlbumId, row.TrackId));
        }

        private void AlbumResult_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not SearchPageViewModel vm)
                return;

            var row = vm.SelectedAlbumResult;
            if (row == null)
                return;

            NavigationService?.Navigate(new EditorPage(row.AlbumId));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MainMenuPage());
        }
    }
}
