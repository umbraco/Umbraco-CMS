using System;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
using umbraco.DataLayer;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

namespace umbraco.cms.businesslogic.media
{
	/// <summary>
	/// A media represents a physical file and metadata on the file.
	///  
	/// By inheriting the Content class it has a generic datafields which enables custumization
	/// </summary>
    [Obsolete("Deprecated, Use Umbraco.Core.Models.Media", false)]
    public class Media : Content
	{
        #region Constants and static members

	    private IMedia _media;
        private const string m_SQLOptimizedMany = @"
			select 
				count(children.id) as children, cmsContentType.isContainer, umbracoNode.id, umbracoNode.uniqueId, umbracoNode.level, umbracoNode.parentId, umbracoNode.path, umbracoNode.sortOrder, umbracoNode.createDate, umbracoNode.nodeUser, umbracoNode.text, 
				cmsContentType.icon, cmsContentType.alias, cmsContentType.thumbnail, cmsContentType.description, cmsContentType.nodeId as contentTypeId
			from umbracoNode 
			left join umbracoNode children on children.parentId = umbracoNode.id
			inner join cmsContent on cmsContent.nodeId = umbracoNode.id
			inner join cmsContentType on cmsContentType.nodeId = cmsContent.contentType
			where umbracoNode.nodeObjectType = @nodeObjectType AND {0}
			group by cmsContentType.isContainer, umbracoNode.id, umbracoNode.uniqueId, umbracoNode.level, umbracoNode.parentId, umbracoNode.path, umbracoNode.sortOrder, umbracoNode.createDate, umbracoNode.nodeUser, umbracoNode.text, 
				cmsContentType.icon, cmsContentType.alias, cmsContentType.thumbnail, cmsContentType.description, cmsContentType.nodeId
			order by {1}"; 
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

        internal Media(IMedia media) : base(media)
        {
            SetupNode(media);
        }
        
        #endregion

        #region Static Methods
        /// <summary>
        /// -
        /// </summary>
        public static Guid _objectType = new Guid("b796f64c-1f99-4ffb-b886-4bf4bc011a9c");

        /// <summary>
        /// Creates a new Media
        /// </summary>
        /// <param name="Name">The name of the media</param>
        /// <param name="dct">The type of the media</param>
        /// <param name="u">The user creating the media</param>
        /// <param name="ParentId">The id of the folder under which the media is created</param>
        /// <returns></returns>
        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.CreateMedia()", false)]
        public static Media MakeNew(string Name, MediaType dct, BusinessLogic.User u, int ParentId)
        {
            var media = ApplicationContext.Current.Services.MediaService.CreateMedia(ParentId, dct.Alias, u.Id);
            media.Name = Name;
            ApplicationContext.Current.Services.MediaService.Save(media);
            var tmp = new Media(media);

            /*Guid newId = Guid.NewGuid();
            // Updated to match level from base node
            CMSNode n = new CMSNode(ParentId);
            int newLevel = n.Level;
            newLevel++;
            CMSNode.MakeNew(ParentId, _objectType, u.Id, newLevel, Name, newId);
            Media tmp = new Media(newId);
            tmp.CreateContent(dct);*/

            NewEventArgs e = new NewEventArgs();
            tmp.OnNew(e);

            return tmp;
        }

        /// <summary>
        /// Retrieve a list of all toplevel medias and folders
        /// </summary>
        /// <returns></returns>
        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.GetRootMedia()", false)]
        public static Media[] GetRootMedias()
        {
            var children = ApplicationContext.Current.Services.MediaService.GetRootMedia();
            return children.Select(x => new Media(x)).ToArray();

            /*Guid[] topNodeIds = CMSNode.TopMostNodeIds(_objectType);

            Media[] retval = new Media[topNodeIds.Length];
            for (int i = 0; i < topNodeIds.Length; i++)
            {
                Media d = new Media(topNodeIds[i]);
                retval[i] = d;
            }
            return retval;*/
        }

        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.GetChildren()", false)]
        public static List<Media> GetChildrenForTree(int nodeId)
        {
            var children = ApplicationContext.Current.Services.MediaService.GetChildren(nodeId);
            return children.Select(x => new Media(x)).ToList();

            /*List<Media> tmp = new List<Media>();
            using (IRecordsReader dr =
                SqlHelper.ExecuteReader(
                    string.Format(m_SQLOptimizedMany.Trim()
                        , "umbracoNode.parentID = @parentId"
                        , "umbracoNode.sortOrder")
                    , SqlHelper.CreateParameter("@nodeObjectType", _objectType)
                    , SqlHelper.CreateParameter("@parentId", nodeId)))
            {

                while (dr.Read())
                {
                    Media d = new Media(dr.GetInt("id"), true);
                    d.PopulateMediaFromReader(dr);                    
                    tmp.Add(d);
                }

            }
            return tmp;*/
        }

        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.GetMediaOfMediaType()", false)]
        public static IEnumerable<Media> GetMediaOfMediaType(int mediaTypeId)
        {
            var children = ApplicationContext.Current.Services.MediaService.GetMediaOfMediaType(mediaTypeId);
            return children.Select(x => new Media(x)).ToList();

            /*var tmp = new List<Media>();
            using (IRecordsReader dr =
                SqlHelper.ExecuteReader(
                                        string.Format(m_SQLOptimizedMany.Trim(), "cmsContent.contentType = @contentTypeId", "umbracoNode.sortOrder"),
                                        SqlHelper.CreateParameter("@nodeObjectType", _objectType),
                                        SqlHelper.CreateParameter("@contentTypeId", mediaTypeId)))
            {
                while (dr.Read())
                {
                    Media d = new Media(dr.GetInt("id"), true);
                    d.PopulateMediaFromReader(dr);
                    tmp.Add(d);
                }
            }

            return tmp.ToArray();*/
        }

