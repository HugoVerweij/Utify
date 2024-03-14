using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using System.Xml.Serialization;

namespace Utify.Models.Objects
{
    [Serializable]
    [XmlType("Song")]
    public class Song
    {
        [XmlAttribute("Song Id")]
        public string Id { get; set; }

        [XmlAttribute("Song Title")]
        public string Title { get; set; }

        [XmlAttribute("Song Duration")]
        public long Duration { get; set; }

        [XmlAttribute("Song Location")]
        public string Location { get; set; }

        [XmlIgnore]
        public Thumbnail Thumbnail { get; set; }

        [XmlIgnore]
        public bool IsDownloaded => !string.IsNullOrEmpty(Location);

        public Song()
        {
        }

        public Song(IVideo video)
        {
            Id = video.Id;
            Title = video.Title;
            Duration = video.Duration != null ?
                       video.Duration.Value.Ticks : 0;
            Thumbnail = video.Thumbnails.GetWithHighestResolution();
        }
    }
}
