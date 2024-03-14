using System.Collections.Generic;

namespace Utify.View.Usercontrols.ContextComponent
{
    /// <summary>
    /// Interaction logic for UserControlContextScroller.xaml
    /// </summary>
    public partial class UserControlContextScroller : BaseContextItem
    {
        #region Variables

        public double? LimitHeight { get; set; }

        public List<BaseContextItem> Children { get; set; }

        #endregion

        #region OnLoaded

        public UserControlContextScroller(List<BaseContextItem> items, double? maxheight = null, BaseContextSettings? settings = null) : base(settings)
        {
            InitializeComponent();

            Children = items;
            LimitHeight = maxheight;
        }

        private void BaseContextItemInitialized(object sender, EventArgs e)
        {
            // Set the max height.
            MaxHeight = (double)
                // Check if limit height isn't null.
                (LimitHeight != null ?
                    LimitHeight :
                // Check if parent isn't null.
                Parent != null ?
                    Parent.ActualHeight :
                    MaxHeight);

            // (Re)load the children.
            ReloadChildren();
        }

        #endregion

        #region Methods

        public void ReloadChildren()
        {
            // Clear the children.
            StackPanelItems.Children.Clear();

            // Loop through the children, and add the items.
            foreach (BaseContextItem item in Children)
                StackPanelItems.Children.Add(item);
        }

        #endregion
    }
}
