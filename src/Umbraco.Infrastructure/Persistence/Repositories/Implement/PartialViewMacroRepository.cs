using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class PartialViewMacroRepository : PartialViewRepository, IPartialViewMacroRepository
    {
        public PartialViewMacroRepository(IFileSystems fileSystems)
            : base(fileSystems.MacroPartialsFileSystem)
        { }

        protected override PartialViewType ViewType => PartialViewType.PartialViewMacro;
    }
}
