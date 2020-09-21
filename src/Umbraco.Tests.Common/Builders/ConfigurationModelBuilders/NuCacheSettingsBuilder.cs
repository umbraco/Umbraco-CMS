using Umbraco.Core.Configuration.Models;

namespace Umbraco.Tests.Common.Builders
{
    public class NuCacheSettingsBuilder : BuilderBase<NuCacheSettings>
    {
        private int? _bTreeBlockSize;

        public NuCacheSettingsBuilder WithBTreeBlockSize(int bTreeBlockSize)
        {
            _bTreeBlockSize = bTreeBlockSize;
            return this;
        }

        public override NuCacheSettings Build()
        {
            var bTreeBlockSize = _bTreeBlockSize ?? 4096;

            return new NuCacheSettings
            {
                BTreeBlockSize = bTreeBlockSize
            };
        }
    }
}
