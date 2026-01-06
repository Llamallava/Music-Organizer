using System;
using System.Collections.Generic;
using System.Text;

namespace Music_Organizer.Lyrics
{
    public sealed class DefaultLyricsProvider : ILyricsProvider
    {
        public async Task<string> GetLyricsAsync(string title, string artist, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = title + " - " + artist;

            var lyrics = await LyricsFetcher.GetLyricsAsync(query);

            cancellationToken.ThrowIfCancellationRequested();

            return lyrics ?? "";
        }
    }
}
