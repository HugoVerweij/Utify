using System.Xml.Serialization;

namespace Utify.Models.Objects
{
    [Serializable]
    [XmlType("Settings")]
    public class Settings
    {
        // General.

        [XmlAttribute("Volume")]
        public int Volume { get; set; }

        [XmlAttribute("AudioOnly")]
        public bool AudioOnly { get; set; }

        // Skipp specific requests.
    }
}
