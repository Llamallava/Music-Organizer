using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Music_Organizer.Data;

namespace Music_Organizer
{
    /// <summary>
    /// Interaction logic for ReviewsPage.xaml
    /// </summary>
    public partial class ReviewsPage : Page
    {
        public ReviewsPage()
        {
            InitializeComponent();
            DataContext = new AlbumGridViewModel(OpenEditorForAlbum);
        }

        private void OpenEditorForAlbum(AlbumItem album)
        {
            if (album == null)
                return;

            NavigationService?.Navigate(new EditorPage(album.AlbumId));
        }

        private void AddAlbum_Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddAlbumPage());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MainMenuPage());
        }
    }
}
