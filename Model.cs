using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SourceStructureAnalyser
{
    [XmlRoot("SourceStructure")]
    public class Model
    {
        private static readonly XmlSerializer _Serializer = new XmlSerializer(typeof(Model));

        private static readonly XmlWriterSettings _Write = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };

        public void Save(Stream stream)
        {
            using (var write = XmlWriter.Create(stream, _Write))
                _Serializer.Serialize(stream, this);
        }

        public void Save(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                Save(stream);
        }

        public static Model Load(Stream stream)
        {
            using (var read = XmlReader.Create(stream))
                return (Model)_Serializer.Deserialize(read);
        }

        public static Model Load(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Load(stream);
        }
    }
}
