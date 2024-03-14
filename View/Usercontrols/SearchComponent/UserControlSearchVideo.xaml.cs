using System.Diagnostics;
using Utify.Models.Objects;
using System.Windows.Input;
using YoutubeExplode.Videos;
using YoutubeExplode.Common;
using System.Windows.Controls;
using Utify.Models.Local.Clients;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Utify.Models.Objects.Interfaces;
using Utify.View.Usercontrols.ContextComponent;

namespace Utify.View.Usercontrols.SearchComponent
{
    /// <summary>
    /// Interaction logic for UserControlSearchVideo.xaml
    /// </summary>
    public partial class UserControlSearchVideo : UserControl, IContextMenu
    {
        #region Variables

        // Static.
        public event EventHandler OnDispose;

        // Public.
        public IVideo Video { get; private set; }

        public Dictionary<UIElement, List<BaseContextItem>> ContextMenus => new()
        {
            {
                GridTest, UserControlContextMenu()
            }
        };

        public List<BaseContextItem> UserControlContextMenu()
        {
            return new()
            {
                new UserControlContextItem(title: "Library - Add",
                                           settings: new(sender: Video,
                                                         action: new(AddLibraryAsync),
                                                         enabled: () => !IsPresentWithinLibrary())),
                new UserControlContextItem(title: "Library - Remove",
                                           settings: new(sender: Video,
                                                         action: new(RemoveLibraryAsync),
                                                         enabled: () => IsPresentWithinLibrary())),
                new UserControlContextDivider(),
                new UserControlContextItem(title: "Queue - Add (Next)",
                                           settings: new(sender: Video,
                                                         action: (s) => { })),
                new UserControlContextItem(title: "Queue - Add (Last)",
                                           settings: new(sender: Video,
                                                         action: (s) => { })),
                new UserControlContextDivider(),
                new UserControlContextItem(title: "Open in Chrome",
                                           settings: new(sender: Video,
                                                         action: new(OpenInBrowserAsync))),
                new UserControlContextItem(title: "Open in Explorer",
                                           settings: new(sender: Video,
                                                         action: new(OpenInExplorerAsync),
                                                         enabled: () => false)),
            };
        }

        // Private.

        private readonly PlaylistClient PlaylistClient = MainWindow.Instance.PlaylistClient;

        #endregion

        #region OnLoaded

        /// <summary>
        /// Empty constructor for phantom loading element.
        /// </summary>
        public UserControlSearchVideo()
        {
            InitializeComponent();
        }

        public UserControlSearchVideo(IVideo video)
        {
            Video = video;
            InitializeComponent();
        }

        private void UserControlInitialized(object sender, EventArgs e)
        {
            // Return on loading.
            if (Video == null)
                return;

            this.RegisterActivators();

            // Create a new task client.
            TaskClient image = new();
            image.TaskCompleted += (s, e) =>
            {
                // Set the image upon completion.
                if (e.Result is BitmapImage image)
                    Dispatcher.Invoke(() => ImageThumbnail.ImageSource = image);
            };

            // Set the URL, and download the image.
            Uri url = new(Video.Thumbnails.GetWithHighestResolution().Url);
            _ = image.RunTask(async () => await url.DownloadImageAsync());

            // Set the duration amount and text.
            TextBlockTitle.Text = Video.Title.Clamp(50);
            TextBlockDuration.Text = Video.Duration == null ?
                "Livestream" : ((TimeSpan)Video.Duration).GetElapsedTime();

            LabelAuthor.Content = Video.Author;
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            OnDispose?.Invoke(this, new());
        }

        #endregion

        #region Events

        #region Context

        private bool IsPresentWithinLibrary()
        {
            return PlaylistClient.Library.Songs.Any(x => x.Id.Equals(Video.Id));
        }

        private async void AddLibraryAsync(object sender)
        {
            // Create a new song using the video, and add it.
            await PlaylistClient.Library.AddAsync(new Song(Video));
        }

        private async void RemoveLibraryAsync(object sender)
        {
            // Attempt to fetch the corresponding song.
            Song? song = await PlaylistClient.Library.GetAsync(Video);

            // Return on null result.
            if (song == null)
                return;

            // Remove the song from the library.
            await PlaylistClient.Library.RemoveAsync(song);
        }

        private void OpenInBrowserAsync(object sender)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = Video.Url,
                UseShellExecute = true
            });
        }

        private async void OpenInExplorerAsync(object sender)
        {
            // Attempt to fetch the corresponding song.
            Song? song = await PlaylistClient.Library.GetAsync(Video);

            // If the song isn't found, or the location is empty, return.
            if (song == null || string.IsNullOrEmpty(song.Location))
                return;

            // Launch in explorer.
            Process.Start("explorer.exe", $"/select, {song.Location}");
        }

        #endregion

        private async void UserControlMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                return;

            await MainWindow.Instance.Audio.PlayAsync(new Song(Video));
        }

        #endregion
    }
}
