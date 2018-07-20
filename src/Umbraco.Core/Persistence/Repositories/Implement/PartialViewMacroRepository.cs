using Umbraco.Core.IO;
using Umbraco.Core.Models;

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
