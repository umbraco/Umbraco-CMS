using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Examine;
using Examine.Search;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Examine;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Search
{

    /// <summary>
    /// Used for internal Umbraco implementations of <see cref="ISearchableTree"/>
    /// </summary>
    public class UmbracoTreeSearcher
    {
        private readonly IExamineManager _examineManager;
        private readonly UmbracoContext _umbracoContext;
        private readonly ILocalizationService _languageService;
        private readonly IEntityService _entityService;
        private readonly UmbracoMapper _mapper;
        private readonly ISqlContext _sqlContext;
        private readonly IUmbracoTreeSearcherFields _umbracoTreeSearcherFields;
        private readonly AppCaches _appCaches;

        [Obsolete("Use constructor specifying all dependencies instead")]
        public UmbracoTreeSearcher(
            IExamineManager examineManager,
            UmbracoContext umbracoContext,
            ILocalizationService languageService,
            IEntityService entityService,
            UmbracoMapper mapper,
            ISqlContext sqlContext,
            IUmbracoTreeSearcherFields umbracoTreeSearcherFields)
            : this(examineManager, umbracoContext, languageService, entityService, mapper, sqlContext, umbracoTreeSearcherFields, Current.AppCaches)
        { }

        public UmbracoTreeSearcher(
            IExamineManager examineManager,
            UmbracoContext umbracoContext,
            ILocalizationService languageService,
            IEntityService entityService,
            UmbracoMapper mapper,
            ISqlContext sqlContext,
            IUmbracoTreeSearcherFields umbracoTreeSearcherFields,
            AppCaches appCaches)
        {
            _examineManager = examineManager ?? throw new ArgumentNullException(nameof(examineManager));
            _umbracoContext = umbracoContext;
            _languageService = languageService;
            _entityService = entityService;
            _mapper = mapper;
            _sqlContext = sqlContext;
            _umbracoTreeSearcherFields = umbracoTreeSearcherFields;
            _appCaches = appCaches;
        }

        /// <summary>
        /// Searches Examine for results based on the entity type
        /// </summary>
        /// <param name="query"></param>
        /// <param name="entityType"></param>
        /// <param name="totalFound"></param>
        /// <param name="searchFrom">
        /// A starting point for the search, generally a node id, but for members this is a member type alias
        /// </param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="ignoreUserStartNodes">If set to true, user and group start node permissions will be ignored.</param>
        /// <returns></returns>
        public IEnumerable<SearchResultEntity> ExamineSearch(
            string query,
            UmbracoEntityTypes entityType,
            int pageSize,
            long pageIndex, out long totalFound, string searchFrom = null, bool ignoreUserStartNodes = false)
        {
            return ExamineSearch(query, entityType, pageSize, pageIndex, culture: null, out totalFound, searchFrom, ignoreUserStartNodes);
        }

        public IEnumerable<SearchResultEntity> ExamineSearch(
            string query,
            UmbracoEntityTypes entityType,
            int pageSize,
            long pageIndex, string culture,
            out long totalFound, string searchFrom = null, bool ignoreUserStartNodes = false)
        {
            var sb = new StringBuilder();

            string type;
            var indexName = Constants.UmbracoIndexes.InternalIndexName;
            var fields = _umbracoTreeSearcherFields.GetBackOfficeFields().ToList();

            // TODO: Remove these checks in v9 when these interfaces merge
            ISet<string> fieldsToLoad = _umbracoTreeSearcherFields is IUmbracoTreeSearcherFields2 searcherFields2
                ? new HashSet<string>(searcherFields2.GetBackOfficeFieldsToLoad())
                : null;

            // TODO: WE should try to allow passing in a lucene raw query, however we will still need to do some manual string
            // manipulation for things like start paths, member types, etc...
            //if (Examine.ExamineExtensions.TryParseLuceneQuery(query))
            //{

            //}

            //special GUID check since if a user searches on one specifically we need to escape it
            if (Guid.TryParse(query, out var g))
            {
                query = "\"" + g.ToString() + "\"";
            }

            switch (entityType)
            {
                case UmbracoEntityTypes.Member:
                    indexName = Constants.UmbracoIndexes.MembersIndexName;
                    type = "member";
                    fields.AddRange(_umbracoTreeSearcherFields.GetBackOfficeMembersFields());
                    if (_umbracoTreeSearcherFields is IUmbracoTreeSearcherFields2 umbracoTreeSearcherFieldMember)
                    {
                        foreach(var field in umbracoTreeSearcherFieldMember.GetBackOfficeMembersFieldsToLoad())
                        {
                            fieldsToLoad.Add(field);
                        }
                    }
                    if (searchFrom != null && searchFrom != Constants.Conventions.MemberTypes.AllMembersListId && searchFrom.Trim() != "-1")
                    {
                        sb.Append("+__NodeTypeAlias:");
                        sb.Append(searchFrom);
                        sb.Append(" ");
                    }
                    break;
                case UmbracoEntityTypes.Media:
                    type = "media";
                    fields.AddRange(_umbracoTreeSearcherFields.GetBackOfficeMediaFields());
                    if (_umbracoTreeSearcherFields is IUmbracoTreeSearcherFields2 umbracoTreeSearcherFieldsMedia)
                    {
                        foreach (var field in umbracoTreeSearcherFieldsMedia.GetBackOfficeMediaFieldsToLoad())
                        {
                            fieldsToLoad.Add(field);
                        }
                    }

                    var allMediaStartNodes = _umbracoContext.Security.CurrentUser.CalculateMediaStartNodeIds(_entityService, _appCaches);
                    AppendPath(sb, UmbracoObjectTypes.Media, allMediaStartNodes, searchFrom, ignoreUserStartNodes, _entityService);
                    break;
                case UmbracoEntityTypes.Document:
                    type = "content";
                    fields.AddRange(_umbracoTreeSearcherFields.GetBackOfficeDocumentFields());
                    if (_umbracoTreeSearcherFields is IUmbracoTreeSearcherFields2 umbracoTreeSearcherFieldsDocument)
                    {
                        foreach (var field in umbracoTreeSearcherFieldsDocument.GetBackOfficeDocumentFieldsToLoad())
                        {
                            fieldsToLoad.Add(field);
                        }
                    }
                    var allContentStartNodes = _umbracoContext.Security.CurrentUser.CalculateContentStartNodeIds(_entityService, _appCaches);
                    AppendPath(sb, UmbracoObjectTypes.Document, allContentStartNodes, searchFrom, ignoreUserStartNodes, _entityService);
                    break;
                default:
                    throw new NotSupportedException("The " + typeof(UmbracoTreeSearcher) + " currently does not support searching against object type " + entityType);
            }

            if (!_examineManager.TryGetIndex(indexName, out var index))
                throw new InvalidOperationException("No index found by name " + indexName);

            var internalSearcher = index.GetSearcher();

            if (!BuildQuery(sb, query, searchFrom, fields, type))
            {
                totalFound = 0;
                return Enumerable.Empty<SearchResultEntity>();
            }

            var examineQuery = internalSearcher.CreateQuery().NativeQuery(sb.ToString());
            if (fieldsToLoad != null)
            {
                examineQuery.SelectFields(fieldsToLoad);
            }

            var result = examineQuery
                //only return the number of items specified to read up to the amount of records to fill from 0 -> the number of items on the page requested
                .Execute(Convert.ToInt32(pageSize * (pageIndex + 1)));

            totalFound = result.TotalItemCount;

            var pagedResult = result.Skip(Convert.ToInt32(pageIndex));

            switch (entityType)
            {
                case UmbracoEntityTypes.Member:
                    return MemberFromSearchResults(pagedResult.ToArray());
                case UmbracoEntityTypes.Media:
                    return MediaFromSearchResults(pagedResult);
                case UmbracoEntityTypes.Document:
                    return ContentFromSearchResults(pagedResult, culture);
                default:
                    throw new NotSupportedException("The " + typeof(UmbracoTreeSearcher) + " currently does not support searching against object type " + entityType);
            }
        }

        /// <summary>
        /// Searches with the <see cref="IEntityService"/> for results based on the entity type
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="totalFound"></param>
        /// <param name="searchFrom"></param>
        /// <returns></returns>
        public IEnumerable<SearchResultEntity> EntitySearch(UmbracoObjectTypes objectType, string query, int pageSize, long pageIndex, out long totalFound, string searchFrom = null)
        {
            //if it's a GUID, match it
            Guid.TryParse(query, out var g);

            var results = _entityService.GetPagedDescendants(objectType, pageIndex, pageSize, out totalFound,
                filter: _sqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains(query) || x.Key == g));
            return _mapper.MapEnumerable<IEntitySlim, SearchResultEntity>(results);
        }

        private bool BuildQuery(StringBuilder sb, string query, string searchFrom, List<string> fields, string type)
        {
            //build a lucene query:
            // the nodeName will be boosted 10x without wildcards
            // then nodeName will be matched normally with wildcards
            // the rest will be normal without wildcards

            var allLangs = _languageService.GetAllLanguages().Select(x => x.IsoCode.ToLowerInvariant()).ToList();

            // the chars [*-_] in the query will mess everything up so let's remove those
            query = Regex.Replace(query, "[\\*\\-_]", "");

            //check if text is surrounded by single or double quotes, if so, then exact match
            var surroundedByQuotes = Regex.IsMatch(query, "^\".*?\"$")
                                     || Regex.IsMatch(query, "^\'.*?\'$");

            if (surroundedByQuotes)
            {
                //strip quotes, escape string, the replace again
                query = query.Trim(Constants.CharArrays.DoubleQuoteSingleQuote);

                query = Lucene.Net.QueryParsers.QueryParser.Escape(query);

                //nothing to search
                if (searchFrom.IsNullOrWhiteSpace() && query.IsNullOrWhiteSpace())
                {
                    return false;
                }

                //update the query with the query term
                if (query.IsNullOrWhiteSpace() == false)
                {
                    //add back the surrounding quotes
                    query = string.Format("{0}{1}{0}", "\"", query);

                    sb.Append("+(");

                    AppendNodeNamePhraseWithBoost(sb, query, allLangs);

                    foreach (var f in fields)
                    {
                        //additional fields normally
                        sb.Append(f);
                        sb.Append(": (");
                        sb.Append(query);
                        sb.Append(") ");
                    }

                    sb.Append(") ");
                }
            }
            else
            {
                var trimmed = query.Trim(Constants.CharArrays.DoubleQuoteSingleQuote);

                //nothing to search
                if (searchFrom.IsNullOrWhiteSpace() && trimmed.IsNullOrWhiteSpace())
                {
                    return false;
                }

                //update the query with the query term
                if (trimmed.IsNullOrWhiteSpace() == false)
                {
                    query = Lucene.Net.QueryParsers.QueryParser.Escape(query);

                    var querywords = query.Split(Constants.CharArrays.Space, StringSplitOptions.RemoveEmptyEntries);

                    sb.Append("+(");

                    AppendNodeNameExactWithBoost(sb, query, allLangs);

                    AppendNodeNameWithWildcards(sb, querywords, allLangs);

                    foreach (var f in fields)
                    {
                        var queryWordsReplaced = new string[querywords.Length];

                        // when searching file names containing hyphens we need to replace the hyphens with spaces
                        if (f.Equals(UmbracoExamineIndex.UmbracoFileFieldName))
                        {
                            for (var index = 0; index < querywords.Length; index++)
                            {
                                queryWordsReplaced[index] = querywords[index].Replace("\\-", " ").Replace("_", " ").Trim(" ");
                            }
                        }
                        else
                        {
                            queryWordsReplaced = querywords;
                        }

                        //additional fields normally
                        sb.Append(f);
                        sb.Append(":");
                        sb.Append("(");
                        foreach (var w in queryWordsReplaced)
                        {
                            sb.Append(w.ToLower());
                            sb.Append("* ");
                        }
                        sb.Append(")");
                        sb.Append(" ");
                    }

                    sb.Append(") ");
                }
            }

            //must match index type
            sb.Append("+__IndexType:");
            sb.Append(type);

            return true;
        }

        private void AppendNodeNamePhraseWithBoost(StringBuilder sb, string query, IEnumerable<string> allLangs)
        {
            //node name exactly boost x 10
            sb.Append("nodeName: (");
            sb.Append(query.ToLower());
            sb.Append(")^10.0 ");

            //also search on all variant node names
            foreach (var lang in allLangs)
            {
                //node name exactly boost x 10
                sb.Append($"nodeName_{lang}: (");
                sb.Append(query.ToLower());
                sb.Append(")^10.0 ");
            }
        }

        private void AppendNodeNameExactWithBoost(StringBuilder sb, string query, IEnumerable<string> allLangs)
        {
            //node name exactly boost x 10
            sb.Append("nodeName:");
            sb.Append("\"");
            sb.Append(query.ToLower());
            sb.Append("\"");
            sb.Append("^10.0 ");
            //also search on all variant node names
            foreach (var lang in allLangs)
            {
                //node name exactly boost x 10
                sb.Append($"nodeName_{lang}:");
                sb.Append("\"");
                sb.Append(query.ToLower());
                sb.Append("\"");
                sb.Append("^10.0 ");
            }
        }

        private void AppendNodeNameWithWildcards(StringBuilder sb, string[] querywords, IEnumerable<string> allLangs)
        {
            //node name normally with wildcards
            sb.Append("nodeName:");
            sb.Append("(");
            foreach (var w in querywords)
            {
                sb.Append(w.ToLower());
                sb.Append("* ");
            }
            sb.Append(") ");
            //also search on all variant node names
            foreach (var lang in allLangs)
            {
                //node name normally with wildcards
                sb.Append($"nodeName_{lang}:");
                sb.Append("(");
                foreach (var w in querywords)
                {
                    sb.Append(w.ToLower());
                    sb.Append("* ");
                }
                sb.Append(") ");
            }
        }

        private void AppendPath(StringBuilder sb, UmbracoObjectTypes objectType, int[] startNodeIds, string searchFrom, bool ignoreUserStartNodes, IEntityService entityService)
        {
            if (sb == null) throw new ArgumentNullException(nameof(sb));
            if (entityService == null) throw new ArgumentNullException(nameof(entityService));

            Udi.TryParse(searchFrom, true, out var udi);
            searchFrom = udi == null ? searchFrom : entityService.GetId(udi).Result.ToString();

            var entityPath = int.TryParse(searchFrom, out var searchFromId) && searchFromId > 0
                ? entityService.GetAllPaths(objectType, searchFromId).FirstOrDefault()
                : null;
            if (entityPath != null)
            {
                // find... only what's underneath
                sb.Append("+__Path:");
                AppendPath(sb, entityPath.Path, false);
                sb.Append(" ");
            }
            else if (startNodeIds.Length == 0)
            {
                // make sure we don't find anything
                sb.Append("+__Path:none ");
            }
            else if (startNodeIds.Contains(-1) == false && ignoreUserStartNodes == false) // -1 = no restriction
            {
                var entityPaths = entityService.GetAllPaths(objectType, startNodeIds);

                // for each start node, find the start node, and what's underneath
                // +__Path:(-1*,1234 -1*,1234,* -1*,5678 -1*,5678,* ...)
                sb.Append("+__Path:(");
                var first = true;
                foreach (var ep in entityPaths)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(" ");
                    AppendPath(sb, ep.Path, true);
                }
                sb.Append(") ");
            }
        }

        private void AppendPath(StringBuilder sb, string path, bool includeThisNode)
        {
            path = path.Replace("-", "\\-").Replace(",", "\\,");
            if (includeThisNode)
            {
                sb.Append(path);
                sb.Append(" ");
            }
            sb.Append(path);
            sb.Append("\\,*");
        }

        /// <summary>
        /// Returns a collection of entities for media based on search results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<SearchResultEntity> MemberFromSearchResults(IEnumerable<ISearchResult> results)
        {
            //add additional data
            foreach (var result in results)
            {
                var m = _mapper.Map<SearchResultEntity>(result);

                //if no icon could be mapped, it will be set to document, so change it to picture
                if (m.Icon == Constants.Icons.DefaultIcon)
                {
                    m.Icon = Constants.Icons.Member;
                }

                if (result.Values.ContainsKey("email") && result.Values["email"] != null)
                {
                    m.AdditionalData["Email"] = result.Values["email"];
                }
                if (result.Values.ContainsKey(UmbracoExamineIndex.NodeKeyFieldName) && result.Values[UmbracoExamineIndex.NodeKeyFieldName] != null)
                {
                    if (Guid.TryParse(result.Values[UmbracoExamineIndex.NodeKeyFieldName], out var key))
                    {
                        m.Key = key;
                    }
                }

                yield return m;
            }
        }

        /// <summary>
        /// Returns a collection of entities for media based on search results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<SearchResultEntity> MediaFromSearchResults(IEnumerable<ISearchResult> results)
            => _mapper.Map<IEnumerable<SearchResultEntity>>(results);

        /// <summary>
        /// Returns a collection of entities for content based on search results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private IEnumerable<SearchResultEntity> ContentFromSearchResults(IEnumerable<ISearchResult> results, string culture = null)
        {
            var defaultLang = _languageService.GetDefaultLanguageIsoCode();
            foreach (var result in results)
            {
                var entity = _mapper.Map<SearchResultEntity>(result, context =>
                {
                    if (culture != null)
                    {
                        context.SetCulture(culture);
                    }
                }
                );

                var intId = entity.Id.TryConvertTo<int>();
                if (intId.Success)
                {
                    //if it varies by culture, return the default language URL
                    if (result.Values.TryGetValue(UmbracoContentIndex.VariesByCultureFieldName, out var varies) && varies == "y")
                    {
                        entity.AdditionalData["Url"] = _umbracoContext.Url(intId.Result, culture ?? defaultLang);
                    }
                    else
                    {
                        entity.AdditionalData["Url"] = _umbracoContext.Url(intId.Result);
                    }
                }

                yield return entity;
            }
        }

    }
}
