using Utify.Models.Objects;
using System.Threading.Tasks;

namespace Utify.Models.Local.Clients
{
    public class SettingsClient
    {
        #region Variables

        // Static.
        public delegate void SettingsEventHandler(Settings settings);
        public event SettingsEventHandler OnVolumeInitialize;
        public event SettingsEventHandler OnAudioOnlyInitialize;

        // Public.
        public Settings Settings { get; private set; }

        // Private.

        #endregion

        #region OnLoaded

        public SettingsClient()
        {
        }

        public async Task<SettingsClient> InitializeAsync()
        {
            // Check if the XML file exists.
            if (!File.Exists(Paths.Settings))
            {
                // Create a new instance.
                Settings = new()
                {
                    Volume = 100
                };

                // Save the instance.
                await SaveAsync();
            }

            // Deserialize the settings.
            Settings ??= await XMLClient.DeserializeToMemory<Settings>(Paths.Settings);

            // Invoke the required events.
            OnVolumeInitialize?.Invoke(Settings);
            OnAudioOnlyInitialize?.Invoke(Settings);
            return this;
        }

        #endregion

        #region Methods

        public static Task<SettingsClient> CreateAsync()
        {
            // Create the settings with a factory pattern.
            SettingsClient settings = new();
            // Call the initialize and use it as an async ctor.
            return settings.InitializeAsync();
        }

        public async Task SaveAsync()
        {
            // Serialize the settings to an XML file.
            await XMLClient.SerializeToFile(Settings, Paths.Settings);
        }

        #endregion
    }
}
