using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Music_Organizer.Classes;

namespace Music_Organizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Directory.CreateDirectory(AppPaths.AppRoot);
            Directory.CreateDirectory(AppPaths.Covers);

            using var db = new MusicOrganizerDbContext();
            db.Database.EnsureCreated();
        }
    }
}
