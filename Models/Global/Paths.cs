using System.IO;

namespace Utify
{
    public static class Paths
    {
        // Public.

        // Folders.
        public static string XML => Path.Combine(Environment.CurrentDirectory, "XML");
        public static string Songs => Path.Combine(XML, "Songs");
        public static string Playlists => Path.Combine(XML, "Playlists");

        // Files.
        public static string Library => Path.Combine(XML, $"Library.{Ext}");
        public static string Settings => Path.Combine(XML, $"Settings.{Ext}");

        // Ext.
        public static readonly string Ext = "utf";
        public static readonly string Song = "song";
        public static readonly string Playlist = "playlist";

        // Private.
    }
}
