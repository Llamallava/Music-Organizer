using System;
using System.Collections.Generic;
using System.Text;

namespace Music_Organizer.Data
{
    public sealed class AlbumConclusionEntity
    {
        public Guid AlbumId { get; set; }

        public string Notes { get; set; }

        public double? Score { get; set; }

        public AlbumEntity Album { get; set; }
    }
}
