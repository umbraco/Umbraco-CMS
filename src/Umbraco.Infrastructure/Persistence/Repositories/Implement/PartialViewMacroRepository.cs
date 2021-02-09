using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class PartialViewMacroRepository : PartialViewRepository, IPartialViewMacroRepository
    {
        public PartialViewMacroRepository(IFileSystems fileSystems, IIOHelper ioHelper)
            : base(fileSystems.MacroPartialsFileSystem, ioHelper)
        { }

        protected override PartialViewType ViewType => PartialViewType.PartialViewMacro;
    }
}
