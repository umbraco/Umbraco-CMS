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
        private readonly Dictionary<int, ContentNodeKit> _kits;

        public TestDataSource(params ContentNodeKit[] kits)
            : this((IEnumerable<ContentNodeKit>) kits)
        { }

        public TestDataSource(IEnumerable<ContentNodeKit> kits)
        {
            _kits = kits.ToDictionary(x => x.Node.Id, x => x);
        }

        public ContentNodeKit GetContentSource(IScope scope, int id)
            => _kits.TryGetValue(id, out var kit) ? kit : default;

        public IEnumerable<ContentNodeKit> GetAllContentSources(IScope scope)
            => _kits.Values;

        public IEnumerable<ContentNodeKit> GetBranchContentSources(IScope scope, int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentNodeKit> GetTypeContentSources(IScope scope, IEnumerable<int> ids)
            => _kits.Values.Where(x => ids.Contains(x.ContentTypeId));

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
