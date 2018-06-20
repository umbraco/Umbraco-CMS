using Umbraco.Core.IO;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    internal class PartialViewMacroRepository : PartialViewRepository, IPartialViewMacroRepository
    {

        public PartialViewMacroRepository(IFileSystem fileSystem)
            : base(fileSystem)
        { }

        protected override PartialViewType ViewType => PartialViewType.PartialViewMacro;
    }
}
