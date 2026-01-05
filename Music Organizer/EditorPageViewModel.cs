using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Music_Organizer.Classes;
using Music_Organizer.Data;

namespace Music_Organizer
{
    public sealed class EditorPageViewModel : INotifyPropertyChanged
    {
        private string _albumTitle;
        private string _artistName;
        private ImageSource _coverImage;
        private TrackTabViewModel _selectedTab;

        public EditorPageViewModel(Guid albumId)
        {
            Tabs = new ObservableCollection<TrackTabViewModel>();
            LoadAlbum(albumId);
        }

        public ObservableCollection<TrackTabViewModel> Tabs
        {
            get;
        }

        public TrackTabViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (ReferenceEquals(value, _selectedTab))
                    return;

                _selectedTab = value;
                OnPropertyChanged();
            }
        }

        public string AlbumTitle
        {
            get => _albumTitle;
            private set
            {
                if (value == _albumTitle)
                    return;

                _albumTitle = value;
                OnPropertyChanged();
            }
        }

        public string ArtistName
        {
            get => _artistName;
            private set
            {
                if (value == _artistName)
                    return;

                _artistName = value;
                OnPropertyChanged();
            }
        }

        public ImageSource CoverImage
        {
            get => _coverImage;
            private set
            {
                if (Equals(value, _coverImage))
                    return;

                _coverImage = value;
                OnPropertyChanged();
            }
        }

        private void LoadAlbum(Guid albumId)
        {
            using var db = new MusicOrganizerDbContext();

            var album = db.Albums.FirstOrDefault(a => a.AlbumId == albumId);
            if (album == null)
            {
                AlbumTitle = "Album not found";
                ArtistName = "";
                CoverImage = null;
                Tabs.Clear();
                SelectedTab = null;
                return;
            }

            AlbumTitle = album.AlbumTitle;
            ArtistName = album.ArtistName;

            var coverPath = Path.Combine(AppPaths.Covers, album.CoverFileName);
            CoverImage = File.Exists(coverPath) ? LoadImage(coverPath) : null;

            Tabs.Clear();

            var tracks = db.Tracks
                .Where(t => t.AlbumId == albumId)
                .OrderBy(t => t.TrackNumber)
                .Select(t => new
                {
                    t.TrackNumber,
                    t.Title
                })
                .ToList();

            foreach (var t in tracks)
            {
                var name = t.TrackNumber.ToString() + ". " + t.Title;
                Tabs.Add(new TrackTabViewModel(name));
            }

            Tabs.Add(new TrackTabViewModel("Conclusion"));

            if (Tabs.Count > 0)
                SelectedTab = Tabs[0];
        }

        private static BitmapImage LoadImage(string path)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = 400;
            image.EndInit();
            image.Freeze();
            return image;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
