global using System;
global using System.IO;
global using System.Linq;
global using System.Windows;
global using static Utify.Extensions;

using YoutubeExplode;
using LibVLCSharp.Shared;
using NAudio.CoreAudioApi;
using Utify.Models.Objects;
using System.Windows.Input;
using YoutubeExplode.Common;
using System.Threading.Tasks;
using System.Windows.Threading;
using Utify.Models.Local.Clients;
using System.Windows.Media.Imaging;
using AudioClient = Utify.Models.Local.Clients.AudioClient;
using Utify.View.Windows;
using Utify.View.Usercontrols.GeneralComponent;
using System.Collections.Generic;

namespace Utify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variables

        // Static.
        public static MainWindow Instance { get; private set; }

        // Public.
        public LibVLC VLCLibrary { get; private set; }
        public AudioClient Audio { get; private set; }
        public PlaylistClient PlaylistClient { get; private set; }
        public SettingsClient SettingsClient { get; private set; }
        public new ContextMenuWindow ContextMenu { get; private set; }

        // Private.
        private MMDevice Device { get; set; }
        private MMDeviceEnumerator Enumerator { get; set; }

        #endregion

        #region OnLoaded

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void WindowInitialized(object sender, EventArgs e)
        {
            // Set the singleton.
            Instance ??= this;

            // Create the XML directory if it doesn't already exists.
            Directory.CreateDirectory(Paths.XML);

            // Initialize the variables and events.
            await InitializeVariables();
            await InitializeEvents();
            await PlaylistClient.InitializeAsync();
            await SettingsClient.InitializeAsync();

            // Manage the periodic actions.
            _ = new Action(VisualizePeakVolume).RunPeriodically(TimeSpan.FromMilliseconds(10));

            var url = "https://www.youtube.com/playlist?list=PL1Gv-OC8tneodd99wZvmJFm4qs6ABOHIQ";

            var client = new YoutubeClient();
            var videos = await client.Playlists.GetVideosAsync(url);

            await Audio.Enqueue(videos);
            await Audio.PlayAsync();
        }

        private Task InitializeEvents()
        {
            Audio.OnTimeChanged += AudioOnTimeChanged;
            Audio.OnSongChanged += AudioOnSongChanged;
            Audio.OnSongBuffered += AudioOnSongBuffered;
            Audio.OnSongPaused += AudioOnSongPaused;
            Audio.OnSongResumed += AudioOnSongResumed;

            SettingsClient.OnVolumeInitialize += SettingsOnVolumeInitialize;
            SettingsClient.OnAudioOnlyInitialize += SettingsOnAudioOnlyInitialize;

            return Task.CompletedTask;
        }

        private Task InitializeVariables()
        {
            #region VLC

            // Initialize the core.
            Core.Initialize();

            // Set the vlc related variables.
            VLCLibrary = new();
            Audio = new(VLCLibrary, VideoView);

            #endregion

            #region NAudio

            // Set the enumerator and device.
            Enumerator = new();
            Device = Enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

            #endregion

            ContextMenu = new();
            SettingsClient = new();
            PlaylistClient = new();

            return Task.CompletedTask;
        }

        #endregion

        #region Methods

        private void VisualizePeakVolume()
        {
            // Fetch the peak volume.
            double volume = Device.AudioMeterInformation.MasterPeakValue;

            // Map the volume to the bar, and lerp the value.
            double mapped = volume.Map(0, 1, 0, VolumebarBack.ActualWidth);
            float lerped = Lerp((float)VolumebarVisualize.ActualWidth, (float)mapped, 0.1f);

            // Set the actual width.
            VolumebarVisualize.Width = lerped;
        }

        private void ToggleAudioOnlyVisibility()
        {
            // Swap the thumbnail visibility.
            ImageThumbnail.Visibility = Audio.IsAudioOnly ?
                Visibility.Visible : Visibility.Hidden;

            // Swap the video visibility.
            VideoView.Visibility = Audio.IsAudioOnly ?
                Visibility.Hidden : Visibility.Visible;
        }

        #endregion

        #region Events

        #region Audio

        private void AudioOnSongChanged(object sender, EventArgs? e)
        {
            // Force this event to run on the UI thread.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Cast the sender to a song.
                if (sender is Song song)
                {
                    // Set the relevant information.
                    TimebarFront.Width = 0;
                    TimebarBuffering.Width = 0;
                    VideoInfo.Text = song.Title;
                    LabelTimeRight.Content = new TimeSpan(song.Duration).GetElapsedTime();

                    // Set & fetch the thumbnail.
                    BitmapImage image = new();
                    image.BeginInit();
                    image.UriSource = new Uri(song.Thumbnail.Url);
                    image.EndInit();

                    // Update the thumbnail.
                    ImageThumbnail.Source = image;

                    // If needed, swap queue'd visibility.
                    ToggleAudioOnlyVisibility();
                }
            }), DispatcherPriority.Normal);
        }

        private void AudioOnTimeChanged(object sender, EventArgs? e)
        {
            // Force this event to run on the UI thread.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (TimebarBack.IsMouseCaptured || sender == null)
                    return;

                // Cast the sender and eventargs.
                if (sender is Song song && e is MediaPlayerTimeChangedEventArgs args)
                {
                    // Fetch the time.
                    TimeSpan time = TimeSpan.FromMilliseconds(args.Time);

                    // Update the information.
                    LabelTimeLeft.Content = time.GetElapsedTime();
                    // Remap the current time from 0 to the max duration, to 0 to max width.
                    TimebarFront.Width = time.TotalSeconds.Map(0, new TimeSpan(song.Duration).TotalSeconds,
                                                               0, TimebarBack.ActualWidth);
                }
            }), DispatcherPriority.Normal);
        }

        private void AudioOnSongBuffered(object sender, EventArgs? e)
        {
            // Force this event to run on the UI thread.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!IsActive || TimebarBack.IsMouseCaptured || sender == null)
                    return;

                // Cast the sender and eventargs.
                if (sender is Song song && e is MediaPlayerBufferingEventArgs args)
                {
                    // Fetch the cache.
                    double cache = args.Cache;

                    // Remap the current time from 0 to the max duration, to 0 to max width.
                    TimebarBuffering.Width = cache.Map(0, 100, 0, TimebarBack.ActualWidth);
                }
            }), DispatcherPriority.Normal);
        }

        private void AudioOnSongResumed(object sender, EventArgs? e)
        {
            // Force this event to run on the UI thread.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ImagePlay.Source = ResourceImage("Pause");
            }), DispatcherPriority.Normal);
        }

        private void AudioOnSongPaused(object sender, EventArgs? e)
        {
            // Force this event to run on the UI thread.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ImagePlay.Source = ResourceImage("PlayRound");
            }), DispatcherPriority.Normal);
        }

        #endregion

        #region Settings

        private void SettingsOnVolumeInitialize(Settings settings)
        {
            // Set the audio volume & UI volume.
            Audio.Volume = settings.Volume;
            VolumebarFront.Width = Convert.ToDouble(settings.Volume).Map(0, 100, 0, VolumebarBack.ActualWidth);
        }

        private void SettingsOnAudioOnlyInitialize(Settings settings)
        {
            Audio.IsAudioOnly = settings.AudioOnly;

            ImageAudio.Source = Audio.IsAudioOnly ?
                ResourceImage("MusicGreen") :
                ResourceImage("Music");

            ToggleAudioOnlyVisibility();
        }

        #endregion

        #region Bar Controls

        // Time.
        private void TimerbarMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Capture the mouse.
            Mouse.Capture(TimebarBack);

            // Set the width.
            TimebarFront.Width = (int)Clamp(e.GetPosition(TimebarBack).X, 0, TimebarBack.ActualWidth);
        }

        private async void TimebarMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Return if the mouse isn't captured.
            if (!TimebarBack.IsMouseCaptured)
                return;

            // Release the capture.
            TimebarBack.ReleaseMouseCapture();

            // Return if the audio isn't playing.
            if (!Audio.IsPlaying)
                return;

            // Get the current time in seconds relative to the timebar front's width.
            double time = TimebarFront.ActualWidth.Map(0, TimebarBack.ActualWidth,
                                                       0, new TimeSpan(Audio.Current.Duration).TotalSeconds);

            // Seek to the found time.
            await Audio.SeekAsync(TimeSpan.FromSeconds(time));
        }

        private void TimerbarMouseMove(object sender, MouseEventArgs e)
        {
            // Return if the mouse isn't captured.
            if (!TimebarBack.IsMouseCaptured)
                return;

            // Set the width.
            TimebarFront.Width = (int)Clamp(e.GetPosition(TimebarBack).X, 0, TimebarBack.ActualWidth);
        }

        // Volume.
        private void VolumebarMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Capture the mouse.
            Mouse.Capture(VolumebarBack);

            // Set the width.
            VolumebarFront.Width = (int)Clamp(e.GetPosition(VolumebarBack).X, 0, VolumebarBack.ActualWidth);
        }

        private async void VolumebarMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Check if the mouse is captured.
            if (VolumebarBack.IsMouseCaptured)
            {
                // Release the capture.
                VolumebarBack.ReleaseMouseCapture();

                // Remap the value to the correct volume range.
                Audio.Volume = (int)VolumebarFront.ActualWidth.Map(0, VolumebarBack.ActualWidth, 0, 100);

                // Update the volume within the settings, and save.
                SettingsClient.Settings.Volume = Audio.Volume;
                await SettingsClient.SaveAsync();
            }
        }

        private void VolumebarMouseMove(object sender, MouseEventArgs e)
        {
            // Check if the mouse is captured.
            if (VolumebarBack.IsMouseCaptured)
            {
                // Set the width.
                VolumebarFront.Width = (int)Clamp(e.GetPosition(VolumebarBack).X, 0, VolumebarBack.ActualWidth);

                // Remap the value to the correct volume range.
                Audio.Volume = (int)VolumebarFront.ActualWidth.Map(0, VolumebarBack.ActualWidth, 0, 100);
            }
        }

        #endregion

        #region Video Controls

        private async void ImagePlayMouseDown(object sender, MouseButtonEventArgs e)
        {
            await Audio.PauseResumeAsync();
        }

        private async void ImageShuffleMouseDown(object sender, MouseButtonEventArgs e)
        {
            await Audio.ShuffleAsync(!Audio.IsShuffling);

            ImageShuffle.Source = Audio.IsShuffling ?
                ResourceImage("ShuffleGreen") :
                ResourceImage("Shuffle");
        }

        private async void ImageSkipLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            await Audio.SkipAsync(AudioClient.Skip.LEFT);
        }

        private async void ImageSkipRightMouseDown(object sender, MouseButtonEventArgs e)
        {
            await Audio.SkipAsync(AudioClient.Skip.RIGHT);
        }

        private void ImageRepeatMouseDown(object sender, MouseButtonEventArgs e)
        {
            Audio.IsRepeating = !Audio.IsRepeating;

            ImageRepeat.Source = Audio.IsRepeating ?
                ResourceImage("RepeatGreen") :
                ResourceImage("Repeat");
        }

        #endregion

        #region Volume Controls

        private async void ImageAudioMouseDown(object sender, MouseButtonEventArgs e)
        {
            Audio.IsAudioOnly = !Audio.IsAudioOnly;

            ImageAudio.Source = Audio.IsAudioOnly ?
                ResourceImage("MusicGreen") :
                ResourceImage("Music");

            // Update the audio within the settings, and save.
            SettingsClient.Settings.AudioOnly = Audio.IsAudioOnly;
            await SettingsClient.SaveAsync();
        }

        private void ImageVolumeMouseDown(object sender, MouseButtonEventArgs e)
        {
            Audio.IsMuted = !Audio.IsMuted;

            ImageVolume.Source = Audio.IsMuted ?
                ResourceImage("VolumeMuted") :
                ResourceImage("VolumeMiddle");
        }

        #endregion

        #endregion
    }
}
