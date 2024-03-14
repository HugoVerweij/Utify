using System.Windows.Input;

namespace Utify.View.Usercontrols.ContextComponent
{
    /// <summary>
    /// Interaction logic for UserControlContextItem.xaml
    /// </summary>
    public partial class UserControlContextItem : BaseContextItem
    {
        #region Variables

        public string Title { get; private set; }

        #endregion

        #region OnLoaded

        public UserControlContextItem(BaseContextSettings settings, string title) : base(settings)
        {
            Title = title;
            InitializeComponent();
        }

        private void UserControlInitialized(object sender, EventArgs e)
        {
            LabelItem.Content = Title;
            LabelMore.Visibility = ChildItems != null ?
                Visibility.Visible :
                Visibility.Hidden;
        }

        #endregion

        #region Events

        private void BorderMouseEnter(object sender, MouseEventArgs e)
        {
            if (ChildItems == null)
                return;

            Parent.OnChildWindowInvoked(this);
        }

        private void BorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (HideOnAction)
                // Activate the main window to 'hide' every context menu.
                MainWindow.Instance.Activate();

            // Invoke the given action if it's set.
            Action?.Invoke(Sender ?? this);
        }

        #endregion
    }
}
