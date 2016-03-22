using System;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using Umbraco.Core.Models.Membership;

namespace umbraco.cms.businesslogic.media
{
	/// <summary>
	/// A media represents a physical file and metadata on the file.
	///  
	/// By inheriting the Content class it has a generic datafields which enables custumization
	/// </summary>
    [Obsolete("Obsolete, Use Umbraco.Core.Models.Media", false)]
    public class Media : Content
	{
        #region Constants and static members

	    protected internal IMedia MediaItem;
       
        #endregion

        #region Constructors

        /// <summary>
        /// Contructs a media object given the Id
        /// </summary>
        /// <param name="id">Identifier</param>
        public Media(int id) : base(id) { }

        /// <summary>
        /// Contructs a media object given the Id
        /// </summary>
        /// <param name="id">Identifier</param>
        public Media(Guid id) : base(id) { }

        public Media(int id, bool noSetup) : base(id, noSetup) { }

        public Media(Guid id, bool noSetup) : base(id, noSetup) { }

        internal Media(IUmbracoEntity entity, bool noSetup = true)
            : base(entity)
        {
            if (noSetup == false)
                setupNode();
        }

        internal Media(IMedia media) : base(media)
        {
            SetupNode(media);
        }
        
        #endregion

        #region Static Methods
        /// <summary>
        /// -
        /// </summary>
        public static Guid _objectType = new Guid(Constants.ObjectTypes.Media);
       

        [Obsolete("Obsolete, Use Umbraco.Core.Services.MediaService.CreateMedia()", false)]
        public static Media MakeNew(string Name, MediaType dct, IUser u, int ParentId)
        {
            var media = ApplicationContext.Current.Services.MediaService.CreateMediaWithIdentity(Name, ParentId, dct.Alias, u.Id);
            //The media object will only have the 'WasCancelled' flag set to 'True' if the 'Creating' event has been cancelled
            if (((Entity)media).WasCancelled)
                return null;

            var tmp = new Media(media);

            return tmp;
        }

