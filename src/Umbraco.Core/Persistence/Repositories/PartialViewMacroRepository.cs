using System.Threading;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class PartialViewMacroRepository : PartialViewRepository 
    {

        public PartialViewMacroRepository(IUnitOfWork work)
            : this(work, new PhysicalFileSystem(SystemDirectories.MvcViews + "/MacroPartials/"))
        {
        }

        public PartialViewMacroRepository(IUnitOfWork work, IFileSystem fileSystem)
            : base(work, fileSystem)
        {
        }

    }
}