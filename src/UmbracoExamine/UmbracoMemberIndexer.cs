using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Config;

using Lucene.Net.Analysis;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;

using UmbracoExamine.DataServices;

namespace UmbracoExamine {

    /// <summary>
    /// Custom indexer for members
    /// </summary>
    public class UmbracoMemberIndexer : UmbracoContentIndexer
    {
        private readonly Dictionary<string, Func<XElement, string>> _customFields = new Dictionary<string, Func<XElement, string>>();
        private readonly IMemberService _memberService;
        private readonly IMemberTypeService _memberTypeService;
        private readonly IDataTypeService _dataTypeService;

        /// <summary>
        /// Default constructor
        /// </summary>
        public UmbracoMemberIndexer() : base()
        {
            InitializeCustomFields();
            _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
            _memberService = ApplicationContext.Current.Services.MemberService;
            _memberTypeService = ApplicationContext.Current.Services.MemberTypeService;
        }

        private void InitializeCustomFields()
        {
            if (_customFields.Any() == false)
            {
                _customFields.Add("_searchEmail", MakeSearchableEmail);
                _customFields.Add("_searchLastLockoutDate", MakeSearchableLockoutDate);
                _customFields.Add("_searchLastLoginDate", MakeSearchableLoginDate);
                _customFields.Add("_searchLastPasswordChangeDate", MakeSearchablePasswordChangeDate);
            }
        }

        private static string MakeSearchableEmail(XElement node)
        {
            var emailAttribute = node.Attribute("email");
            if (emailAttribute != null)
            {
                return emailAttribute.Value.Replace(".", " ").Replace("@", " ");
            }
            return null;
        }

        private static string MakeSearchableLockoutDate(XElement node) {
            
            var lastLockoutDateElement = node.Element(Constants.Conventions.Member.LastLockoutDate);
            if(lastLockoutDateElement != null)
            {
                return GetSortableDateString(lastLockoutDateElement.Value);
            }
            return null;
        }

