using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Music_Organizer.Classes
{
    public interface IReviewRepository
    {
        Task<List<AlbumReview>> GetAllAlbumReviewsAsync();
    }

    public sealed class AlbumReview
    {
        public string Title { get; init; } = "";
        public string Artist { get; init; } = "";
        public double? ConclusionScore { get; init; }
        public List<SongReview> Tracks { get; init; } = new List<SongReview>();
    }

    public sealed class SongReview
    {
        public string Title { get; init; } = "";
        public double? Score { get; init; }
    }
}
