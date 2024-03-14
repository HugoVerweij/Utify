using YoutubeExplode.Videos;
using YoutubeExplode.Common;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Windows.Controls.Primitives;
using Utify.View.Usercontrols.GeneralComponent;
using System.Diagnostics;
using System.Threading.Channels;
using YoutubeExplode.Search;
using YoutubeExplode.Channels;
using YoutubeExplode;

namespace Utify.View.Usercontrols.SearchComponent
{
    /// <summary>
    /// Interaction logic for UserControlSearchChannel.xaml
    /// </summary>
    public partial class UserControlSearchChannel : UserControl
    {
        #region Variables

        // Public.
        public string Search { get; private set; }

        // Private.

        #endregion

        #region OnLoaded

        public UserControlSearchChannel(string search)
        {
            Search = search;
            InitializeComponent();
        }

        private void UserControlInitialized(object sender, EventArgs e)
        {

        }

        #endregion

        #region Methods

        public async Task<KeyValuePair<ChannelSearchResult, List<Video>>> LoadChannelAsync()
        {
            // Search.
            YoutubeClient youtube = new();

            // Fetch the channel and uploads.
            var channels = await youtube.Search.GetChannelsAsync(Search).ToListAsync();
            var uploads = await Task.Run(async () => await youtube.Channels.GetUploadsAsync(channels.First().Id)
                                                                           .ToListAsync());
            var channel = channels.First();

            // Fetch the individual videos and sort by popular.
            List<Video> Popular = await Task.Run(async () =>
            {
                // Define a new concurrent thread safe bag.
                ConcurrentBag<Video> videos = new();

                //ParallelOptions options = new() { MaxDegreeOfParallelism = 2 };

                // Loop over the uploads in parallel, and add their metadata to the bag.
                await Parallel.ForEachAsync(uploads, async (upload, token) =>
                {
                    videos.Add(await youtube.Videos.GetAsync(upload.Id, token));
                });

                // Return in correct order by descending.
                return videos.OrderByDescending(x => x.Engagement.LikeCount).ToList();
            });

            return new(channel, Popular);
        }

        public async Task SetChannelInfoAsync(ChannelSearchResult channel)
        {
            // Set the channel info.
            LabelChannel.Content = channel.Title;

            // Fetch the highest resolution thumbnail.
            Thumbnail channelThumbnail = channel.Thumbnails.GetWithHighestResolution();

            // Filter on non https.
            string url = channelThumbnail.Url.StartsWith("//") ?
                $"https:{channelThumbnail.Url}" : channelThumbnail.Url;
            Uri channelUrl = new(url);

            // Download and set the image.
            ImageChannel.ImageSource = await channelUrl.DownloadImageAsync();
        }

        public async Task SetChannelVideosAsync(List<Video> popular)
        {
            // Define the thumbnail image list.
            var playlists = new Image[]
            {
                ImagePlaylist1,
                ImagePlaylist2,
                ImagePlaylist3
            };

            for (int i = 0; i < playlists.Length; i++)
            {
                // Break on popular videon't
                if (popular.Count <= i)
                    break;

                // Fetch the highest resolution, download and set.
                Image playlist = playlists[i];
                Uri playlistUrl = new(popular[i].Thumbnails.GetWithHighestResolution().Url);
                playlist.Source = await playlistUrl.DownloadImageAsync();
            }

            foreach (Video video in popular.Take(10))
            {
                UserControlSearchChannelVideo ucscv = new(video);
                StackPanelVideos.Children.Add(ucscv);
            }
        }

        #endregion

        #region Events

        #endregion
    }
}
