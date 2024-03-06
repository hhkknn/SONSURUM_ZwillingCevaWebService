using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace ZwillingCevaWebService.Utils
{

    public static class XmlUtils
    {
        /// <summary>
        /// Serializes the specified object value to XML.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="encoding">The encoding</param>
        /// <param name="xmlSerializerNamespaces">The xml serializer namespaces.</param>
        /// <param name="xmlProcessingInstructions">The xml processing instructions.</param>
        /// <returns>The serialized xml.</returns>
        public static string SerializeToXml<TValue>(TValue value, Encoding encoding = null, XmlSerializerNamespaces xmlSerializerNamespaces = null, IDictionary<string, string> xmlProcessingInstructions = null)
            where TValue : class
        {

            if (encoding == null)
            {
                encoding = new UTF8Encoding(false);
            }

            Type type = typeof(TValue);

            return SerializeToXml(type, value, encoding, xmlSerializerNamespaces, xmlProcessingInstructions);
        }

        /// <summary>
        /// Serializes to XML.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="encoding">The xml encoding.</param>
        /// <param name="xmlSerializerNamespaces">The xml serializer namespaces.</param>
        /// <param name="xmlProcessingInstructions">The xml processing instructions.</param>
        /// <returns>The serialized xml.</returns>
        private static string SerializeToXml(Type type, object value, Encoding encoding, XmlSerializerNamespaces xmlSerializerNamespaces = null, IEnumerable<KeyValuePair<string, string>> xmlProcessingInstructions = null)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                {
                    Encoding = new UTF8Encoding(false),
                    OmitXmlDeclaration = true,
                    Indent = true
                };
                //XmlSerializer serializer = new XmlSerializer(type);
                //var enc = new UTF8Encoding(false);
                //using (XmlTextWriter writer = new XmlTextWriter(ms, enc))
                //{
                //    xmlSerializerNamespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });
                //    xmlSerializerNamespaces.Add(string.Empty, string.Empty);
                //    serializer.Serialize(writer, value, xmlSerializerNamespaces);

                //    string xml = Encoding.UTF8.GetString(
                //    ms.GetBuffer(), 0, (int)ms.Length);

                //    return xml;
                //}


                using (XmlWriter xmlWriter = XmlTextWriter.Create(ms, xmlWriterSettings))
                {
                    if (xmlProcessingInstructions != null)
                    {
                        IEnumerator<KeyValuePair<string, string>> enumerator = xmlProcessingInstructions.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<string, string> keyValuePair = enumerator.Current;
                            xmlWriter.WriteProcessingInstruction(keyValuePair.Key, keyValuePair.Value);
                        }
                    }

                    XmlSerializer xmlSerializer = new XmlSerializer(type);

                    if (xmlSerializerNamespaces == null)
                    {
                        xmlSerializerNamespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });
                        xmlSerializerNamespaces.Add(string.Empty, string.Empty);
                        xmlSerializer.Serialize(xmlWriter, value, xmlSerializerNamespaces);
                    }
                    else
                    {
                        xmlSerializer.Serialize(xmlWriter, value, xmlSerializerNamespaces);
                    }


                    return encoding.GetString(
                    ms.GetBuffer(), 0, (int)ms.Length);
                }
            }
        }

        /// <summary>
        /// Formats the XML.
        /// </summary>
        /// <param name="xmlText">The XML text.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public static string FormatXml(string xmlText, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (XmlTextWriter writer = new XmlTextWriter(memoryStream, encoding))
                {
                    XmlDocument document = new XmlDocument();

                    // Load the XmlDocument with the XML.
                    document.LoadXml(xmlText);

                    writer.Formatting = Formatting.Indented;

                    // Write the XML into a formatting XmlTextWriter
                    document.WriteContentTo(writer);
                    writer.Flush();
                    memoryStream.Flush();

                    // Have to rewind the MemoryStream in order to read
                    // its contents.
                    memoryStream.Position = 0;

                    // Read MemoryStream contents into a StreamReader.
                    StreamReader sReader = new StreamReader(memoryStream);

                    // Extract the text from the StreamReader.
                    return sReader.ReadToEnd();
                }
            }
        }

        public static T Deserialize<T>(string input)
        where T : class
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));

            using (StringReader sr = new StringReader(input))
                return (T)ser.Deserialize(sr);
        }
    }
}