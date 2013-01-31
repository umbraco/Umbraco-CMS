using System;
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
        private Lazy<ContentService> _contentService;
        private Lazy<UserService> _userService;
        private Lazy<MediaService> _mediaService;
        private Lazy<MacroService> _macroService;
        private Lazy<ContentTypeService> _contentTypeService;
        private Lazy<DataTypeService> _dataTypeService;
        private Lazy<FileService> _fileService;
        private Lazy<LocalizationService> _localizationService;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="dbUnitOfWorkProvider"></param>
		/// <param name="fileUnitOfWorkProvider"></param>
		/// <param name="publishingStrategy"></param>
		internal ServiceContext(IDatabaseUnitOfWorkProvider dbUnitOfWorkProvider, IUnitOfWorkProvider fileUnitOfWorkProvider, IPublishingStrategy publishingStrategy)
		{
			BuildServiceCache(dbUnitOfWorkProvider, fileUnitOfWorkProvider, publishingStrategy, 
				//this needs to be lazy because when we create the service context it's generally before the
				//resolvers have been initialized!
				new Lazy<RepositoryFactory>(() => RepositoryResolver.Current.Factory));
		}

        /// <summary>
        /// Builds the various services
        /// </summary>
		private void BuildServiceCache(
			IDatabaseUnitOfWorkProvider dbUnitOfWorkProvider, 
			IUnitOfWorkProvider fileUnitOfWorkProvider, 
			IPublishingStrategy publishingStrategy, 
			Lazy<RepositoryFactory> repositoryFactory)
        {
            var provider = dbUnitOfWorkProvider;
            var fileProvider = fileUnitOfWorkProvider;

			if (_userService == null)
				_userService = new Lazy<UserService>(() => new UserService(provider, repositoryFactory.Value));

            if (_contentService == null)
				_contentService = new Lazy<ContentService>(() => new ContentService(provider, repositoryFactory.Value, publishingStrategy));

            if(_mediaService == null)
                _mediaService = new Lazy<MediaService>(() => new MediaService(provider, repositoryFactory.Value));

            if(_macroService == null)
				_macroService = new Lazy<MacroService>(() => new MacroService(fileProvider, repositoryFactory.Value));

            if(_contentTypeService == null)
				_contentTypeService = new Lazy<ContentTypeService>(() => new ContentTypeService(provider, repositoryFactory.Value, _contentService.Value, _mediaService.Value));

            if(_dataTypeService == null)
				_dataTypeService = new Lazy<DataTypeService>(() => new DataTypeService(provider, repositoryFactory.Value));

            if(_fileService == null)
				_fileService = new Lazy<FileService>(() => new FileService(fileProvider, provider, repositoryFactory.Value));

            if(_localizationService == null)
				_localizationService = new Lazy<LocalizationService>(() => new LocalizationService(provider, repositoryFactory.Value));
        }

        /// <summary>
        /// Gets the <see cref="IContentService"/>
        /// </summary>
        public IContentService ContentService
        {
            get { return _contentService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IContentTypeService"/>
        /// </summary>
        public IContentTypeService ContentTypeService
        {
			get { return _contentTypeService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IDataTypeService"/>
        /// </summary>
        public IDataTypeService DataTypeService
        {
			get { return _dataTypeService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IFileService"/>
        /// </summary>
        public IFileService FileService
        {
			get { return _fileService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="ILocalizationService"/>
        /// </summary>
        public ILocalizationService LocalizationService
        {
			get { return _localizationService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IMediaService"/>
        /// </summary>
        public IMediaService MediaService
        {
			get { return _mediaService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IMacroService"/>
        /// </summary>
        internal IMacroService MacroService
        {
			get { return _macroService.Value; }
        }

        /// <summary>
        /// Gets the <see cref="IMacroService"/>
        /// </summary>
        internal IUserService UserService
        {
			get { return _userService.Value; }
        }
    }
}