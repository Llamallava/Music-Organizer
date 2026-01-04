using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Music_Organizer;

public sealed class AlbumMetadataFetcher : IDisposable
{
    private readonly HttpClient _http;

    public AlbumMetadataFetcher()
    {
        _http = new HttpClient();

        // MusicBrainz requires a meaningful User-Agent. :contentReference[oaicite:2]{index=2}
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "MusicOrganizer/0.1 (local personal app; contact: none)"
        );
    }

    public void Dispose()
    {
        _http.Dispose();
    }

    public async Task<FetchedAlbumData> FetchAsync(
        string artistName,
        string albumTitle,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(artistName))
            throw new ArgumentException("Artist name is required.", nameof(artistName));

        if (string.IsNullOrWhiteSpace(albumTitle))
            throw new ArgumentException("Album title is required.", nameof(albumTitle));

        var release = await SearchBestReleaseAsync(artistName, albumTitle, cancellationToken);

        // MusicBrainz rate limiting guidance: keep requests gentle. :contentReference[oaicite:3]{index=3}
        await Task.Delay(1100, cancellationToken);

        var tracks = await GetTrackListAsync(release.Id, cancellationToken);

        await Task.Delay(1100, cancellationToken);

        var cover = await TryGetFrontCoverAsync(release.Id, cancellationToken);

        return new FetchedAlbumData
        {
            AlbumTitle = release.Title,
            ArtistName = release.ArtistCreditName ?? artistName,
            ReleaseMbid = release.Id,
            Tracks = tracks,
            CoverImage = cover
        };
    }

    private async Task<ReleaseSearchHit> SearchBestReleaseAsync(
        string artistName,
        string albumTitle,
        CancellationToken cancellationToken
    )
    {
        // Search endpoint docs: WS/2 Search supports Lucene query. :contentReference[oaicite:4]{index=4}
        var luceneQuery =
            $"release:\"{albumTitle}\" AND artist:\"{artistName}\"";

        var url =
            "https://musicbrainz.org/ws/2/release/?" +
            "query=" + UrlEncoder.Default.Encode(luceneQuery) +
            "&fmt=json" +
            "&limit=5";

        using var response = await _http.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var payload = await JsonSerializer.DeserializeAsync<ReleaseSearchResponse>(
            stream,
            JsonOptions,
            cancellationToken
        );

        if (payload == null || payload.Releases == null || payload.Releases.Count == 0)
            throw new InvalidOperationException("No matching releases found on MusicBrainz.");

        // Simple first-pass choice: take the top result.
        // Later you can show a “choose release” dialog (deluxe/remaster/etc).
        return payload.Releases[0];
    }

    private async Task<IReadOnlyList<string>> GetTrackListAsync(
        string releaseMbid,
        CancellationToken cancellationToken
    )
    {
        // Track list: lookup release with inc=recordings. :contentReference[oaicite:5]{index=5}
        var url =
            $"https://musicbrainz.org/ws/2/release/{releaseMbid}?inc=recordings&fmt=json";

        using var response = await _http.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var payload = await JsonSerializer.DeserializeAsync<ReleaseLookupResponse>(
            stream,
            JsonOptions,
            cancellationToken
        );

        if (payload == null || payload.Media == null)
            return Array.Empty<string>();

        var tracks = payload.Media
            .Where(m => m.Tracks != null)
            .SelectMany(m => m.Tracks)
            .Where(t => !string.IsNullOrWhiteSpace(t.Title))
            .Select(t => t.Title.Trim())
            .ToList();

        return tracks;
    }

    private async Task<BitmapImage> TryGetFrontCoverAsync(
        string releaseMbid,
        CancellationToken cancellationToken
    )
    {
        // Cover Art Archive: front cover for a release.
        // The service commonly redirects to an Internet Archive URL. :contentReference[oaicite:6]{index=6}
        var url = $"https://coverartarchive.org/release/{releaseMbid}/front";

        using var response = await _http.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        return BytesToBitmapImage(bytes);
    }

    private static BitmapImage BytesToBitmapImage(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return null;

        using var ms = new MemoryStream(bytes);

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = ms;
        image.EndInit();
        image.Freeze();
        return image;
    }

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class ReleaseSearchResponse
    {
        public List<ReleaseSearchHit> Releases { get; set; }
    }

    private sealed class ReleaseSearchHit
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public List<ArtistCredit> ArtistCredit { get; set; }

        public string ArtistCreditName =>
            ArtistCredit != null && ArtistCredit.Count > 0
                ? string.Join(", ", ArtistCredit
                    .Where(a => a?.Artist != null && !string.IsNullOrWhiteSpace(a.Artist.Name))
                    .Select(a => a.Artist.Name))
                : null;
    }

    private sealed class ArtistCredit
    {
        public Artist Artist { get; set; }
    }

    private sealed class Artist
    {
        public string Name { get; set; }
    }

    private sealed class ReleaseLookupResponse
    {
        public List<Media> Media { get; set; }
    }

    private sealed class Media
    {
        public List<Track> Tracks { get; set; }
    }

    private sealed class Track
    {
        public string Title { get; set; }
    }
}
