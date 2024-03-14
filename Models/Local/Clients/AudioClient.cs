using YoutubeExplode;
using LibVLCSharp.WPF;
using System.Threading;
using LibVLCSharp.Shared;
using Utify.Models.Objects;
using YoutubeExplode.Videos;
using System.Threading.Tasks;
using System.Collections.Generic;
using YoutubeExplode.Videos.Streams;

namespace Utify.Models.Local.Clients
{
    public class AudioClient
    {
        #region Variables

        // Static.
        public enum Skip { LEFT = -1, RIGHT = 1 }
        public delegate void AudioClientEventHandler(object sender, EventArgs? e);
        public event AudioClientEventHandler OnSongPaused;
        public event AudioClientEventHandler OnSongResumed;
        public event AudioClientEventHandler OnSongChanged;
        public event AudioClientEventHandler OnSongBuffered;
        public event AudioClientEventHandler OnTimeChanged;
        public event AudioClientEventHandler OnQueueAdded;
        public event AudioClientEventHandler OnQueueRemoved;

        // Public.
        public bool IsRepeating { get; set; }
        public bool IsAudioOnly { get; set; }
        public bool IsMuted { get => Player.Mute; set => Player.Mute = value; }
        public int Volume { get => Player.Volume; set => Player.Volume = value; }

        // Public (Readonly).
        public Song Current { get; private set; }
        public List<Song> Queue { get; private set; }
        public List<int> QueueShuffled { get; private set; }
        public bool IsPlaying => Player.IsPlaying;
        public bool IsShuffling { get; private set; }

        // Private.
        private VideoView View { get; set; }
        private LibVLC Library { get; set; }
        private MediaPlayer Player { get; set; }
        private YoutubeClient Youtube { get; set; }

        #endregion

        #region OnLoaded

        public AudioClient(LibVLC library, VideoView view)
        {
            // Set inherit variables.
            View = view;
            Library = library;

            // Initialize new variables.
            Youtube = new();
            Queue = new();
            Player = new(Library);

            // Handle events.
            View.Loaded += (s, e) => View.MediaPlayer = Player;
            Player.EndReached += PlayerEndReached;
            Player.TimeChanged += PlayerTimeChanged;
            Player.Buffering += PlayerBuffering;
            Player.Paused += PlayerPaused;
            Player.Playing += PlayerResumed;
        }

        #endregion

        #region Helper Methods

        // Private.

        private int GetShuffledIndex(Song song)
        {
            int actual = Queue.IndexOf(song);
            int index = QueueShuffled.IndexOf(actual);
            return index;
        }

        #endregion

        #region External Methods

        // Play & Enqueue

