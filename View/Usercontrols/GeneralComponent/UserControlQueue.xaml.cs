using Utify.Models.Objects;
using System.Windows.Controls;

namespace Utify.View.Usercontrols.GeneralComponent
{
    /// <summary>
    /// Interaction logic for UserControlQueue.xaml
    /// </summary>
    public partial class UserControlQueue : UserControl
    {
        #region Variables

        public Song Song { get; set; }

        #endregion


        #region OnLoaded

        public UserControlQueue(Song song)
        {
            Song = song;
            InitializeComponent();
        }

        private void UserControlInitialized(object sender, EventArgs e)
        {
            LabelTitle.Content = Song.Title;
            LabelDuration.Content = TimeSpan.FromTicks(Song.Duration).ToDurationString();
        }

        #endregion
    }
}
