using System;
using System.Collections.Concurrent;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Web.Publishing;

namespace Umbraco.Web.Services
{
    /// <summary>
    /// The Umbraco ServiceContext, which provides access to the following services:
    /// <see cref="IContentService"/>, <see cref="IContentTypeService"/>, <see cref="IDataTypeService"/>,
    /// <see cref="IFileService"/>, <see cref="ILocalizationService"/> and <see cref="IMediaService"/>.
    /// </summary>
    public class ServiceContext
    {
        #region Singleton
        private static readonly Lazy<ServiceContext> lazy = new Lazy<ServiceContext>(() => new ServiceContext());
        
        public static ServiceContext Current { get { return lazy.Value; } }

        private ServiceContext()
        {
            if(_cache.IsEmpty)
            {
                BuildServiceCache();
            }
        }
        #endregion

        private readonly ConcurrentDictionary<string, IService> _cache = new ConcurrentDictionary<string, IService>();

        /// <summary>
        /// Builds the various services and adds them to the internal cache
        /// </summary>
        private void BuildServiceCache()
        {
            var provider = new PetaPocoUnitOfWorkProvider();
            var publishingStrategy = new PublishingStrategy();

            var contentService = new ContentService(provider, publishingStrategy);
            _cache.AddOrUpdate(typeof (IContentService).Name, contentService, (x, y) => contentService);

            var mediaService = new MediaService(provider);
            _cache.AddOrUpdate(typeof(IMediaService).Name, mediaService, (x, y) => mediaService);

            var contentTypeService = new ContentTypeService(contentService, mediaService, provider);
            _cache.AddOrUpdate(typeof(IContentTypeService).Name, contentTypeService, (x, y) => contentTypeService);

            var dataTypeService = new DataTypeService(provider);
            _cache.AddOrUpdate(typeof(IDataTypeService).Name, dataTypeService, (x, y) => dataTypeService);

            var fileService = new FileService(provider);
            _cache.AddOrUpdate(typeof(IFileService).Name, fileService, (x, y) => fileService);
            
            var localizationService = new LocalizationService(provider);
            _cache.AddOrUpdate(typeof(ILocalizationService).Name, localizationService, (x, y) => localizationService);
        }

        /// <summary>
        /// Gets the <see cref="IContentService"/>
        /// </summary>
        public IContentService ContentService
        {
            get { return _cache[typeof (IContentService).Name] as IContentService; }
        }

        /// <summary>
        /// Gets the <see cref="IContentTypeService"/>
        /// </summary>
        public IContentTypeService ContentTypeService
        {
            get { return _cache[typeof(IContentTypeService).Name] as IContentTypeService; }
        }

        /// <summary>
        /// Gets the <see cref="IDataTypeService"/>
        /// </summary>
        public IDataTypeService DataTypeService
        {
            get { return _cache[typeof(IDataTypeService).Name] as IDataTypeService; }
        }

        /// <summary>
        /// Gets the <see cref="IFileService"/>
        /// </summary>
        public IFileService FileService
        {
            get { return _cache[typeof (IFileService).Name] as IFileService; }
        }

        /// <summary>
        /// Gets the <see cref="ILocalizationService"/>
        /// </summary>
        public ILocalizationService LocalizationService
        {
            get { return _cache[typeof(ILocalizationService).Name] as ILocalizationService; }
        }

        /// <summary>
        /// Gets the <see cref="IMediaService"/>
        /// </summary>
        public IMediaService MediaService
        {
            get { return _cache[typeof(IMediaService).Name] as IMediaService; }
        }
    }
}