using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Music_Organizer
{
    public class AlbumItem
    {
        public Guid AlbumId { get; set; }

        public ImageSource CoverImage {  get; set; }

        public string DisplayText { get; set; }
    }
}
