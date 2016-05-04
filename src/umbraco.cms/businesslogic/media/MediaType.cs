using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using System.Linq;
using System.Threading;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;

namespace umbraco.cms.businesslogic.media
{
    /// <summary>
    /// The Mediatype
    /// 
    /// Due to the inheritance of the ContentType class,it enables definition of generic datafields on a Media.
    /// </summary>
    [Obsolete("Obsolete, Use Umbraco.Core.Models.MediaType", false)]
    public class MediaType : ContentType
    {
        #region Constructors

        /// <summary>
        /// Constructs a MediaTypeobject given the id
        /// </summary>
        /// <param name="id">Id of the mediatype</param>
        public MediaType(int id) : base(id) { }

        /// <summary>
        /// Constructs a MediaTypeobject given the id
        /// </summary>
        /// <param name="id">Id of the mediatype</param>
        public MediaType(Guid id) : base(id) { }

        public MediaType(int id, bool noSetup) : base(id, noSetup) { }

        internal MediaType(IMediaType mediaType)
            : base(mediaType)
        {
            SetupNode(mediaType);
        }

        #endregion

        #region Constants and static members

        public static Guid _objectType = new Guid(Constants.ObjectTypes.MediaType);

        #endregion

        #region Private Members

        private IMediaType MediaTypeItem
        {
            get { return base.ContentTypeItem as IMediaType; }
            set { base.ContentTypeItem = value; }
        }

        #endregion

        #region Static Methods
        /// <summary>
        /// Retrieve a MediaType by it's alias
        /// </summary>
        /// <param name="Alias">The alias of the MediaType</param>
        /// <returns>The MediaType with the alias</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentTypeService.GetMediaType()", false)]
        public static new MediaType GetByAlias(string Alias)
        {
            var mediaType = ApplicationContext.Current.Services.MediaTypeService.Get(Alias);
            return new MediaType(mediaType);
        }

        /// <summary>
        /// Retrieve all MediaTypes in the umbraco installation
        /// </summary>
        [Obsolete("Use the GetAllAsList() method instead")]
        public new static MediaType[] GetAll
        {
            get
            {
                return GetAllAsList().ToArray();
            }
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Services.ContentTypeService.GetMediaType()", false)]
        public static IEnumerable<MediaType> GetAllAsList()
        {
            var mediaTypes = ApplicationContext.Current.Services.MediaTypeService.GetAll();
            return mediaTypes.OrderBy(x => x.Name).Select(x => new MediaType(x));
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Models.MediaType and Umbraco.Core.Services.ContentTypeService.Save()", false)]
        public static MediaType MakeNew(IUser u, string Text)
        {
            return MakeNew(u, Text, -1);
        }

        internal static MediaType MakeNew(IUser u, string text, int parentId)
        {
            var mediaType = new Umbraco.Core.Models.MediaType(parentId) { Name = text, Alias = text, CreatorId = u.Id, Thumbnail = "icon-folder", Icon = "icon-folder" };
            ApplicationContext.Current.Services.MediaTypeService.Save(mediaType, u.Id);
            var mt = new MediaType(mediaType.Id);


            return mt;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            var current = Thread.CurrentPrincipal != null ? Thread.CurrentPrincipal.Identity as UmbracoBackOfficeIdentity : null;
            var userId = current == null ? Attempt<int>.Fail() : current.Id.TryConvertTo<int>();
            ApplicationContext.Current.Services.MediaTypeService.Save(MediaTypeItem, userId.Success ? userId.Result : 0);

            base.Save();

            
        }

        /// <summary>
        /// Deletes the current MediaType and all created Medias of the type.
        /// </summary>
        public override void delete()
        {
            // check that no media types uses me as a master
            if (GetAllAsList().Any(dt => dt.MasterContentTypes.Contains(this.Id)))
            {
                throw new ArgumentException("Can't delete a Media Type used as a Master Content Type. Please remove all references first!");
            }

            ApplicationContext.Current.Services.MediaTypeService.Delete(MediaTypeItem);

            
        }
        #endregion

        #region Protected Methods

        protected override void setupNode()
        {
            var mediaType = ApplicationContext.Current.Services.MediaTypeService.Get(Id);
            // If it's null, it's probably a folder
            if (mediaType != null)
                SetupNode(mediaType);
        }

        #endregion

        #region Private Methods
        private void SetupNode(IMediaType mediaType)
        {
            MediaTypeItem = mediaType;

            base.PopulateContentTypeFromContentTypeBase(MediaTypeItem);
            base.PopulateCMSNodeFromUmbracoEntity(MediaTypeItem, _objectType);
        }
        #endregion


    }
}
