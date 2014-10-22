using System.Threading;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    internal class PartialViewMacroRepository : PartialViewRepository, IPartialViewMacroRepository
    {
        private readonly IMacroRepository _macroRepository;

        public PartialViewMacroRepository(IUnitOfWork work, IMacroRepository macroRepository)
            : this(work, new PhysicalFileSystem(SystemDirectories.MvcViews + "/MacroPartials/"), macroRepository)
        {
        }

        public PartialViewMacroRepository(IUnitOfWork work, IFileSystem fileSystem, IMacroRepository macroRepository)
            : base(work, fileSystem)
        {
            _macroRepository = macroRepository;
        }

        /// <summary>
        /// Ensure the macro repo is disposed which contains a database UOW
        /// </summary>
        protected override void DisposeResources()
        {
            base.DisposeResources();
            _macroRepository.Dispose();
        }

        /// <summary>
        /// Adds or updates a macro associated with the partial view
        /// </summary>
        /// <param name="macro"></param>
        public void AddOrUpdate(IMacro macro)
        {
            _macroRepository.AddOrUpdate(macro);
        }
    }
}