using System;
using System.Collections.Generic;
using System.Text;
using Music_Organizer.Classes;

namespace Music_Organizer.Data
{
    public sealed class AlbumEntity
    {
        public Guid AlbumId { get; set; }

        public string AlbumTitle { get; set; }

        public string ArtistName { get; set; }

        public string CoverFileName { get; set; }

        public List<TrackEntity> Tracks { get; set; } = new List<TrackEntity>();

        public List<TrackReviewEntity> TrackReviews { get; set; } = new List<TrackReviewEntity>();

        public AlbumConclusionEntity Conclusion { get; set; }

    }
}
