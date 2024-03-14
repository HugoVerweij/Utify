using System.Windows.Controls;

namespace Utify.View.Usercontrols.GeneralComponent
{
    /// <summary>
    /// Interaction logic for UserControlLoading.xaml
    /// </summary>
    public partial class UserControlLoading : UserControl
    {
        #region Variables

        // Public.
        public FrameworkElement? ParentDummy { get; private set; }
        public UserControl? ChildDummy { get; private set; }

        #endregion

        #region OnLoaded

        public UserControlLoading(UserControl? child = null, FrameworkElement? parent = null)
        {
            if (child == null && parent == null)
                return;

            ParentDummy = parent;
            ChildDummy = child;

            InitializeComponent();

            Width = ParentDummy?.ActualWidth ?? ChildDummy.Width;
            Height = ParentDummy?.ActualHeight ?? ChildDummy.Height;
            Margin = ParentDummy?.Margin ?? ChildDummy.Margin;
        }

        #endregion
    }
}
