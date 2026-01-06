using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Music_Organizer;
using Music_Organizer.Classes;
using Music_Organizer.Data;

public sealed class AlbumGridViewModel : INotifyPropertyChanged
{
    private readonly Action<AlbumItem> _openEditor;
    private readonly List<AlbumItem> _allAlbumItems;

    public ObservableCollection<AlbumItem> AlbumItems { get; }
    public RelayCommand<AlbumItem> OpenAlbumCommand { get; }

    private string _searchText;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (string.Equals(_searchText, value, StringComparison.Ordinal))
                return;

            _searchText = value;
            OnPropertyChanged(nameof(SearchText));

            ApplySearchFilter();
        }
    }

    public AlbumGridViewModel(Action<AlbumItem> openEditor)
    {
        _openEditor = openEditor;

        _allAlbumItems = new List<AlbumItem>();
        AlbumItems = new ObservableCollection<AlbumItem>();

        OpenAlbumCommand = new RelayCommand<AlbumItem>(
            album =>
            {
                if (album == null)
                    return;

                _openEditor?.Invoke(album);
            });

        LoadFromDatabase();
        ApplySearchFilter();
    }

    private void LoadFromDatabase()
    {
        using var db = new MusicOrganizerDbContext();

        var albums = db.Albums
            .OrderBy(a => a.ArtistName)
            .ThenBy(a => a.AlbumTitle)
            .ToList();

        _allAlbumItems.Clear();

        foreach (var a in albums)
        {
            var coverPath = Path.Combine(AppPaths.Covers, a.CoverFileName);

            _allAlbumItems.Add(new AlbumItem
            {
                AlbumId = a.AlbumId,
                AlbumTitle = a.AlbumTitle,
                ArtistName = a.ArtistName,
                DisplayText = $"{a.AlbumTitle} - {a.ArtistName}",
                CoverImage = File.Exists(coverPath) ? LoadImage(coverPath) : null
            });
        }
    }

    private void ApplySearchFilter()
    {
        var query = (_searchText ?? string.Empty).Trim();

        IEnumerable<AlbumItem> filtered = _allAlbumItems;

        if (query.Length > 0)
        {
            var terms = query
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            filtered = filtered.Where(a => MatchesAllTerms(a, terms));
        }

        AlbumItems.Clear();

        foreach (var item in filtered)
            AlbumItems.Add(item);
    }
    private static bool MatchesAllTerms(AlbumItem item, string[] terms)
    {
        var album = item.AlbumTitle ?? string.Empty;
        var artist = item.ArtistName ?? string.Empty;
        var display = item.DisplayText ?? string.Empty;

        foreach (var term in terms)
        {
            var found =
                album.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                artist.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                display.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

            if (!found)
                return false;
        }

        return true;
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

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


public sealed class AlternativeDisplay
{
    public string ReleaseId { get; init; }
    public string Line1 { get; init; }
    public string Line2 { get; init; }
}
