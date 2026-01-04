using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Music_Organizer
{
    public sealed class FetchedAlbumData
    {
        public string AlbumTitle { get; init; }
        public string ArtistName { get; init; }
        public string ReleaseMbid { get; init; }
        public IReadOnlyList<string> Tracks { get; init; }
        public ImageSource CoverImage { get; init; }
    }
}
