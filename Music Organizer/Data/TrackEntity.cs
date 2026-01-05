using System;
using System.Collections.Generic;
using System.Text;

namespace Music_Organizer.Data
{
    public sealed class TrackEntity
    {
        public Guid TrackId { get; set; }

        public Guid AlbumId { get; set; }

        public int TrackNumber { get; set; }

        public string Title { get; set; }

        public AlbumEntity Album { get; set; }
    }
}
