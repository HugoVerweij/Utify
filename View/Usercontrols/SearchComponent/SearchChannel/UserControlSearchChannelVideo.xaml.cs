using System.Windows.Controls;
using YoutubeExplode.Videos;

namespace Utify.View.Usercontrols.SearchComponent
{
    /// <summary>
    /// Interaction logic for UserControlSearchChannelVideo.xaml
    /// </summary>
    public partial class UserControlSearchChannelVideo : UserControl
    {
        #region Variables

        // Public.
        public Video Video { get; private set; }

        // Private.

        #endregion

        #region OnLoaded

        public UserControlSearchChannelVideo(Video video)
        {
            Video = video;
            InitializeComponent();
        }

        private void UserControlInitialized(object sender, EventArgs e)
        {
            LabelTitle.Content = Video.Title;
            LabelDuration.Content = Video.Duration != null ? ((TimeSpan)Video.Duration).ToDurationString() : string.Empty;
            LabelInfo.Content = $"{Video.Engagement.ViewCount.ToViewCountString()} • {Video.UploadDate.ToRelativeDateString()}";
        }

        #endregion

        #region Methods

        #endregion

        #region Events

        #endregion
    }
}
