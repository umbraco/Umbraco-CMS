using Umbraco.Core.Services;

namespace Umbraco.Web.Services
{
    /// <summary>
    /// Represents the ServiceFactory, which provides access to the various services in
    /// a non-singleton way.
    /// </summary>
    public static class ServiceFactory
    {
        private static readonly ServiceContext ServiceContext = new ServiceContext();

        /// <summary>
        /// Gets the <see cref="IContentService"/>
        /// </summary>
        public static IContentService ContentService
        {
            get { return ServiceContext.ContentService; }
        }

        /// <summary>
        /// Gets the <see cref="IContentTypeService"/>
        /// </summary>
        public static IContentTypeService ContentTypeService
        {
            get { return ServiceContext.ContentTypeService; }
        }

        /// <summary>
        /// Gets the <see cref="IDataTypeService"/>
        /// </summary>
        public static IDataTypeService DataTypeService
        {
            get { return ServiceContext.DataTypeService; }
        }

        /// <summary>
        /// Gets the <see cref="IFileService"/>
        /// </summary>
        public static IFileService FileService
        {
            get { return ServiceContext.FileService; }
        }

        /// <summary>
        /// Gets the <see cref="ILocalizationService"/>
        /// </summary>
        public static ILocalizationService LocalizationService
        {
            get { return ServiceContext.LocalizationService; }
        }

        /// <summary>
        /// Gets the <see cref="IMediaService"/>
        /// </summary>
        public static IMediaService MediaService
        {
            get { return ServiceContext.MediaService; }
        }

        /// <summary>
        /// Gets the <see cref="IMacroService"/>
        /// </summary>
        public static IMacroService MacroService
        {
            get { return ServiceContext.MacroService; }
        }
    }
}