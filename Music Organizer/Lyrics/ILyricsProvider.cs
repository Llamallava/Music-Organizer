using System;
using System.Collections.Generic;
using System.Text;

namespace Music_Organizer.Lyrics
{
    public interface ILyricsProvider
    {
        Task<string> GetLyricsAsync(string title, string artist, CancellationToken cancellationToken);
    }
}
