using System.Collections.Generic;
using System.Windows.Input;
using Utify.View.Usercontrols.ContextComponent;

namespace Utify.Models.Objects.Interfaces
{
    public static class ContextMenuExtension
    {
        /// <summary>
        /// Loops through the various context menus, and registers their activator.
        /// </summary>
        /// <param name="context"></param>
        public static void RegisterActivators(this IContextMenu context)
        {
            // Loop through the various menus.
            foreach (var menu in context.ContextMenus)
            {
                void register(object s, MouseButtonEventArgs e)
                {
                    // Register the activator.
                    MainWindow.Instance.ContextMenu.RegisterActivator(menu.Key, menu.Value, e);
                }

                // Subscribe to the activator's mouse down event.
                menu.Key.MouseDown += register;

                // Unsubscribe to the activator's mouse down event.
                context.OnDispose += (s, e) =>
                    menu.Key.MouseDown -= register;
            }
        }

    }

    public interface IContextMenu : IDisposable
    {
        public event EventHandler OnDispose;

        /// <summary>
        /// The dataset that holds the activator: <see cref="UIElement"/>, and items: <see cref="List{BaseContextItem}<"/>.
        /// </summary>
        public Dictionary<UIElement, List<BaseContextItem>> ContextMenus { get; }
    }
}
