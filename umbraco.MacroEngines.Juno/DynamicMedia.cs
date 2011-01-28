using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.property;

namespace umbraco.MacroEngines
{
    public class DynamicMedia : DynamicObject
    {
        private Dictionary<string, string> _propertyCache;
        private Media _media;
        public DynamicMedia(int mediaId)
        {
            _media = new Media(mediaId);
            _propertyCache = new Dictionary<string, string>();
        }
        public DynamicMedia(Media media)
        {
            _media = media;
            _propertyCache = new Dictionary<string, string>();
        }
        public DynamicMedia()
        {

        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name;
            if (_propertyCache != null && _propertyCache.ContainsKey(name))
            {
                result = _propertyCache[name];
                return true;
            }
            if (_media != null)
            {
                Property prop = _media.getProperty(name);
                if (prop != null)
                {
                    result = prop.Value;
                    if (_propertyCache != null)
                    {
                        _propertyCache.Add(name, string.Format("{0}", prop.Value));
                    }
                    return true;
                }
                //return false because we have a media item now but the property doesn't exist
                result = null;
                return false;
            }
            result = null;
            //return true because the _media is likely null, meaning we're in test mode
            return true;
        }
    }
}
