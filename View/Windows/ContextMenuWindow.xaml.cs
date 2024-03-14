using AngleSharp.Dom;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using Utify.View.Usercontrols.ContextComponent;

namespace Utify.View.Windows
{
    /// <summary>
    /// Interaction logic for ContextMenuWindow.xaml
    /// </summary>
    public partial class ContextMenuWindow : Window
    {
        #region Variables

        public BaseContextItem Activator { get; private set; }
        public List<ContextMenuWindow> Children { get; private set; } = new();

        private bool handled = true;

        #endregion

        #region OnLoaded

        public ContextMenuWindow()
        {
            OnLoaded();
        }

        public ContextMenuWindow(BaseContextItem activator)
        {
            OnLoaded(activator);
        }

        private void OnLoaded(BaseContextItem? activator = null)
        {
            Closed += OnClosing;
            MainWindow.Instance.Activated += (s, e) => Owner = MainWindow.Instance;
            Activator = activator ?? Activator;

            InitializeComponent();

            if (activator != null)
                LoadContextItems(Activator.ChildItems);

            LoadWindowActivations();
        }

        #endregion

        #region Methods

        private void LoadContextItems(List<BaseContextItem>? items)
        {
            // Return on null items.
            if (items == null)
                return;

            // Reset the panel.
            StackPanelItems.Children.Clear();

            // Loop through the enabled items.
            foreach (BaseContextItem item in items.Except(items.Where(x => x.Enabled != null && !x.Enabled.Invoke())))
                RegisterContextItem(item);
        }

        private void RegisterContextItem(BaseContextItem item)
        {
            // Set the parent(s).
            item.Parent = this;

            // Add the control to the children.
            StackPanelItems.Children.Add(item);
        }

        private void LoadWindowActivations()
        {
            // Loop through the current active windows.
            foreach (Window window in Application.Current.Windows)
                RegisterWindow(window);
        }

        public void RegisterWindow(Window window)
        {
            // Hide on deactivation of the current ContextMenu(s).
            if (window.GetType() != typeof(ContextMenuWindow))
                // Hide the base window, and clear all the sub windows.
                window.Activated += (s, e) =>
                {
                    Children.ForEach(x => x.Close());
                    Children.Clear();
                    StackPanelItems.Children.Clear();
                    Hide();
                };
        }

        public void RegisterActivator(UIElement activator, List<BaseContextItem> items, MouseButtonEventArgs e)
        {
            // Return on non right mouse button.
            if (e.RightButton != MouseButtonState.Pressed)
                return;

            #region Soft-lock

            // Return if the left-right click isn't handled.
            if (!handled)
                return;

            // Soft-lock the function.
            handled = false;

            // Capture the mouse outside the application.
            activator.CaptureMouse();
            activator.MouseUp += HandleMouseUp;

            // Handle the mouse up event, in order to un-subscribe.
            void HandleMouseUp(object s, MouseButtonEventArgs e)
            {
                // Handle, release and unsubscribe.
                handled = true;
                activator.ReleaseMouseCapture();
                activator.MouseUp -= HandleMouseUp;
            }

            #endregion

            // Return if all of the items have an enabled status, and the status is false.
            if (items.All(x => x.Enabled != null) && items.All(x => !x.Enabled.Invoke()))
                return;

            // Load & refresh.
            LoadContextItems(items);

            // Convert & position.
            Point mouse = Mouse.GetPosition(activator);
            Point pos = activator.PointToScreen(mouse);

            // Recalculate for any screen size/amount.
            PresentationSource source = PresentationSource.FromVisual(Owner);
            Left = pos.X / source.CompositionTarget.TransformToDevice.M11;
            Top = pos.Y / source.CompositionTarget.TransformToDevice.M22;

            Show();
        }

        public void HideAllChildren()
        {
            // Recursively hide everything.
            Children.ForEach(x => x.HideAllChildren());
            Hide();
        }

        #endregion

        #region Events

        private void OnClosing(object? sender, EventArgs e)
        {
            // Get rid of the stackpanel items.
            StackPanelItems.Children.Clear();
        }

        public void OnChildWindowInvoked(BaseContextItem context)
        {
            // Check if the ContextMenuWindow already exists.
            if (Children.Where(x => x.Activator.Equals(context)).FirstOrDefault() is ContextMenuWindow oldchild)
            {
                // Hide every child but the existing one.
                Children.Where(x => x != oldchild)
                        .ToList()
                        .ForEach(x => x.HideAllChildren());
                oldchild.Show();
                return;
            }

            // Create a new window.
            ContextMenuWindow newchild = new(context);

            // Set the window's location.
            Point relative = PointToScreen(new Point(0d, 0d));
            Vector offset = VisualTreeHelper.GetOffset(context);
            newchild.Left = Left + ActualWidth;
            newchild.Top = relative.Y + offset.Y;

            // Hide every other window.
            Children.ForEach(x => x.HideAllChildren());
            Children.Add(newchild);
            newchild.Show();
        }

        #endregion
    }
}
