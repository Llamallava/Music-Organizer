using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Music_Organizer.Lyrics
{
    public static class LyricsFetcher
    {
        public static async Task<string> GetLyricsAsync(string songName)
        {
            var lyricsDirectory = AppPaths.Lyrics;
            var lyricsFilePath =
                Path.Combine(lyricsDirectory, songName + ".txt");

            // 1) Read from disk if already cached (even if it's empty)
            if (File.Exists(lyricsFilePath))
            {
                return await File.ReadAllTextAsync(lyricsFilePath, Encoding.UTF8);
            }

            var lyrics = "";

            using var http = new HttpClient();

            // 2) Genius API search
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    "ixUDDLdcGGPXeonLtkYOG2dITacY588nfgTHClbR_DzqIQwtcaRllV1-X-3Kk_9A"
                );

            var query = Uri.EscapeDataString(songName);
            var searchResponse =
                await http.GetAsync($"https://api.genius.com/search?q={query}");

            if (!searchResponse.IsSuccessStatusCode)
            {
                await SaveLyricsFileAsync(lyricsDirectory, lyricsFilePath, lyrics);
                return lyrics;
            }

            var searchJson =
                await searchResponse.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(searchJson);

            var hits =
                doc.RootElement
                    .GetProperty("response")
                    .GetProperty("hits");

            if (hits.GetArrayLength() == 0)
            {
                await SaveLyricsFileAsync(lyricsDirectory, lyricsFilePath, lyrics);
                return lyrics;
            }

            var url =
                hits[0]
                    .GetProperty("result")
                    .GetProperty("url")
                    .GetString();

            if (string.IsNullOrWhiteSpace(url))
            {
                await SaveLyricsFileAsync(lyricsDirectory, lyricsFilePath, lyrics);
                return lyrics;
            }

            // 3) Fetch lyrics page HTML
            http.DefaultRequestHeaders.UserAgent.Clear();
            http.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/115.0.0.0 Safari/537.36"
            );

            var pageResponse = await http.GetAsync(url);

            if (!pageResponse.IsSuccessStatusCode)
            {
                await SaveLyricsFileAsync(lyricsDirectory, lyricsFilePath, lyrics);
                return lyrics;
            }

            var html =
                await pageResponse.Content.ReadAsStringAsync();

            // 4) Parse HTML
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(html);

            var nodes =
                htmlDoc.DocumentNode
                    .SelectNodes("//div[@data-lyrics-container='true']");

            if (nodes is not null && nodes.Count > 0)
            {
                lyrics =
                    string.Join(
                        "\n",
                        nodes
                            .Select(n =>
                                HtmlAgilityPack.HtmlEntity
                                    .DeEntitize(n.InnerText)
                                    .Trim()
                            )
                            .Where(t => !string.IsNullOrWhiteSpace(t))
                    );
            }

            string cleanLyrics = LyricsCleaner.CleanGeniusLyrics(lyrics);
            // 5) Persist newly fetched lyrics, even if empty
            await SaveLyricsFileAsync(lyricsDirectory, lyricsFilePath, cleanLyrics);

            return cleanLyrics;
        }

        private static async Task SaveLyricsFileAsync(
            string lyricsDirectory,
            string lyricsFilePath,
            string lyrics)
        {
            Directory.CreateDirectory(lyricsDirectory);

            await File.WriteAllTextAsync(
                lyricsFilePath,
                lyrics ?? "",
                Encoding.UTF8);
        }
    }
}