        /// <summary>
        /// Retrieve a list of all toplevel medias and folders
        /// </summary>
        /// <returns></returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.MediaService.GetRootMedia()", false)]
        public static Media[] GetRootMedias()
        {
            var children = ApplicationContext.Current.Services.MediaService.GetRootMedia();
            return children.Select(x => new Media(x)).ToArray();
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Services.MediaService.GetChildren()", false)]
        public static List<Media> GetChildrenForTree(int nodeId)
        {
            var children = ApplicationContext.Current.Services.MediaService.GetChildren(nodeId);
            return children.Select(x => new Media(x)).ToList();
        }

        [Obsolete("Obsolete, Use Umbraco.Core.Services.MediaService.GetMediaOfMediaType()", false)]
        public static IEnumerable<Media> GetMediaOfMediaType(int mediaTypeId)
        {
            var children = ApplicationContext.Current.Services.MediaService.GetMediaOfMediaType(mediaTypeId);
            return children.Select(x => new Media(x)).ToList();
        }

        /// <summary>
        /// Deletes all medias of the given type, used when deleting a mediatype
        /// 
        /// Use with care.
        /// </summary>
        /// <param name="dt"></param>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.MediaService.DeleteMediaOfType()", false)]
        public static void DeleteFromType(MediaType dt)
        {
            ApplicationContext.Current.Services.MediaService.DeleteMediaOfType(dt.Id);
        }
        
        #endregion

        #region Public Properties
        public override int sortOrder
        {
            get
            {
                return MediaItem == null ? base.sortOrder : MediaItem.SortOrder;
            }
            set
            {
                if (MediaItem == null)
                {
                    base.sortOrder = value;
                }
                else
                {
                    MediaItem.SortOrder = value;
                }
            }
        }

        public override int Level
        {
            get
            {
                return MediaItem == null ? base.Level : MediaItem.Level;
            }
            set
            {
                if (MediaItem == null)
                {
                    base.Level = value;
                }
                else
                {
                    MediaItem.Level = value;
                }
            }
        }

        public override int ParentId
        {
            get
            {
                return MediaItem == null ? base.ParentId : MediaItem.ParentId;
            }
        }

        public override string Path
        {
            get
            {
                return MediaItem == null ? base.Path : MediaItem.Path;
            }
            set
            {
                if (MediaItem == null)
                {
                    base.Path = value;
                }
                else
                {
                    MediaItem.Path = value;
                }
            }
        }

        [Obsolete("Obsolete, Use Name property on Umbraco.Core.Models.Content", false)]
        public override string Text
        {
            get
            {
                return MediaItem.Name;
            }
            set
            {
                value = value.Trim();
                MediaItem.Name = value;
            }
        }

        /// <summary>
        /// Retrieve a list of all medias underneath the current
        /// </summary>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.MediaService.GetChildren()", false)]
        public new Media[] Children
        {
            get
            {
                //return refactored optimized method
                //return Media.GetChildrenForTree(this.Id).ToArray();

                var children = ApplicationContext.Current.Services.MediaService.GetChildren(Id).OrderBy(c => c.SortOrder);
                return children.Select(x => new Media(x)).ToArray();
            }
        }
        
        #endregion

        #region Public methods

        public override XmlNode ToXml(XmlDocument xd, bool Deep)
        {
            if (IsTrashed == false)
            {
                return base.ToXml(xd, Deep);   
            }
            return null;
        }

      

        /// <summary>
        /// Moves the media to the trash
        /// </summary>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.MediaService.MoveToRecycleBin()", false)]
        public override void delete()
        {
            MoveToTrash();
        }

        /// <summary>
        /// With either move the media to the trash or permanently remove it from the database.
        /// </summary>
        /// <param name="deletePermanently">flag to set whether or not to completely remove it from the database or just send to trash</param>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.MediaService.Delete() or Umbraco.Core.Services.MediaService.MoveToRecycleBin()", false)]
        public void delete(bool deletePermanently)
        {
            if (!deletePermanently)
            {
                MoveToTrash();
            }
            else
            {
                DeletePermanently();
            }
        }

        #endregion

        #region Protected methods
        protected override void setupNode()
        {
            if (Id == -1 || Id == -21)
            {
                base.setupNode();
                return;
            }

            var media = Version == Guid.Empty
                           ? ApplicationContext.Current.Services.MediaService.GetById(Id)
                           : ApplicationContext.Current.Services.MediaService.GetByVersion(Version);

            if (media == null)
                throw new ArgumentException(string.Format("No Media exists with id '{0}'", Id));

            SetupNode(media);
        }
      
        #endregion

        #region Private methods
        private void SetupNode(IMedia media)
        {
            MediaItem = media;
            //Also need to set the ContentBase item to this one so all the propery values load from it
            ContentBase = MediaItem;

            //Setting private properties from IContentBase replacing CMSNode.setupNode() / CMSNode.PopulateCMSNodeFromReader()
            base.PopulateCMSNodeFromUmbracoEntity(MediaItem, _objectType);

            //If the version is empty we update with the latest version from the current IContent.
            if (Version == Guid.Empty)
                Version = MediaItem.Version;
        }

        /// <summary>
        /// Used internally to permanently delete the data from the database
        /// </summary>      
        /// <returns>returns true if deletion isn't cancelled</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.MediaService.Delete()", false)]
        private bool DeletePermanently()
        {
            if (MediaItem != null)
            {
                ApplicationContext.Current.Services.MediaService.Delete(MediaItem);
            }
            else
            {
                var media = ApplicationContext.Current.Services.MediaService.GetById(Id);
                ApplicationContext.Current.Services.MediaService.Delete(media);
            }

            base.delete();
            return true;
        }

        /// <summary>
        /// Used internally to move the node to the recyle bin
        /// </summary>
        /// <returns>Returns true if the move was not cancelled</returns>
        [Obsolete("Obsolete, Use Umbraco.Core.Services.MediaService.MoveToRecycleBin()", false)]
        private bool MoveToTrash()
        {
            if (MediaItem != null)
            {
                ApplicationContext.Current.Services.MediaService.MoveToRecycleBin(MediaItem);
            }
            else
            {
                var media = ApplicationContext.Current.Services.MediaService.GetById(Id);
                ApplicationContext.Current.Services.MediaService.MoveToRecycleBin(media);
            }

            //Move((int)RecycleBin.RecycleBinType.Media);

            //TODO: Now that we've moved it to trash, we need to move the actual files so they are no longer accessible
            //from the original URL.
            return true;

        }
        
        #endregion
		
       
    }
}