        /// <summary>
        /// Deletes all medias of the given type, used when deleting a mediatype
        /// 
        /// Use with care.
        /// </summary>
        /// <param name="dt"></param>
        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.DeleteMediaOfType()", false)]
        public static void DeleteFromType(MediaType dt)
        {
            ApplicationContext.Current.Services.MediaService.DeleteMediaOfType(dt.Id);

            //get all document for the document type and order by level (top level first)
            /*var medias = Media.GetMediaOfMediaType(dt.Id)
                .OrderByDescending(x => x.Level);

            foreach (Media media in medias)
            {
                //before we delete this document, we need to make sure we don't end up deleting other documents that 
                //are not of this document type that are children. So we'll move all of it's children to the trash first.
                foreach (Media m in media.GetDescendants())
                {
                    if (m.ContentType.Id != dt.Id)
                    {
                        m.MoveToTrash();
                    }
                }

                media.DeletePermanently();
            }*/
        }
        
        #endregion

        #region Public Properties
        /// <summary>
        /// Retrieve a list of all medias underneath the current
        /// </summary>
        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.GetChildren()", false)]
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

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.Save()", false)]
        public override void Save()
        {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {
                foreach (var property in GenericProperties)
                {
                    _media.SetValue(property.PropertyType.Alias, property.Value);
                }

                ApplicationContext.Current.Services.MediaService.Save(_media);

                base.Save();

                XmlDocument xd = new XmlDocument();
                XmlGenerate(xd);

                // generate preview for blame history?
                if (UmbracoSettings.EnableGlobalPreviewStorage)
                {
                    // Version as new guid to ensure different versions are generated as media are not versioned currently!
                    SavePreviewXml(generateXmlWithoutSaving(xd), Guid.NewGuid());
                }

                FireAfterSave(e);
            }
        }

        /// <summary>
        /// Moves the media to the trash
        /// </summary>
        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.MoveToRecycleBin()", false)]
        public override void delete()
        {
            MoveToTrash();
        }

        /// <summary>
        /// With either move the media to the trash or permanently remove it from the database.
        /// </summary>
        /// <param name="deletePermanently">flag to set whether or not to completely remove it from the database or just send to trash</param>
        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.Delete() or Umbraco.Core.Services.MediaService.MoveToRecycleBin()", false)]
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

        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.GetDescendants()", false)]
        public override IEnumerable GetDescendants()
        {
            var descendants = ApplicationContext.Current.Services.MediaService.GetDescendants(Id);
            return descendants.Select(x => new Media(x));

            /*var tmp = new List<Media>();
            using (IRecordsReader dr = SqlHelper.ExecuteReader(
                                        string.Format(m_SQLOptimizedMany.Trim(), "umbracoNode.path LIKE '%," + this.Id + ",%'", "umbracoNode.level"),
                                            SqlHelper.CreateParameter("@nodeObjectType", Media._objectType)))
            {
                while (dr.Read())
                {
                    Media d = new Media(dr.GetInt("id"), true);
                    d.PopulateMediaFromReader(dr);
                    tmp.Add(d);
                }
            }

            return tmp.ToArray();*/
        }

        #endregion

