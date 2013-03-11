using System;
using System.IO;
using System.Text;
using ServiceStack.Text;

namespace Umbraco.Core.Serialization
{
    public class ServiceStackXmlSerializer : ISerializer
    {
        public ServiceStackXmlSerializer()
        {
        }

        public object FromStream(Stream input, Type outputType)
        {
            return XmlSerializer.DeserializeFromStream(outputType, input);
        }

        public IStreamedResult ToStream(object input)
        {
            string output = XmlSerializer.SerializeToString(input);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(output));
            return new StreamedResult(stream, true);
        }
    }
}