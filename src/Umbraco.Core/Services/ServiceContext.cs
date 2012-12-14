using System;
using System.Collections.Concurrent;
using Umbraco.Core.Persistence;
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

		/// <summary>
		/// Internal constructor used for unit tests
		/// </summary>
		/// <param name="dbUnitOfWorkProvider"></param>
		/// <param name="fileUnitOfWorkProvider"></param>
		/// <param name="publishingStrategy"></param>
		internal ServiceContext(IDatabaseUnitOfWorkProvider dbUnitOfWorkProvider, IUnitOfWorkProvider fileUnitOfWorkProvider, IPublishingStrategy publishingStrategy)
		{
			BuildServiceCache(dbUnitOfWorkProvider, fileUnitOfWorkProvider, publishingStrategy, RepositoryResolver.Current.Factory);
		}

        /// <summary>
        /// Builds the various services
        /// </summary>
		private void BuildServiceCache(
			IDatabaseUnitOfWorkProvider dbUnitOfWorkProvider, 
			IUnitOfWorkProvider fileUnitOfWorkProvider, 
			IPublishingStrategy publishingStrategy, 
			RepositoryFactory repositoryFactory)
        {
            var provider = dbUnitOfWorkProvider;
            var fileProvider = fileUnitOfWorkProvider;	        

            if(_userService == null)
                _userService = new UserService(provider, repositoryFactory);

            if (_contentService == null)
				_contentService = new ContentService(provider, publishingStrategy, repositoryFactory, _userService);

            if(_mediaService == null)
                _mediaService = new MediaService(provider, repositoryFactory);

            if(_macroService == null)
				_macroService = new MacroService(fileProvider, repositoryFactory);

            if(_contentTypeService == null)
				_contentTypeService = new ContentTypeService(provider, repositoryFactory, _contentService, _mediaService);

            if(_dataTypeService == null)
				_dataTypeService = new DataTypeService(provider, repositoryFactory);

            if(_fileService == null)
				_fileService = new FileService(fileProvider, provider, repositoryFactory);

            if(_localizationService == null)
				_localizationService = new LocalizationService(provider, repositoryFactory);
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