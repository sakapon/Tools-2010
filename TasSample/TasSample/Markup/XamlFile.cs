using System.Windows.Markup;
using System.Xml;

namespace TasSample.Markup
{
    public static class XamlFile
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <exception cref="XamlParseException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T Read<T>(string filePath)
        {
            using (XmlReader reader = XmlReader.Create(filePath))
            {
                return (T)XamlReader.Load(reader);
            }
        }

        public static void Write<T>(string filePath, T obj)
        {
            using (XmlWriter writer = XmlWriter.Create(filePath, CreateXmlWriterSettingsForXamlWriter()))
            {
                XamlWriter.Save(obj, writer);
            }
        }

        private static XmlWriterSettings CreateXmlWriterSettingsForXamlWriter()
        {
            return new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };
        }
    }
}
