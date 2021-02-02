using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Scoping;
using Umbraco.Web;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.PublishedCache.NuCache.DataSource;

namespace Umbraco.Tests.Testing.Objects
{

    internal class TestDataSource : IDataSource
    {
        public TestDataSource(params ContentNodeKit[] kits)
            : this((IEnumerable<ContentNodeKit>) kits)
        { }

        public TestDataSource(IEnumerable<ContentNodeKit> kits)
        {
            Kits = kits.ToDictionary(x => x.Node.Id, x => x);
        }

        public Dictionary<int, ContentNodeKit> Kits { get; }

        // note: it is important to clone the returned kits, as the inner
        // ContentNode is directly reused and modified by the snapshot service

        public ContentNodeKit GetContentSource(IScope scope, int id)
            => Kits.TryGetValue(id, out var kit) ? kit.Clone() : default;

        public IEnumerable<ContentNodeKit> GetAllContentSources(IScope scope)
            => Kits.Values
                .OrderBy(x => x.Node.Level)
                .ThenBy(x => x.Node.ParentContentId)
                .ThenBy(x => x.Node.SortOrder)
                .Select(x => x.Clone());

        public IEnumerable<ContentNodeKit> GetBranchContentSources(IScope scope, int id)
            => Kits.Values
                .Where(x => x.Node.Path.EndsWith("," + id) || x.Node.Path.Contains("," + id + ","))
                .OrderBy(x => x.Node.Level)
                .ThenBy(x => x.Node.ParentContentId)
                .ThenBy(x => x.Node.SortOrder)
                .Select(x => x.Clone());

        public IEnumerable<ContentNodeKit> GetTypeContentSources(IScope scope, IEnumerable<int> ids)
            => Kits.Values
                .Where(x => ids.Contains(x.ContentTypeId))
                .OrderBy(x => x.Node.Level)
                .ThenBy(x => x.Node.ParentContentId)
                .ThenBy(x => x.Node.SortOrder)
                .Select(x => x.Clone());

        public ContentNodeKit GetMediaSource(IScope scope, int id)
        {
            return default;
        }

        public IEnumerable<ContentNodeKit> GetAllMediaSources(IScope scope)
        {
            return Enumerable.Empty<ContentNodeKit>();
        }

        public IEnumerable<ContentNodeKit> GetBranchMediaSources(IScope scope, int id)
        {
            return Enumerable.Empty<ContentNodeKit>();
        }

        public IEnumerable<ContentNodeKit> GetTypeMediaSources(IScope scope, IEnumerable<int> ids)
        {
            return Enumerable.Empty<ContentNodeKit>();
        }

        public void RemoveEntity(IScope scope, int id)
        {
            throw new NotImplementedException();
        }

        public void RemovePublishedEntity(IScope scope, int id)
        {
            throw new NotImplementedException();
        }

        public void DeleteAllContentEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteAllMediaEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {
            throw new NotImplementedException();
        }

        public void DeleteAllMemberEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {
            throw new NotImplementedException();
        }

        public void LoadAllContentEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {
            throw new NotImplementedException();
        }

        public void LoadAllMediaEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {
            throw new NotImplementedException();
        }

        public void LoadAllMemberEntities(IScope scope, IEnumerable<int> contentTypeIds = null)
        {
            throw new NotImplementedException();
        }

        public bool MemberEntitiesValid(IScope scope)
        {
            throw new NotImplementedException();
        }

        public bool ContentEntitiesValid(IScope scope)
        {
            throw new NotImplementedException();
        }

        public bool MediaEntitiesValid(IScope scope)
        {
            throw new NotImplementedException();
        }

        public void UpsertContentEntity(IScope scope, IContentBase contentBase, bool published)
        {
            throw new NotImplementedException();
        }

        public void UpsertMediaEntity(IScope scope, IContentBase contentBase, bool published)
        {
            throw new NotImplementedException();
        }

        public void UpsertMemberEntity(IScope scope, IContentBase contentBase, bool published)
        {
            throw new NotImplementedException();
        }

        public bool MemberEntitiesValid()
        {
            throw new NotImplementedException();
        }

        public bool ContentEntitiesValid()
        {
            throw new NotImplementedException();
        }

        public bool MediaEntitiesValid()
        {
            throw new NotImplementedException();
        }

        public void RebuildMediaDbCache(IEnumerable<int> contentTypeIds = null)
        {
            throw new NotImplementedException();
        }

        public void RebuildContentDbCache(IEnumerable<int> contentTypeIds = null)
        {
            throw new NotImplementedException();
        }

        public void RebuildMemberDbCache(IEnumerable<int> contentTypeIds = null)
        {
            throw new NotImplementedException();
        }
    }
}
