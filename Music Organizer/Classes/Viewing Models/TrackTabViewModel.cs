using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Music_Organizer.Classes
{
    public sealed class TrackTabViewModel : INotifyPropertyChanged
    {
        private string _displayName;
        private string _notes;
        private string _lyrics;
        private string _scoreText;

        public TrackTabViewModel(
        System.Guid? trackId,
        bool isConclusion,
        string displayName,
        string trackTitle)
        {
            TrackId = trackId;
            IsConclusion = isConclusion;
            _displayName = displayName;
            TrackTitle = trackTitle;
            _notes = "";
            _lyrics = "";
            _scoreText = "";
        }

        public System.Guid? TrackId { get; }

        public bool IsConclusion { get; }

        public string TrackTitle { get; }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (value == _displayName)
                {
                    return;
                }

                _displayName = value;
                OnPropertyChanged();
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (value == _notes)
                {
                    return;
                }

                _notes = value;
                OnPropertyChanged();
            }
        }

        public string Lyrics
        {
            get => _lyrics;
            set
            {
                if (value == _lyrics)
                {
                    return;
                }

                _lyrics = value;
                OnPropertyChanged();
            }
        }

        public string ScoreText
        {
            get => _scoreText;
            set
            {
                if (value == _scoreText)
                {
                    return;
                }

                _scoreText = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
