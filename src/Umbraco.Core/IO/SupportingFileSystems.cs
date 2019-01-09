using Umbraco.Core.Composing;

namespace Umbraco.Core.IO
{
    public class SupportingFileSystems : TargetedServiceProvider<IFileSystem>
    {
        public SupportingFileSystems(IFactory factory)
            : base(factory)
        { }
    }
}