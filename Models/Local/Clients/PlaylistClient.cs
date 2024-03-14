using Utify.Models.Objects;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Utify.Models.Local.Clients
{
    public class PlaylistClient
    {

        #region Variables

        // Public.
        public Playlist Library { get; private set; }
        public IReadOnlyList<Playlist> Playlists => playlists.AsReadOnly();

        // Private.
        private readonly List<Playlist> playlists;

        #endregion

        #region OnLoaded

        public PlaylistClient()
        {
            playlists = new();
        }

        public async Task<PlaylistClient> InitializeAsync()
        {
            // Create the directories if needed.
            Directory.CreateDirectory(Paths.Songs);
            Directory.CreateDirectory(Paths.Playlists);

            // Check if the file exists.
            if (File.Exists(Paths.Library))
            {
                // Attempt to load in the library.
                Library = await XMLClient.DeserializeToMemory<Playlist>(Paths.Library);
            }
            else
            {
                // Create a brand new library.
                Library = new("Library", Paths.Library);
                await Library.SaveAsync();
            }


            // Loop over reach found playlist file.
            foreach (string file in Directory.GetFiles(Paths.Playlists, Paths.Playlist))
            {
                // Load in each playlist.
                Playlist playlist = await XMLClient.DeserializeToMemory<Playlist>(file);
                playlists.Add(playlist);
            }

            return this;
        }

        #endregion

        #region Methods

        public async Task CreateAsync(Playlist playlist)
        {
            // Create a unique name with ticks.
            if (string.IsNullOrEmpty(playlist.Location))
                playlist.Location = $"{Path.Combine(Paths.Playlists, DateTime.Now.Ticks.ToString())}.{Paths.Playlist}";

            // Return on in use location.
            if (playlists.Any(x => x.Location.Equals(playlist.Location)))
                return;

            // Create the playlist and save it.
            playlists.Add(playlist);
            await playlist.SaveAsync();
        }

        #endregion

        #region Events

        #endregion
    }
}