        private static string GetSortableDateString(string value)
        {
            DateTime date;
            //This unspecified format appears beacuse these properties are labels on the member object, and are stored as text in the database, not proper datetimes.
            if (DateTime.TryParseExact(value, "dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentUICulture, DateTimeStyles.None, out date))
            {
                return date.ToString("s");
            }
            return value;
        }

        private static string MakeSearchableLoginDate(XElement node) {
            var lastLoginElement = node.Element(Constants.Conventions.Member.LastLoginDate);
            if(lastLoginElement != null) {
                return GetSortableDateString(lastLoginElement.Value);
            }
            return null;
        }

        private static string MakeSearchablePasswordChangeDate(XElement node) {
            var lastPasswordChangeDateElement = node.Element(Constants.Conventions.Member.LastPasswordChangeDate);
            if(lastPasswordChangeDateElement != null) {
                return GetSortableDateString(lastPasswordChangeDateElement.Value);
            }
            return null;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="analyzer"></param>
        [Obsolete("Use the overload that specifies the Umbraco services")]
        public UmbracoMemberIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService, Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async)
        {
            InitializeCustomFields();
            _dataTypeService = ApplicationContext.Current.Services.DataTypeService;
            _memberService = ApplicationContext.Current.Services.MemberService;
            _memberTypeService = ApplicationContext.Current.Services.MemberTypeService;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="memberService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        [Obsolete("Use the ctor specifying all dependencies instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public UmbracoMemberIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService,
              IDataTypeService dataTypeService,
              IMemberService memberService,
              Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async)
        {
            InitializeCustomFields();
            _dataTypeService = dataTypeService;
            _memberService = memberService;
            _memberTypeService = ApplicationContext.Current.Services.MemberTypeService;
        }

        /// <summary>
        /// Constructor to allow for creating an indexer at runtime
        /// </summary>
        /// <param name="indexerData"></param>
        /// <param name="indexPath"></param>
        /// <param name="dataService"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="memberService"></param>
        /// <param name="memberTypeService"></param>
        /// <param name="analyzer"></param>
        /// <param name="async"></param>
        public UmbracoMemberIndexer(IIndexCriteria indexerData, DirectoryInfo indexPath, IDataService dataService,
              IDataTypeService dataTypeService,
              IMemberService memberService,
              IMemberTypeService memberTypeService,
              Analyzer analyzer, bool async)
            : base(indexerData, indexPath, dataService, analyzer, async)
        {
            InitializeCustomFields();
            _dataTypeService = dataTypeService;
            _memberService = memberService;
            _memberTypeService = memberTypeService;
        }

        /// <summary>
        /// Ensures that the'_searchEmail', as well as the date properties are added to the user fields so that they are indexed - without having to modify the config
        /// </summary>
        /// <param name="indexSet"></param>
        /// <returns></returns>
        protected override IIndexCriteria GetIndexerData(IndexSet indexSet)
        {
            var indexerData = base.GetIndexerData(indexSet);

            if (CanInitialize())
            {
                var customIndexFields = new List<IIndexField>();
                foreach (var customFieldName in _customFields.Keys)
                {
                    if(indexerData.UserFields.Any(x => x.Name == customFieldName) == false) {
                        customIndexFields.Add(GetCustomIndexField(customFieldName));
                    }
                }
                return new IndexCriteria(
                    indexerData.StandardFields,
                    indexerData.UserFields.Concat(customIndexFields),
                    indexerData.IncludeNodeTypes,
                    indexerData.ExcludeNodeTypes,
                    indexerData.ParentNodeId
                );
            }

            return indexerData;
        }

        private IndexField GetCustomIndexField(string fieldName)
        {
            var field = new IndexField { Name = fieldName };
            StaticField policy;
            if (IndexFieldPolicies.TryGetValue(fieldName, out policy))
            {
                field.Type = policy.Type;
                field.EnableSorting = policy.EnableSorting;
            }
            return field;
        }

        /// <summary>
        /// The supported types for this indexer
        /// </summary>
        protected override IEnumerable<string> SupportedTypes
        {
            get
            {
                return new string[] { IndexTypes.Member };
            }
        }

        /// <summary>
        /// Reindex all members
        /// </summary>
        /// <param name="type"></param>
        protected override void PerformIndexAll(string type)
        {
            //This only supports members
            if (SupportedTypes.Contains(type) == false)
                return;

            DataService.LogService.AddInfoLog(-1, string.Format("PerformIndexAll - Start data queries - {0}", type));
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                if (DisableXmlDocumentLookup == false)
                {
                    ReindexWithXmlEntries(type, -1,
                        () => _memberTypeService.GetAll().ToArray(),
                        (path, pIndex, pSize) =>
                        {
                            long totalContent;
                            var result = _memberService.GetPagedXmlEntries(pIndex, pSize, out totalContent).ToArray();
                            var more = result.Length == pSize;
                            return Tuple.Create(result, more);
                        },
                        i => _memberService.GetById(i));
                }
                else
                {
                    const int pageSize = 1000;
                    var pageIndex = 0;

                    IMember[] members;

                    if (IndexerData.IncludeNodeTypes.Any())
                    {
                        //if there are specific node types then just index those
                        foreach (var nodeType in IndexerData.IncludeNodeTypes)
                        {
                            do
                            {
                                long total;
                                members = _memberService.GetAll(pageIndex, pageSize, out total, "LoginName", Direction.Ascending, true, null, nodeType).ToArray();

                                AddNodesToIndex(GetSerializedMembers(members), type);

                                pageIndex++;
                            } while (members.Length == pageSize);
                        }
                    }
                    else
                    {
                        //no node types specified, do all members
                        do
                        {
                            int total;
                            members = _memberService.GetAll(pageIndex, pageSize, out total).ToArray();

                            AddNodesToIndex(GetSerializedMembers(members), type);

                            pageIndex++;
                        } while (members.Length == pageSize);
                    }
                }
            }
            finally
            {
                stopwatch.Stop();
            }

            DataService.LogService.AddInfoLog(-1, string.Format("PerformIndexAll - End data queries - {0}, took {1}ms", type, stopwatch.ElapsedMilliseconds));
        }

        private IEnumerable<XElement> GetSerializedMembers(IEnumerable<IMember> members)
        {
            var serializer = new EntityXmlSerializer();
            return members.Select(member => serializer.Serialize(_dataTypeService, member));
        }

        protected override XDocument GetXDocument(string xPath, string type)
        {
            throw new NotSupportedException();
        }       
        
        /// <summary>
        /// Add the special __key and _searchEmail fields
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGatheringNodeData(IndexingNodeDataEventArgs e)
        {
            base.OnGatheringNodeData(e);

            if (e.Node.Attribute("key") != null)
            {
                if (e.Fields.ContainsKey(NodeKeyFieldName) == false)
                    e.Fields.Add(NodeKeyFieldName, e.Node.Attribute("key").Value);
            }
            foreach (var customField in _customFields)
            {
                AddCustomField(e, customField);
            }
            if (e.Fields.ContainsKey(IconFieldName) == false)
                e.Fields.Add(IconFieldName, (string)e.Node.Attribute("icon"));
        }

        private static void AddCustomField(IndexingNodeDataEventArgs eventArgs, KeyValuePair<string, Func<XElement, string>> customField)
        {
            if (eventArgs.Fields.ContainsKey(customField.Key) == false)
            {
                var customFieldValue = customField.Value(eventArgs.Node);
                if(customFieldValue != null) {
                    eventArgs.Fields.Add(customField.Key, customFieldValue);
                }
            }
        }

        protected override FieldIndexTypes GetPolicy(string fieldName)
        {
            if (TreatAsDate(fieldName))
            {
                return FieldIndexTypes.NOT_ANALYZED;
            }
            return base.GetPolicy(fieldName);
        }

        private bool TreatAsDate(string fieldName) {
            return fieldName.InvariantContains("Date") && _customFields.ContainsKey(fieldName);
        }

        private static XElement GetMemberItem(int nodeId)
        {
            //TODO: Change this so that it is not using the LegacyLibrary, just serialize manually!
            var nodes = LegacyLibrary.GetMember(nodeId);
            return XElement.Parse(nodes.Current.OuterXml);
        }
    }
}
