using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Music_Organizer;
using System.Windows.Input;

public class AlbumGridViewModel
{
    private readonly Action<AlbumItem> _navigateToAlbum;

    public ObservableCollection<AlbumItem> AlbumItems { get; }
    public ICommand OpenAlbumCommand { get; }

    public AlbumGridViewModel(Action<AlbumItem> navigateToAlbum)
    {
        _navigateToAlbum = navigateToAlbum;

        AlbumItems = new ObservableCollection<AlbumItem>();

        OpenAlbumCommand = new RelayCommand<AlbumItem>(
            album =>
            {
                if (album == null)
                    return;

                _navigateToAlbum?.Invoke(album);
            });

        LoadAlbumsFromCoversFolder();
    }

    private void LoadAlbumsFromCoversFolder()
    {
        var appRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Music Organizer"
        );

        var coversPath = Path.Combine(appRoot, "Covers");

        if (!Directory.Exists(AppPaths.Covers))
            return;

        var imageFiles = Directory
            .EnumerateFiles(AppPaths.Covers)
            .Where(f =>
                f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f);

        foreach (var file in imageFiles)
        {
            AlbumItems.Add(new AlbumItem
            {
                AlbumId = Guid.NewGuid(),
                CoverImage = LoadImage(file),
                DisplayText = Path.GetFileNameWithoutExtension(file)
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
