using LightInject;
using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class PartialViewMacroRepository : PartialViewRepository, IPartialViewMacroRepository
    {

        public PartialViewMacroRepository([Inject("PartialViewMacroFileSystem")] IFileSystem fileSystem)
            : base(fileSystem)
        { }

        protected override PartialViewType ViewType => PartialViewType.PartialViewMacro;
    }
}
