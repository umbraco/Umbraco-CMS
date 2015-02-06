using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using System.Linq;
using umbraco.BusinessLogic;

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
            var mediaType = ApplicationContext.Current.Services.ContentTypeService.GetMediaType(Alias);
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
            var mediaTypes = ApplicationContext.Current.Services.ContentTypeService.GetAllMediaTypes();
            return mediaTypes.OrderBy(x => x.Name).Select(x => new MediaType(x));
        }

        /// <summary>
        /// Create a new Mediatype
        /// </summary>
        /// <param name="u">The Umbraco user context</param>
        /// <param name="Text">The name of the MediaType</param>
        /// <returns>The new MediaType</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Models.MediaType and Umbraco.Core.Services.ContentTypeService.Save()", false)]
        public static MediaType MakeNew(User u, string Text)
        {
            return MakeNew(u, Text, -1);
        }

        internal static MediaType MakeNew(User u, string text, int parentId)
        {
            var mediaType = new Umbraco.Core.Models.MediaType(parentId) { Name = text, Alias = text, CreatorId = u.Id, Thumbnail = "icon-folder", Icon = "icon-folder" };
            ApplicationContext.Current.Services.ContentTypeService.Save(mediaType, u.Id);
            var mt = new MediaType(mediaType.Id);

            NewEventArgs e = new NewEventArgs();
            mt.OnNew(e);

            return mt;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {
                var current = User.GetCurrent();
                int userId = current == null ? 0 : current.Id;
                ApplicationContext.Current.Services.ContentTypeService.Save(MediaTypeItem, userId);

                base.Save();

                FireAfterSave(e);
            }
        }

        /// <summary>
        /// Deletes the current MediaType and all created Medias of the type.
        /// </summary>
        public override void delete()
        {
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel)
            {
                // check that no media types uses me as a master
                if (GetAllAsList().Any(dt => dt.MasterContentTypes.Contains(this.Id)))
                {
                    throw new ArgumentException("Can't delete a Media Type used as a Master Content Type. Please remove all references first!");
                }

                ApplicationContext.Current.Services.ContentTypeService.Delete(MediaTypeItem);

                FireAfterDelete(e);
            }
        }
        #endregion

        #region Protected Methods

        protected override void setupNode()
        {
            var mediaType = ApplicationContext.Current.Services.ContentTypeService.GetMediaType(Id);
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

        #region Events

        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(MediaType sender, SaveEventArgs e);
        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewEventHandler(MediaType sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(MediaType sender, DeleteEventArgs e);


        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        public static new event SaveEventHandler BeforeSave;
        /// <summary>
        /// Raises the <see cref="E:BeforeSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        public static new event SaveEventHandler AfterSave;
        /// <summary>
        /// Raises the <see cref="E:AfterSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireAfterSave(SaveEventArgs e)
        {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        /// <summary>
        /// Occurs when [new].
        /// </summary>
        public static event NewEventHandler New;
        /// <summary>
        /// Raises the <see cref="E:New"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnNew(NewEventArgs e)
        {
            if (New != null)
                New(this, e);
        }

        /// <summary>
        /// Occurs when [before delete].
        /// </summary>
        public static new event DeleteEventHandler BeforeDelete;
        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireBeforeDelete(DeleteEventArgs e)
        {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        /// <summary>
        /// Occurs when [after delete].
        /// </summary>
        public static new event DeleteEventHandler AfterDelete;
        /// <summary>
        /// Raises the <see cref="E:AfterDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }
        #endregion

    }
}
