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
        internal DynamicMediaList ownerList;

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
        public DynamicMedia(string mediaId)
        {
            int iMediaId = 0;
            if (int.TryParse(mediaId, out iMediaId))
            {
                _media = new Media(iMediaId);
                _propertyCache = new Dictionary<string, string>();
            }
        }
        public DynamicMedia(object mediaId)
        {
            int iMediaId = 0;
            if (int.TryParse(string.Format("{0}", mediaId), out iMediaId))
            {
                _media = new Media(iMediaId);
                _propertyCache = new Dictionary<string, string>();
            }
        }
        public DynamicMedia()
        {

        }

        private DynamicMediaList _children;
        public DynamicMediaList Children
        {
            get
            {
                if (_children == null)
                {
                    List<DynamicMedia> nodes = new List<DynamicMedia>();
                    foreach (var mSub in _media.Children)
                        nodes.Add(new DynamicMedia(mSub));
                    _children = new DynamicMediaList(nodes);

                }

                return _children;
            }
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
                if (binder.Name.ToLower() == "name")
                {
                    result = _media.Text;
                    return true;
                }

                Property prop = _media.getProperty(name);
                // check for nicer support of Pascal Casing EVEN if alias is camelCasing:
                if (prop == null && name.Substring(0, 1).ToUpper() == name.Substring(0, 1))
                {
                    prop = _media.getProperty(name.Substring(0, 1).ToLower() + name.Substring((1)));
                }
                if (prop != null)
                {
                    result = prop.Value;
                    if (_propertyCache != null)
                    {
                        _propertyCache.Add(name, string.Format("{0}", prop.Value));
                    }
                    return true;
                }

                // failback to default properties
                try
                {
                    result = _media.GetType().InvokeMember(binder.Name,
                                                      System.Reflection.BindingFlags.GetProperty |
                                                      System.Reflection.BindingFlags.Instance |
                                                      System.Reflection.BindingFlags.Public |
                                                      System.Reflection.BindingFlags.NonPublic,
                                                      null,
                                                      _media,
                                                      null);
                    return true;
                }
                catch
                {
                }


                //return false because we have a media item now but the property doesn't exist
                result = null;
                return false;
            }
            result = null;
            //return true because the _media is likely null, meaning we're in test mode
            return true;
        }

        public bool IsNull()
        {
            return false;
        }
        public bool HasValue()
        {
            return true;
        }
    }
}