        #region Protected methods
        protected override void setupNode()
        {
            if (Id == -1)
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
        
        [Obsolete("Deprecated, This method is no longer used")]
        protected void PopulateMediaFromReader(IRecordsReader dr)
        {
            var hc = dr.GetInt("children") > 0;

            SetupMediaForTree(dr.GetGuid("uniqueId")
                , dr.GetShort("level")
                , dr.GetInt("parentId")
                , dr.GetInt("nodeUser")
                , dr.GetString("path")
                , dr.GetString("text")
                , dr.GetDateTime("createDate")
                , dr.GetString("icon")
                , hc
                , dr.GetString("alias")
                , dr.GetString("thumbnail")
                , dr.GetString("description")
                , null
                , dr.GetInt("contentTypeId")
                , dr.GetBoolean("isContainer"));
        } 
        #endregion

        #region Private methods
        private void SetupNode(IMedia media)
        {
            _media = media;

            //Setting private properties from IContentBase replacing CMSNode.setupNode() / CMSNode.PopulateCMSNodeFromReader()
            base.PopulateCMSNodeFromContentBase(_media, _objectType);

            //If the version is empty we update with the latest version from the current IContent.
            if (Version == Guid.Empty)
                Version = _media.Version;
        }

        [Obsolete("Deprecated, This method is no longer needed", false)]
        private void SetupMediaForTree(Guid uniqueId, int level, int parentId, int user, string path,
                                          string text, DateTime createDate, string icon, bool hasChildren, string contentTypeAlias, string contentTypeThumb,
                                            string contentTypeDesc, int? masterContentType, int contentTypeId, bool isContainer)
        {
            SetupNodeForTree(uniqueId, _objectType, level, parentId, user, path, text, createDate, hasChildren);
            ContentType = new ContentType(contentTypeId, contentTypeAlias, icon, contentTypeThumb, masterContentType, isContainer);
            ContentTypeIcon = icon;
        }

        /// <summary>
        /// Used internally to permanently delete the data from the database
        /// </summary>      
        /// <returns>returns true if deletion isn't cancelled</returns>
        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.Delete()", false)]
        private bool DeletePermanently()
        {
            DeleteEventArgs e = new DeleteEventArgs();

            FireBeforeDelete(e);

            if (!e.Cancel)
            {
                /*foreach (Media m in Children.ToList())
                {
                    m.DeletePermanently();
                } */              

                // Remove all files
                //DeleteAssociatedMediaFiles();

                if (_media != null)
                {
                    ApplicationContext.Current.Services.MediaService.Delete(_media);
                }
                else
                {
                    var media = ApplicationContext.Current.Services.MediaService.GetById(Id);
                    ApplicationContext.Current.Services.MediaService.Delete(media);
                }

                base.delete();

                FireAfterDelete(e);
            }
            return !e.Cancel;
        }

        /// <summary>
        /// Used internally to move the node to the recyle bin
        /// </summary>
        /// <returns>Returns true if the move was not cancelled</returns>
        [Obsolete("Deprecated, Use Umbraco.Core.Services.MediaService.MoveToRecycleBin()", false)]
        private bool MoveToTrash()
        {
            MoveToTrashEventArgs e = new MoveToTrashEventArgs();
            FireBeforeMoveToTrash(e);

            if (!e.Cancel)
            {
                if (_media != null)
                {
                    ApplicationContext.Current.Services.MediaService.MoveToRecycleBin(_media);
                }
                else
                {
                    var media = ApplicationContext.Current.Services.MediaService.GetById(Id);
                    ApplicationContext.Current.Services.MediaService.MoveToRecycleBin(media);
                }

                //Move((int)RecycleBin.RecycleBinType.Media);

                //TODO: Now that we've moved it to trash, we need to move the actual files so they are no longer accessible
                //from the original URL.

                FireAfterMoveToTrash(e);
            }
            return !e.Cancel;           
            
        }
        
        #endregion
		
        #region Events

        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(Media sender, SaveEventArgs e);
        /// <summary>
        /// The new  event handler
        /// </summary>
        public delegate void NewEventHandler(Media sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(Media sender, DeleteEventArgs e);


        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        public new static event SaveEventHandler BeforeSave;
        /// <summary>
        /// Raises the <see cref="E:BeforeSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected new virtual void FireBeforeSave(SaveEventArgs e)
        {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        public new static event SaveEventHandler AfterSave;
        /// <summary>
        /// Raises the <see cref="E:AfterSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected new virtual void FireAfterSave(SaveEventArgs e)
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
        public new static event DeleteEventHandler BeforeDelete;
        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected new virtual void FireBeforeDelete(DeleteEventArgs e)
        {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        /// <summary>
        /// Occurs when [after delete].
        /// </summary>
        public new static event DeleteEventHandler AfterDelete;
        /// <summary>
        /// Raises the <see cref="E:AfterDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected new virtual void FireAfterDelete(DeleteEventArgs e)
        {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }

        /// <summary>
        /// The Move to trash event handler
        /// </summary>
        public delegate void MoveToTrashEventHandler(Media sender, MoveToTrashEventArgs e);
        /// <summary>
        /// Occurs when [before delete].
        /// </summary>
        public static event MoveToTrashEventHandler BeforeMoveToTrash;
        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void FireBeforeMoveToTrash(MoveToTrashEventArgs e)
        {
            if (BeforeMoveToTrash != null)
                BeforeMoveToTrash(this, e);
        }

        /// <summary>
        /// Occurs when [after move to trash].
        /// </summary>
        public static event MoveToTrashEventHandler AfterMoveToTrash;
        /// <summary>
        /// Fires the after move to trash.
        /// </summary>
        /// <param name="e">The <see cref="umbraco.cms.businesslogic.MoveToTrashEventArgs"/> instance containing the event data.</param>
        protected virtual void FireAfterMoveToTrash(MoveToTrashEventArgs e)
        {
            if (AfterMoveToTrash != null)
                AfterMoveToTrash(this, e);
        } 
        #endregion
    
    }
}
