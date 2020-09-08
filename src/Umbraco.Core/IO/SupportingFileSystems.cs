using System;
using Umbraco.Core.Composing;

namespace Umbraco.Core.IO
{
    public class SupportingFileSystems : TargetedServiceFactory<IFileSystem>
    {
        public SupportingFileSystems(IServiceProvider serviceProvider)
            : base(serviceProvider)
        { }
    }
}
