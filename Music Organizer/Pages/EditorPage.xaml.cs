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
using Music_Organizer.Classes;

namespace Music_Organizer
{
    /// <summary>
    /// Interaction logic for EditorPage.xaml
    /// </summary>
    public partial class EditorPage : Page
    {
        public EditorPage(Guid albumId)
        {
            InitializeComponent();
            DataContext = new EditorPageViewModel(albumId);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }
        private void SaveExit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!TryValidateScore(ScoreBox.Text, out var songScore))
            {
                MessageBox.Show(
                    "Score must be a number between 0 and 10.",
                    "Invalid Score",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            if (DataContext is Music_Organizer.Classes.EditorPageViewModel vm)
            {
                vm.SaveCommand.Execute(null);
            }

            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
        }
        private bool TryValidateScore(string? input, out double score)
        {
            score = 0.0;

            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            if (!double.TryParse(input, out score))
            {
                return false;
            }

            if (score < 0.0 || score > 10.0)
            {
                return false;
            }

            return true;
        }

    }
}
