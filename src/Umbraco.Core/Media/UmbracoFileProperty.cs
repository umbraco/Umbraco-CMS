using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Media
{
    public class UmbracoFileProperty
    {
        private readonly IMedia _media;

        public UmbracoFileProperty(IMedia media)
        {
            _media = media;
        }

        private Property Property()
        {
            return _media.Properties.FirstOrDefault(x => x.Alias.InvariantEquals("umbracoFile"));
        }

        public string Src 
        {
            get
            {
                var value = Property()?.Value as string;
                if (value != null && value.DetectIsJson())
                {
                    // the property value is a JSON serialized image crop data set - grab the "src" property as the file source
                    var jObject = JsonConvert.DeserializeObject<JObject>(value);
                    value = jObject != null ? jObject.GetValueAsString("src") : value;
                }
                return value;
            }
            set
            {
                var property = Property();
                if (property == null)
                {
                    return;
                }
                var filename = property?.Value as string;
                if (filename != null && filename.DetectIsJson())
                {
                    var jObject = JsonConvert.DeserializeObject<JObject>(value);
                    jObject.Property("src").Value = value;
                }

                property.Value = value;
            }
        }
    }
}
