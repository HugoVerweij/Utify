using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Utify.View.Usercontrols.ContextComponent
{
    /// <summary>
    /// Interaction logic for UserControlContextInput.xaml
    /// </summary>
    public partial class UserControlContextInput : BaseContextItem
    {
        #region Variables

        // Public.
        public string Placement { get; private set; }

        public string Replacement { get; private set; }

        public string Result { get; private set; }

        // Private.
        private bool loaded;

        #endregion

        #region OnLoaded

        public UserControlContextInput(BaseContextSettings settings, string placement, string replacement = "") : base(settings)
        {
            Placement = placement;
            Replacement = replacement;
            InitializeComponent();
        }

        private void BaseContextItemInitialized(object sender, EventArgs e)
        {
            // Set the initial text.
            TextBoxInput.Text = Placement;
        }

        #endregion

        #region Methods

        #endregion

        #region Events

        private void TextBoxInputMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Return on loaded.
            if (loaded)
                return;

            // Force a single activation.
            loaded = true;

            // TODO : Move colors into a separate class.

            // Update the color.
            TextBoxInput.Foreground = FindResource("HighlightNormalBrush") as SolidColorBrush;

            // Force the selection to run after the mouseclick, so it doesn't deselect.
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                TextBoxInput.Text = Replacement;
                TextBoxInput.SelectAll();
            }));
        }

        private void TextBoxInputPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Return on non enter or invalid input.
            if (e.Key != Key.Enter ||
                string.IsNullOrWhiteSpace(TextBoxInput.Text))
                return;

            if (HideOnAction)
                // Activate the main window to 'hide' every context menu.
                MainWindow.Instance.Activate();

            // Set the result.
            Result = TextBoxInput.Text;

            // Prime for reuse.
            TextBoxInput.Text = "";

            // Invoke the action with the result.
            Action?.Invoke(this);
        }

        #endregion
    }
}
