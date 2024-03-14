using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utify.Models.Local.Clients
{
    public class XMLClient
    {
        // Serialize
        private static async Task<MemoryStream?> SerializeAsync<T>(T data, string output = "")
        {
            try
            {
                // Force the task to run on a new thread.
                return await Task.Run(() =>
                {
                    // Create the doc.
                    XDocument doc = new();

                    // Assign the writer.
                    using (XmlWriter writer = doc.CreateWriter())
                    {
                        // Create the serializer and serialize the data.
                        XmlSerializer serializer = new(data.GetType());
                        serializer.Serialize(writer, data);
                    }


                    // Check if the output is set.
                    if (string.IsNullOrEmpty(output))
                    {
                        // Save the XML doc to a memorystream.
                        MemoryStream ms = new();
                        doc.Save(ms);

                        // Reset the position & return the stream.
                        ms.Position = 0;
                        return ms;
                    }
                    else
                    {
                        // Save the XMl as a file.
                        doc.Save(output);
                        return null;
                    }

                });
            }
            catch (Exception e)
            {
                // Throw on exception.
                throw new XmlException($"Something went wrong with the serialization: {e.Message}");
            }
        }

        // Deserialize
        private static async Task<T> DeserializeAsync<T>(Stream input)
        {
            try
            {
                // Force the task to run on a new thread.
                return await Task.Run(() =>
                {
                    // Create a new serializer.
                    XmlSerializer ser = new(typeof(T));
                    // Create a new reader.
                    using XmlReader reader = XmlReader.Create(input);
                    // Return the result.
                    return (T)ser.Deserialize(reader);

                });
            }
            catch (Exception e)
            {
                // Throw on exception.
                throw new XmlException($"Something went wrong with the deserialization: {e.Message}");
            }
            finally
            {
                // Dispose the dangling stream.
                await input.DisposeAsync();
            }
        }

        // Serialize XML object to memory
        public static async Task<MemoryStream?> SerializeToMemory<T>(T data)
        {
            return await SerializeAsync(data);
        }

        // Serialize XML object to file.
        public static async Task SerializeToFile(object data, string output)
        {
            await SerializeAsync(data, output);
        }

        // Deserialize XML file to memory
        public static async Task<T> DeserializeToMemory<T>(string input)
        {
            // Check if the file exists.
            if (!File.Exists(input))
                throw new FileNotFoundException("File does not exist.");

            return await DeserializeAsync<T>(new FileStream(input, FileMode.Open));
        }

        // Deserialize Xml MemoryStream to memory.
        public static async Task<T> DeserializeToMemory<T>(MemoryStream input)
        {
            return await DeserializeAsync<T>(input);
        }
    }
}
