// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Examine.LuceneEngine.Directories;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Examine
{
    public sealed class ExamineLuceneComponent : IComponent
    {
        private readonly IndexRebuilder _indexRebuilder;
        private readonly IExamineManager _examineManager;
        private readonly IMainDom _mainDom;
        private readonly ILoggerFactory _loggerFactory;

        public ExamineLuceneComponent(IndexRebuilder indexRebuilder, IExamineManager examineManager, IMainDom mainDom, ILoggerFactory loggerFactory)
        {
            _indexRebuilder = indexRebuilder;
            _examineManager = examineManager;
            _mainDom = mainDom;
            _loggerFactory = loggerFactory;
        }

        public void Initialize()
        {
            //we want to tell examine to use a different fs lock instead of the default NativeFSFileLock which could cause problems if the AppDomain
            //terminates and in some rare cases would only allow unlocking of the file if IIS is forcefully terminated. Instead we'll rely on the simplefslock
            //which simply checks the existence of the lock file
            DirectoryFactory.DefaultLockFactory = d =>
            {
                var simpleFsLockFactory = new NoPrefixSimpleFsLockFactory(d);
                return simpleFsLockFactory;
            };

            _indexRebuilder.RebuildingIndexes += IndexRebuilder_RebuildingIndexes;
        }

        /// <summary>
        /// Handles event to ensure that all lucene based indexes are properly configured before rebuilding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IndexRebuilder_RebuildingIndexes(object sender, IndexRebuildingEventArgs e) => _examineManager.ConfigureIndexes(_mainDom, _loggerFactory.CreateLogger<IExamineManager>());

        public void Terminate()
        {
        }
    }
}
