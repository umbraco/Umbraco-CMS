using System;
using System.Collections.Concurrent;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// The Umbraco ServiceContext, which provides access to the following services:
    /// <see cref="IContentService"/>, <see cref="IContentTypeService"/>, <see cref="IDataTypeService"/>,
    /// <see cref="IFileService"/>, <see cref="ILocalizationService"/> and <see cref="IMediaService"/>.
    /// </summary>
    public class ServiceContext
    {
        private ContentService _contentService;
        private UserService _userService;
        private MediaService _mediaService;
        private MacroService _macroService;
        private ContentTypeService _contentTypeService;
        private DataTypeService _dataTypeService;
        private FileService _fileService;
        private LocalizationService _localizationService;

        #region Singleton
        private static readonly Lazy<ServiceContext> lazy = new Lazy<ServiceContext>(() => new ServiceContext());

        /// <summary>
        /// Gets the current Database Context.
        /// </summary>
        public static ServiceContext Current { get { return lazy.Value; } }

        private ServiceContext()
        {
            BuildServiceCache();
        }
        #endregion

        /// <summary>
        /// Builds the various services
        /// </summary>
        private void BuildServiceCache()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var fileProvider = new FileUnitOfWorkProvider();
            var publishingStrategy = new PublishingStrategy();

            if(_userService == null)
                _userService = new UserService(provider);

            if (_contentService == null)
                _contentService = new ContentService(provider, publishingStrategy, _userService);

            if(_mediaService == null)
                _mediaService = new MediaService(provider);

            if(_macroService == null)
                _macroService = new MacroService(fileProvider);

            if(_contentTypeService == null)
                _contentTypeService = new ContentTypeService(_contentService, _mediaService, provider);

            if(_dataTypeService == null)
                _dataTypeService = new DataTypeService(provider);

            if(_fileService == null)
                _fileService = new FileService(fileProvider, provider);

            if(_localizationService == null)
                _localizationService = new LocalizationService(provider);
        }

        /// <summary>
        /// Gets the <see cref="IContentService"/>
        /// </summary>
        public IContentService ContentService
        {
            get { return _contentService; }
        }

        /// <summary>
        /// Gets the <see cref="IContentTypeService"/>
        /// </summary>
        public IContentTypeService ContentTypeService
        {
            get { return _contentTypeService; }
        }

        /// <summary>
        /// Gets the <see cref="IDataTypeService"/>
        /// </summary>
        public IDataTypeService DataTypeService
        {
            get { return _dataTypeService; }
        }

        /// <summary>
        /// Gets the <see cref="IFileService"/>
        /// </summary>
        public IFileService FileService
        {
            get { return _fileService; }
        }

        /// <summary>
        /// Gets the <see cref="ILocalizationService"/>
        /// </summary>
        public ILocalizationService LocalizationService
        {
            get { return _localizationService; }
        }

        /// <summary>
        /// Gets the <see cref="IMediaService"/>
        /// </summary>
        public IMediaService MediaService
        {
            get { return _mediaService; }
        }

        /// <summary>
        /// Gets the <see cref="IMacroService"/>
        /// </summary>
        internal IMacroService MacroService
        {
            get { return _macroService; }
        }

        /// <summary>
        /// Gets the <see cref="IMacroService"/>
        /// </summary>
        internal IUserService UserService
        {
            get { return _userService; }
        }
    }
}