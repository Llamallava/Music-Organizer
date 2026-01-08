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
    /// Interaction logic for StatsPage.xaml
    /// </summary>
    public partial class StatsPage : Page
    {
        public StatsPage()
        {
            InitializeComponent();
            DataContext = new StatsPageViewModel();

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MainMenuPage());
        }

        private void BubbleMouseWheelToOuterScroll(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled)
                return;

            var scroller = FindName("OuterScroll") as ScrollViewer;
            if (scroller == null)
                return;

            e.Handled = true;

            // Negative delta means scroll down
            scroller.ScrollToVerticalOffset(scroller.VerticalOffset - e.Delta);
        }
    }
}
