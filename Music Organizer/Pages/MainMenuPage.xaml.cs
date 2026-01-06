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
using Music_Organizer.Pages;

namespace Music_Organizer
{
    /// <summary>
    /// Interaction logic for MainMenuPage.xaml
    /// </summary>
    public partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private void Reviews_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ReviewsPage());
        }

        private void Stats_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new StatsPage());
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
