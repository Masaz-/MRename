using MRename.Properties;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MRename.Classes
{
    static class MSerializer
    {
        /// <summary>
        /// Writes the given object instance to an XML file.
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filepath">The file path to write the object instance to.</param>
        /// <param name="obj">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToXmlFile<T>(string filepath, T obj, bool append = false) where T : new()
        {
            TextWriter writer = null;

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                writer = new StreamWriter(filepath, append);
                serializer.Serialize(writer, obj);
            }
            finally
            {
                writer?.Close();
            }
        }

        /// <summary>
        /// Reads an object instance from an XML file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filepath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the XML file.</returns>
        public static T ReadFromXmlFile<T>(string filepath) where T : new()
        {
            T result;
            TextReader reader = null;

            try
            {
                var xmlReaderCfg = new XmlReaderSettings
                {
                    IgnoreWhitespace = false
                };
                reader = new StreamReader(filepath);
                var xml = reader.ReadToEnd();

                using (StringReader stringReader = new StringReader(xml))
                using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlReaderCfg))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    result = (T)serializer.Deserialize(xmlReader);
                }

            }
            finally
            {
                reader?.Close();
            }

            return result;
        }
    }
}
