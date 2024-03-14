using Utify.View.Windows;
using System.Windows.Forms;
using System.Collections.Generic;
using UserControl = System.Windows.Controls.UserControl;

namespace Utify.View.Usercontrols.ContextComponent
{
    public delegate bool ContextItemSwitch();
    public delegate void ContextItemAction(object sender);

    public class BaseContextSettings
    {
        public object? Sender { get; set; }
        public bool HideOnAction { get; set; }
        public ContextItemAction? Action { get; set; }
        public ContextItemSwitch? Enabled { get; set; }
        public KeyValuePair<Keys, ContextItemAction>? Keybind { get; set; }
        public List<BaseContextItem>? ChildItems { get; set; }

        public BaseContextSettings(object? sender = null,
                                   bool hideOnAction = true,
                                   ContextItemAction? action = null,
                                   ContextItemSwitch? enabled = null,
                                   List<BaseContextItem>? childItems = null,
                                   KeyValuePair<Keys, ContextItemAction>? keybind = null)
        {
            Sender = sender;
            Action = action;
            Enabled = enabled;
            Keybind = keybind;
            ChildItems = childItems;
            HideOnAction = hideOnAction;
        }
    }

    public class BaseContextItem : UserControl
    {
        /// <summary>
        /// The parent of the ContextItem.
        /// </summary>
        public new ContextMenuWindow Parent { get; set; }

        /// <summary>
        /// The possible sender (variable) to expect on <see cref="Action"/> invoke.
        /// </summary>
        public object? Sender { get; private set; }

        /// <summary>
        /// Determains whether the control should hide after the action is called.
        /// </summary>
        public bool HideOnAction { get; private set; }

        /// <summary>
        /// The possible action on click.
        /// </summary>
        public ContextItemAction? Action { get; private set; }

        /// <summary>
        /// Determains whether the control is enabled (visible) or not (invisible).
        /// </summary>
        public ContextItemSwitch? Enabled { get; private set; }

        /// <summary>
        /// The key-action combination of a possible keybind.
        /// </summary>
        public KeyValuePair<Keys, ContextItemAction>? Keybind { get; private set; }

        /// <summary>
        /// The possible child of the ContextItem.
        /// </summary>
        public List<BaseContextItem>? ChildItems { get; private set; }

        public BaseContextItem(BaseContextSettings? settings = null)
        {
            if (settings == null)
                return;

            Sender = settings.Sender;
            Action = settings.Action;
            Enabled = settings.Enabled;
            Keybind = settings.Keybind;
            ChildItems = settings.ChildItems;
            HideOnAction = settings.HideOnAction;
        }
    }
}
