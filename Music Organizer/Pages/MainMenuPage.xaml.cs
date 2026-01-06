using Music_Organizer.Pages;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    public sealed class ClampConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 4)
                return 0.0;

            double actual = ToDouble(values[0]);
            double min = ToDouble(values[1]);
            double max = ToDouble(values[2]);
            double divisor = ToDouble(values[3]);

            if (divisor <= 0.0001)
                divisor = 1.0;

            double scaled = actual / divisor;

            if (scaled < min) return min;
            if (scaled > max) return max;
            return scaled;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static double ToDouble(object v)
        {
            if (v == null) return 0.0;
            if (v is double d) return d;
            if (v is float f) return f;
            if (v is int i) return i;
            if (double.TryParse(v.ToString(), out double parsed)) return parsed;
            return 0.0;
        }
    }
}
