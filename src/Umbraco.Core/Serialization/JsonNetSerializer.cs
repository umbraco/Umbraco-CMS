using System;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Umbraco.Core.Serialization
{
    internal class JsonNetSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        public JsonNetSerializer()
        {
            _settings = new JsonSerializerSettings();

            //var customResolver = new CustomIgnoreResolver
            //    {
            //        DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.Public
            //    };
            //_settings.ContractResolver = customResolver;

            var javaScriptDateTimeConverter = new JavaScriptDateTimeConverter();

            _settings.Converters.Add(javaScriptDateTimeConverter);
            _settings.Converters.Add(new EntityKeyMemberConverter());
            _settings.Converters.Add(new KeyValuePairConverter());
            _settings.Converters.Add(new ExpandoObjectConverter());
            _settings.Converters.Add(new XmlNodeConverter());

            _settings.NullValueHandling = NullValueHandling.Include;
            _settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            _settings.TypeNameHandling = TypeNameHandling.Objects;
            _settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
        }

        #region Implementation of ISerializer

        /// <summary>
        /// Deserialize input stream to object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="outputType"></param>
        /// <returns></returns>
        public object FromStream(Stream input, Type outputType)
        {
            byte[] bytes = new byte[input.Length];
            input.Position = 0;
            input.Read(bytes, 0, (int)input.Length);
            string s = Encoding.UTF8.GetString(bytes);

            return JsonConvert.DeserializeObject(s, outputType, _settings);
        }

        /// <summary>
        /// Serialize object to streamed result
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IStreamedResult ToStream(object input)
        {
            string s = JsonConvert.SerializeObject(input, Formatting.Indented, _settings);
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            MemoryStream ms = new MemoryStream(bytes);

            return new StreamedResult(ms, true);
        }

        #endregion
    }
}
