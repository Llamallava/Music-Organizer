using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Music_Organizer;
using Music_Organizer.Classes;

public sealed class AlbumLibraryService
{
    public async Task<Guid> SaveFetchedAlbumAsync(FetchedAlbumData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var albumId = Guid.NewGuid();

        var coverFileName = $"{albumId}.jpg";
        var coverPath = Path.Combine(AppPaths.Covers, coverFileName);

        if (data.CoverBytes != null && data.CoverBytes.Length > 0)
            await File.WriteAllBytesAsync(coverPath, data.CoverBytes);

        using var db = new MusicOrganizerDbContext();

        var album = new AlbumEntity
        {
            AlbumId = albumId,
            AlbumTitle = data.AlbumTitle,
            ArtistName = data.ArtistName,
            CoverFileName = coverFileName,
            Tracks = data.Tracks
                .Select((t, i) =>
                    new TrackEntity
                    {
                        TrackId = Guid.NewGuid(),
                        AlbumId = albumId,
                        TrackNumber = i + 1,
                        Title = t
                    })
                .ToList()
        };

        db.Albums.Add(album);
        await db.SaveChangesAsync();

        return albumId;
    }
}
