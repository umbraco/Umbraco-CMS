using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.XPath;
using Examine.LuceneEngine.Providers;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.LegacyXmlPublishedCache
{
    /// <summary>
    /// An IPublishedContent that is represented all by a dictionary.
    /// </summary>
    /// <remarks>
    /// This is a helper class and definitely not intended for public use, it expects that all of the values required
    /// to create an IPublishedContent exist in the dictionary by specific aliases.
    /// </remarks>
    internal class DictionaryPublishedContent : PublishedContentBase
    {
        // note: I'm not sure this class fully complies with IPublishedContent rules especially
        // I'm not sure that _properties contains all properties including those without a value,
        // neither that GetProperty will return a property without a value vs. null... @zpqrtbnk

        // List of properties that will appear in the XML and do not match
        // anything in the ContentType, so they must be ignored.
        private static readonly string[] IgnoredKeys = { "version", "isDoc" };

        public DictionaryPublishedContent(
            IReadOnlyDictionary<string, string> valueDictionary,
            Func<int, IPublishedContent> getParent,
            Func<int, XPathNavigator, IEnumerable<IPublishedContent>> getChildren,
            Func<DictionaryPublishedContent, string, IPublishedProperty> getProperty,
            IAppCache appCache,
            PublishedContentTypeCache contentTypeCache,
            XPathNavigator nav,
            bool fromExamine)
        {
            if (valueDictionary == null) throw new ArgumentNullException(nameof(valueDictionary));
            if (getParent == null) throw new ArgumentNullException(nameof(getParent));
            if (getProperty == null) throw new ArgumentNullException(nameof(getProperty));

            _getParent = new Lazy<IPublishedContent>(() => getParent(ParentId));
            _getChildren = new Lazy<IEnumerable<IPublishedContent>>(() => getChildren(Id, nav));
            _getProperty = getProperty;
            _appCache = appCache;

            LoadedFromExamine = fromExamine;

            ValidateAndSetProperty(valueDictionary, val => _id = Int32.Parse(val), "id", "nodeId", "__NodeId"); //should validate the int!
            ValidateAndSetProperty(valueDictionary, val => _key = Guid.Parse(val), "key", "__key", "__Key");
            //ValidateAndSetProperty(valueDictionary, val => _templateId = int.Parse(val), "template", "templateId");
            ValidateAndSetProperty(valueDictionary, val => _sortOrder = Int32.Parse(val), "sortOrder");
            ValidateAndSetProperty(valueDictionary, val => _name = val, "nodeName");
            ValidateAndSetProperty(valueDictionary, val => _urlName = val, "urlName");
            ValidateAndSetProperty(valueDictionary, val => _documentTypeAlias = val, "nodeTypeAlias", LuceneIndex.ItemTypeFieldName);
            ValidateAndSetProperty(valueDictionary, val => _documentTypeId = Int32.Parse(val), "nodeType");
            //ValidateAndSetProperty(valueDictionary, val => _writerName = val, "writerName");
            ValidateAndSetProperty(valueDictionary, val => _creatorName = val, "creatorName", "writerName"); //this is a bit of a hack fix for: U4-1132
            //ValidateAndSetProperty(valueDictionary, val => _writerId = int.Parse(val), "writerID");
            ValidateAndSetProperty(valueDictionary, val => _creatorId = Int32.Parse(val), "creatorID", "writerID"); //this is a bit of a hack fix for: U4-1132
            ValidateAndSetProperty(valueDictionary, val => _path = val, "path", "__Path");
            ValidateAndSetProperty(valueDictionary, val => _createDate = ParseDateTimeValue(val), "createDate");
            ValidateAndSetProperty(valueDictionary, val => _updateDate = ParseDateTimeValue(val), "updateDate");
            ValidateAndSetProperty(valueDictionary, val => _level = Int32.Parse(val), "level");
            ValidateAndSetProperty(valueDictionary, val =>
            {
                int pId;
                ParentId = -1;
                if (Int32.TryParse(val, out pId))
                {
                    ParentId = pId;
                }
            }, "parentID");

            _contentType = contentTypeCache.Get(PublishedItemType.Media, _documentTypeAlias);
            _properties = new Collection<IPublishedProperty>();

            //handle content type properties
            //make sure we create them even if there's no value
            foreach (var propertyType in _contentType.PropertyTypes)
            {
                var alias = propertyType.Alias;
                _keysAdded.Add(alias);
                string value;
                const bool isPreviewing = false; // false :: never preview a media
                var property = valueDictionary.TryGetValue(alias, out value) == false || value == null
                    ? new XmlPublishedProperty(propertyType, this, isPreviewing)
                    : new XmlPublishedProperty(propertyType, this, isPreviewing, value);
                _properties.Add(property);
            }

            //loop through remaining values that haven't been applied
            foreach (var i in valueDictionary.Where(x =>
                _keysAdded.Contains(x.Key) == false // not already processed
                && IgnoredKeys.Contains(x.Key) == false)) // not ignorable
            {
                if (i.Key.InvariantStartsWith("__"))
                {
                    // no type for that one, dunno how to convert, drop it
                    //IPublishedProperty property = new PropertyResult(i.Key, i.Value, PropertyResultType.CustomProperty);
                    //_properties.Add(property);
                }
                else
                {
                    // this is a property that does not correspond to anything, ignore and log
                    Current.Logger.Warn<PublishedMediaCache,string>("Dropping property '{PropertyKey}' because it does not belong to the content type.", i.Key);
                }
            }
        }

        private DateTime ParseDateTimeValue(string val)
        {
            if (LoadedFromExamine == false)
                return DateTime.Parse(val);

            //we need to parse the date time using Lucene converters
            var ticks = Int64.Parse(val);
            return new DateTime(ticks);
        }

        /// <summary>
        /// Flag to get/set if this was loaded from examine cache
        /// </summary>
        internal bool LoadedFromExamine { get; }

        //private readonly Func<DictionaryPublishedContent, IPublishedContent> _getParent;
        private readonly Lazy<IPublishedContent> _getParent;
        //private readonly Func<DictionaryPublishedContent, IEnumerable<IPublishedContent>> _getChildren;
        private readonly Lazy<IEnumerable<IPublishedContent>> _getChildren;
        private readonly Func<DictionaryPublishedContent, string, IPublishedProperty> _getProperty;
        private readonly IAppCache _appCache;

        /// <summary>
        /// Returns 'Media' as the item type
        /// </summary>
        public override PublishedItemType ItemType => PublishedItemType.Media;

        public override IPublishedContent Parent => _getParent.Value;

        public int ParentId { get; private set; }

        public override int Id => _id;

        public override Guid Key => _key;

        public override int? TemplateId => null;

        public override int SortOrder => _sortOrder;

        public override string Name => _name;

        private static readonly Lazy<Dictionary<string, PublishedCultureInfo>> NoCultures = new Lazy<Dictionary<string, PublishedCultureInfo>>(() => new Dictionary<string, PublishedCultureInfo>());
        public override IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => NoCultures.Value;

        public override string UrlSegment => _urlName;

        public override string WriterName => _creatorName;

        public override string CreatorName => _creatorName;

        public override int WriterId => _creatorId;

        public override int CreatorId => _creatorId;

        public override string Path => _path;

        public override DateTime CreateDate => _createDate;

        public override DateTime UpdateDate => _updateDate;

        public override int Level => _level;

        public override bool IsDraft(string culture = null) => false;

        public override bool IsPublished(string culture = null) => true;

        public override IEnumerable<IPublishedProperty> Properties => _properties;

        public override IEnumerable<IPublishedContent> Children => _getChildren.Value;

        public override IEnumerable<IPublishedContent> ChildrenForAllCultures => Children;

        public override IPublishedProperty GetProperty(string alias)
        {
            return _getProperty(this, alias);
        }

        public override IPublishedContentType ContentType => _contentType;

        private readonly List<string> _keysAdded = new List<string>();
        private int _id;
        private Guid _key;
        //private int _templateId;
        private int _sortOrder;
        private string _name;
        private string _urlName;
        private string _documentTypeAlias;
        private int _documentTypeId;
        //private string _writerName;
        private string _creatorName;
        //private int _writerId;
        private int _creatorId;
        private string _path;
        private DateTime _createDate;
        private DateTime _updateDate;
        //private Guid _version;
        private int _level;
        private readonly ICollection<IPublishedProperty> _properties;
        private readonly IPublishedContentType _contentType;

        private void ValidateAndSetProperty(IReadOnlyDictionary<string, string> valueDictionary, Action<string> setProperty, params string[] potentialKeys)
        {
            var key = potentialKeys.FirstOrDefault(x => valueDictionary.ContainsKey(x) && valueDictionary[x] != null);
            if (key == null)
            {
                throw new FormatException("The valueDictionary is not formatted correctly and is missing any of the  '" + String.Join(",", potentialKeys) + "' elements");
            }

            setProperty(valueDictionary[key]);
            _keysAdded.Add(key);
        }
    }
}
