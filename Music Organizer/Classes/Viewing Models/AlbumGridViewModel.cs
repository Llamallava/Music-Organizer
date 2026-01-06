using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Music_Organizer;
using Music_Organizer.Classes;
using Music_Organizer.Data;

public sealed class AlbumGridViewModel
{
    private readonly Action<AlbumItem> _openEditor;

    public ObservableCollection<AlbumItem> AlbumItems { get; }
    public RelayCommand<AlbumItem> OpenAlbumCommand { get; }

    public AlbumGridViewModel(Action<AlbumItem> openEditor)
    {
        _openEditor = openEditor;

        AlbumItems = new ObservableCollection<AlbumItem>();

        OpenAlbumCommand = new RelayCommand<AlbumItem>(
            album =>
            {
                if (album == null)
                    return;

                _openEditor?.Invoke(album);
            });

        LoadFromDatabase();
    }

    private void LoadFromDatabase()
    {
        using var db = new MusicOrganizerDbContext();

        var albums = db.Albums
            .OrderBy(a => a.ArtistName)
            .ThenBy(a => a.AlbumTitle)
            .ToList();

        AlbumItems.Clear();

        foreach (var a in albums)
        {
            var coverPath = Path.Combine(AppPaths.Covers, a.CoverFileName);

            AlbumItems.Add(new AlbumItem
            {
                AlbumId = a.AlbumId,
                DisplayText = $"{a.AlbumTitle} - {a.ArtistName}",
                CoverImage = File.Exists(coverPath) ? LoadImage(coverPath) : null
            });
        }
    }

    private BitmapImage LoadImage(string path)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.UriSource = new Uri(path);
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.DecodePixelWidth = 300;
        image.EndInit();
        image.Freeze();
        return image;
    }
}

public sealed class AlternativeDisplay
{
    public string ReleaseId { get; init; }
    public string Line1 { get; init; }
    public string Line2 { get; init; }
}
