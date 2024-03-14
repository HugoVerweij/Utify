using YoutubeExplode.Videos;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Utify.Models.Local.Clients;
using System.Collections.Generic;

namespace Utify.Models.Objects
{
    public static class PlaylistExtensions
    {
        public static Task<Song?> GetAsync(this Playlist playlist, IVideo song)
        {
            return Task.FromResult(playlist.Songs.FirstOrDefault(x => x.Id.Equals(song.Id)));
        }

        public static async Task AddAsync(this Playlist playlist, Song song)
        {
            playlist.Songs.Add(song);
            await playlist.SaveAsync();
        }

        public static async Task RemoveAsync(this Playlist playlist, Song song)
        {
            playlist.Songs.Remove(song);
            await playlist.SaveAsync();
        }

        public static async Task SaveAsync(this Playlist playlist)
        {
            await XMLClient.SerializeToFile(playlist, playlist.Location);
        }
    }

    [Serializable]
    [XmlType("Playlist")]
    public class Playlist
    {
        [XmlAttribute("Playlist Name")]
        public string Name { get; set; }

        [XmlAttribute("Playlist Location")]
        public string Location { get; set; }

        [XmlAttribute("Playlist Thumbnail")]
        public string Thumbnail { get; set; }

        [XmlAttribute("Playlist Creation Date")]
        public string CreationDate { get; set; }

        [XmlElement("Playlist Songs")]
        public List<Song> Songs { get; set; }

        public Playlist()
        {
        }

        public Playlist(string name, string location = "", string thumbnail = "")
        {
            Songs = new();

            Name = name;
            Location = location;
            Thumbnail = thumbnail;
            CreationDate = DateTime.Now.Ticks.ToString();
        }
    }
}
