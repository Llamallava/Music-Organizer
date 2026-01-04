using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Music_Organizer
{
    public static class AppPaths
    {
        public const string AppName = "Music Organizer";

        public static string AppRoot =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppName
            );

        public static string Covers =>
            Path.Combine(AppRoot, "Covers");

        public static string Database =>
            Path.Combine(AppRoot, "library.db");
    }
}
