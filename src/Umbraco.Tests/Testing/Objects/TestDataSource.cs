using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Scoping;
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
                .Select(x => x.Clone());

        public IEnumerable<ContentNodeKit> GetBranchContentSources(IScope scope, int id)
            => Kits.Values
                .Where(x => x.Node.Path.EndsWith("," + id) || x.Node.Path.Contains("," + id + ","))
                .OrderBy(x => x.Node.Level).ThenBy(x => x.Node.SortOrder)
                .Select(x => x.Clone());

        public IEnumerable<ContentNodeKit> GetTypeContentSources(IScope scope, IEnumerable<int> ids)
            => Kits.Values
                .Where(x => ids.Contains(x.ContentTypeId))
                .Select(x => x.Clone());

        public ContentNodeKit GetMediaSource(IScope scope, int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentNodeKit> GetAllMediaSources(IScope scope)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentNodeKit> GetBranchMediaSources(IScope scope, int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentNodeKit> GetTypeMediaSources(IScope scope, IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }
    }
}
