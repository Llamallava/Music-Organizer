using System;
using System.Collections.Generic;
using System.Text;

namespace Music_Organizer.Data
{
    public sealed class TrackReviewEntity
    {
        public Guid TrackReviewId { get; set; }

        public Guid AlbumId { get; set; }

        public Guid TrackId { get; set; }

        public string Notes { get; set; }

        public double? Score { get; set; }

        public AlbumEntity Album { get; set; }

        public TrackEntity Track { get; set; }
    }
}