        /// <summary>
        /// Enqueue's a list of songs without force playing it.
        /// </summary>
        /// <param name="videos">The list of IVideo's in question.</param>
        /// <returns></returns>
        public Task Enqueue(IEnumerable<IVideo> videos)
        {
            Enqueue(videos.Select(x => new Song(x)));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Enqueue's a list of songs without force playing it.
        /// </summary>
        /// <param name="songs">The list of songs in question.</param>
        /// <returns></returns>
        public Task Enqueue(IEnumerable<Song> songs)
        {
            Queue.AddRange(songs);
            OnQueueAdded?.Invoke(songs, new());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Enqueue's a song without force playing it.
        /// </summary>
        /// <param name="video">The video in question.</param>
        /// <returns></returns>
        public Task Enqueue(IVideo video)
        {
            Enqueue(new Song(video));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Enqueue's a song without force playing it.
        /// </summary>
        /// <param name="song">The song in question.</param>
        /// <returns></returns>
        public Task Enqueue(Song song)
        {
            Queue.Add(song);
            OnQueueAdded?.Invoke(song, new());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Immediately enqueue's a list of songs, and plays the first one, regardless if the player is already playing something.
        /// </summary>
        /// <param name="videos">The videos in question.</param>
        /// <returns></returns>
        public async Task PlayAsync(IEnumerable<IVideo> videos)
        {
            await PlayAsync(videos.Select(x => new Song(x)));
        }

        /// <summary>
        /// Immediately enqueue's a list of songs, and plays the first one, regardless if the player is already playing something.
        /// </summary>
        /// <param name="songs">The songs in question.</param>
        /// <returns></returns>
        public async Task PlayAsync(IEnumerable<Song> songs)
        {
            Queue.AddRange(songs);
            await PlayInternalAsync(Queue.First());
        }

        /// <summary>
        /// Immediately plays a song, regardless if the player is already playing something.
        /// </summary>
        /// <param name="id">The song id in question.</param>
        /// <returns></returns>
        public async Task PlayAsync(IVideo video)
        {
            await PlayAsync(new Song(video));
        }

        /// <summary>
        /// Immediately plays a song, regardless if the player is already playing something.
        /// </summary>
        /// <param name="song">The song in question.</param>
        /// <returns></returns>
        public async Task PlayAsync(Song song)
        {
            await PlayInternalAsync(song);
        }

        /// <summary>
        /// Plays the first song of the enqueue'd songs.
        /// </summary>
        /// <param name="forced">Disregards <see cref="IsPlaying"/> if true.</param>
        /// <returns></returns>
        public async Task PlayAsync(bool forced = false)
        {
            if (IsPlaying && !forced)
                return;

            Song song = !IsShuffling ?
                Queue.First() :
                Queue[QueueShuffled.First()];

            await PlayInternalAsync(song);
        }

        // General controls

        public async Task PauseResumeAsync()
        {
            if (IsPlaying)
            {
                await PauseInternalAsync();
                return;
            }

            if (!IsPlaying)
            {
                await ResumeInternalAsync();
                return;
            }
        }

        public async Task SkipAsync(Skip direction)
        {
            await SkipInternalAsync(direction);
        }

        public async Task ShuffleAsync(bool active)
        {
            await ShuffleInternalAsync(active);
        }

        public async Task SeekAsync(TimeSpan time)
        {
            await SeekInternalAsync(time);
        }

        #endregion

        #region Internal Methods

        private async Task PlayInternalAsync(Song song)
        {
            // Update the current song.
            Current = song;

            OnSongChanged?.Invoke(song, null);

            // Grab the manifest.
            var manifest = await Youtube.Videos.Streams.GetManifestAsync(song.Id);

            // Extract the separate channels.
            var audio = manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var video = manifest.GetVideoOnlyStreams().GetWithHighestVideoQuality();

            // Play the media on a different thread, so it can be chained from a VLC thread.
            ThreadPool.QueueUserWorkItem(_ =>
            {
                // Set the url and options depending if it's audio only.
                var url = new Uri(IsAudioOnly ? audio.Url : video.Url);
                var options = IsAudioOnly ? Array.Empty<string>() : new string[]
                {
                    $":input-slave={audio.Url}"
                };

                // Create a new media stream, muxxing the audio and video channels together.
                using Media media = new(Library, url, options);
                Player.Play(media);
            });
        }

        private async Task PauseInternalAsync()
        {
            OnSongPaused?.Invoke(this, null);
            Player.Pause();
        }

        private async Task ResumeInternalAsync()
        {
            OnSongResumed?.Invoke(this, null);
            Player.Pause();
        }

        private async Task SkipInternalAsync(Skip direction)
        {
            if (Queue.Count <= 1)
                return;

            // Grab the index based on IsShuffling.
            int index = !IsShuffling ?
                // Grab the normal index, and either one to the left or one to the right.
                Queue.IndexOf(Current) + (int)direction :
                // Grab the shuffled index, and either one to the left, or one to the right.
                GetShuffledIndex(Current) + (int)direction;

            // Return on index out of bounds.
            if (index < 0 || index >= Queue.Count)
                return;

            // Grab the song based on the correct shuffle index.
            Song song = !IsShuffling ? Queue[index] : Queue[QueueShuffled[index]];

            await PlayInternalAsync(song);
        }

        private async Task ShuffleInternalAsync(bool active)
        {
            if (Queue.Count <= 1)
                return;

            IsShuffling = active;

            if (!IsShuffling)
                return;

            // Shuffle the queue and create a list of indexes.
            QueueShuffled = Queue.OrderBy(x => Guid.NewGuid())
                                 .Select(x => Queue.IndexOf(x))
                                 .ToList();

            // Return on empty current.
            if (!Queue.Contains(Current))
                return;

            // Find, remove, insert the current to the first index.
            int index = GetShuffledIndex(Current);
            int item = QueueShuffled[index];
            QueueShuffled.RemoveAt(index);
            QueueShuffled.Insert(0, item);
        }

        private async Task SeekInternalAsync(TimeSpan time)
        {
            if (!IsPlaying)
                return;

            Player.SeekTo(time);
        }

        #endregion

        #region Events

        private void PlayerTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
        {
            OnTimeChanged?.Invoke(Current, e);
        }

        private async void PlayerEndReached(object? sender, EventArgs e)
        {
            try
            {
                // Check if the player is looping.
                if (IsRepeating)
                {
                    await PlayInternalAsync(Current);
                    return;
                }

                // Check if the queue reached its end.
                if (Queue.IndexOf(Current) >= Queue.Count)
                {
                    // Clear the queue.
                    Queue.Clear();
                    QueueShuffled.Clear();
                    return;
                }

                // Check if the audio player is shuffeling.
                if (IsShuffling)
                {
                    await SkipInternalAsync(Skip.RIGHT);
                    return;
                }
            }
            catch
            {
                // TODO : Handle exception.
            }

            await SkipInternalAsync(Skip.RIGHT);
        }

        private void PlayerBuffering(object? sender, MediaPlayerBufferingEventArgs e)
        {
            OnSongBuffered?.Invoke(Current, e);
        }

        private void PlayerResumed(object? sender, EventArgs e)
        {
            OnSongResumed?.Invoke(Current, e);
        }

        private void PlayerPaused(object? sender, EventArgs e)
        {
            OnSongPaused?.Invoke(Current, e);
        }

        #endregion
    }
}
