using YoutubeExplode;
using System.Windows.Input;
using YoutubeExplode.Search;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utify.View.Usercontrols.GeneralComponent;
using System.Threading;
using Utify.Models.Local.Clients;
using YoutubeExplode.Videos;

namespace Utify.View.Usercontrols.SearchComponent
{
    /// <summary>
    /// Interaction logic for UserControlSearch.xaml
    /// </summary>
    public partial class UserControlSearch : UserControlContentBase
    {
        #region Variables

        // Public.
        public bool IsSearching { get; private set; }

        // Private.

        #endregion

        #region OnLoaded

        public UserControlSearch()
        {
            InitializeComponent();
        }

        private void UserControlContentBaseInitialized(object sender, EventArgs e)
        {

        }

        #endregion

        #region Methods

        #endregion

        #region Events

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            // Check if the phantom text is still visible.
            if (!TextboxSearch.Text.ToLower().Equals("search"))
                return;

            // Reset the prompt.
            TextboxSearch.Text = "";
        }

        private void TextboxSearchKeyDown(object sender, KeyEventArgs e)
        {
            // Return on non enter or if the program is already searching.
            if (e.Key != Key.Enter || IsSearching)
                return;

            // Lock the searching until it's finished.
            IsSearching = true;

            // Search.
            YoutubeClient youtube = new();
            string query = TextboxSearch.Text;

            // Clear the prior search result.
            WrapPanelVideo.Children.Clear();
            StackPanelPlaylist.Children.Clear();

            // Create some dummy records for immediate loading feedback.
            for (int i = 0; i < 50; i++)
                WrapPanelVideo.Children.Add(new UserControlLoading(child: new UserControlSearchVideo()));
            StackPanelPlaylist.Children.Add(new UserControlLoading(parent: StackPanelPlaylist));

            // Create a channel task client.
            TaskClient channel = new();
            channel.TaskCompleted += OnChannelTaskCompleted;

            // Create a new user control, and load the required information.
            var channelTask = channel.RunTask(async () =>
            {
                UserControlSearchChannel ucsc = new(query);
                var result = await ucsc.LoadChannelAsync();
                return new object[] { ucsc, result };
            });

            // Create a video task client.
            TaskClient video = new();
            video.TaskCompleted += OnVideoTaskCompleted;

            // Search a list of video's with the corresponding query.
            var videoTask = video.RunTask(async () =>
            {
                return await youtube.Search.GetVideosAsync(query).ToListAsync(24);
            });

            IsSearching = false;
        }

        private async void OnChannelTaskCompleted(object? sender, TaskCompletedEventArgs e)
        {
            if (e.Result is object[] objects &&
                objects[0] is UserControlSearchChannel ucsc &&
                objects[1] is KeyValuePair<ChannelSearchResult, List<Video>> kvp)

            {
                await Dispatcher.Invoke(async () =>
                {
                    // Set the channel and video info.
                    await ucsc.SetChannelInfoAsync(kvp.Key);
                    await ucsc.SetChannelVideosAsync(kvp.Value);

                    StackPanelPlaylist.Children.Add(ucsc);

                    // Remove all the dummy records.
                    StackPanelPlaylist.Children.OfType<UserControlLoading>()
                                      .ToList()
                                      .ForEach(x => StackPanelPlaylist.Children.Remove(x));
                });
            }
        }

        private void OnVideoTaskCompleted(object? sender, TaskCompletedEventArgs e)
        {
            if (e.Result is List<VideoSearchResult> videos)
            {
                // Loop over the video's, create new user controls and add.
                foreach (VideoSearchResult result in videos)
                {
                    UserControlSearchVideo ucsv = new(result);
                    WrapPanelVideo.Children.Add(ucsv);
                }

                // Remove all the dummy records.
                WrapPanelVideo.Children.OfType<UserControlLoading>()
                                       .ToList()
                                       .ForEach(x => WrapPanelVideo.Children.Remove(x));
            }
        }

        #endregion
    }
}