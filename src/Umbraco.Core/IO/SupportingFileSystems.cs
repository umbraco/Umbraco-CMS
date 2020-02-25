using Umbraco.Core.Composing;

namespace Umbraco.Core.IO
{
    public class SupportingFileSystems : TargetedServiceFactory<IFileSystem>
    {
        public SupportingFileSystems(IFactory factory)
            : base(factory)
        { }
    }
}
