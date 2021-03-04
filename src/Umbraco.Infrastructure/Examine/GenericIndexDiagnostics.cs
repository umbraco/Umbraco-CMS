﻿using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine
{

    /// <summary>
    /// Used to return diagnostic data for any index
    /// </summary>
    public class GenericIndexDiagnostics : IIndexDiagnostics
    {
        private readonly IIndex _index;
        private static readonly string[] IgnoreProperties = { "Description" };

        public GenericIndexDiagnostics(IIndex index)
        {
            _index = index;
        }

        public int DocumentCount => -1; //unknown

        public int FieldCount => -1; //unknown

        public Attempt<string> IsHealthy()
        {
            if (!_index.IndexExists())
                return Attempt.Fail("Does not exist");

            try
            {
                var searcher = _index.GetSearcher();
                var result = searcher.Search("test");
                return Attempt<string>.Succeed(); //if we can search we'll assume it's healthy
            }
            catch (Exception e)
            {
                return Attempt.Fail($"Error: {e.Message}");
            }
        }

        public IReadOnlyDictionary<string, object> Metadata
        {
            get
            {
                var result = new Dictionary<string, object>();

                var props = TypeHelper.CachedDiscoverableProperties(_index.GetType(), mustWrite: false)
                    .Where(x => IgnoreProperties.InvariantContains(x.Name) == false)
                    .OrderBy(x => x.Name);

                foreach (var p in props)
                {
                    var val = p.GetValue(_index, null) ?? string.Empty;

                    result.Add(p.Name, val);
                }

                return result;
            }
        }
    }
}
