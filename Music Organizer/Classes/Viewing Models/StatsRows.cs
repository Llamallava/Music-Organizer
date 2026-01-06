using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Music_Organizer.Classes.Viewing_Models
{
    public sealed class RankedAlbumRow
    {
        public int Rank { get; init; }
        public Guid AlbumId { get; init; }
        public string AlbumDisplay { get; init; }
        public double Score { get; init; }
        public string ScoreText { get; init; }
        public int TracksUsed { get; init; }
        public string TracksUsedText { get; init; }

        public ImageSource CoverImage { get; init; }
    }

    public sealed class RankedSongRow
    {
        public int Rank { get; init; }
        public Guid TrackId { get; init; }
        public Guid AlbumId { get; init; }
        public string SongTitle { get; init; }
        public string AlbumDisplay { get; init; }
        public double Score { get; init; }
        public string ScoreText { get; init; }
    }
}
