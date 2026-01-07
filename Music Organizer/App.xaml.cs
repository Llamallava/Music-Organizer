using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Music_Organizer.Classes;
using Music_Organizer.Data;

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
            Directory.CreateDirectory(AppPaths.Lyrics);

            using var db = new MusicOrganizerDbContext();
            db.Database.Migrate();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            try
            {
                if (Directory.Exists(AppPaths.TempMusic))
                {
                    foreach (var file in Directory.GetFiles(AppPaths.TempMusic))
                    {
                        try { File.Delete(file); } catch { }
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        
    }
}
