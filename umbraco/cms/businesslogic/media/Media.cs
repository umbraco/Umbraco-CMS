using System;
using System.IO;
using umbraco.BusinessLogic.Actions;
using umbraco.DataLayer;
using System.Collections;
using System.Collections.Generic;
using umbraco.IO;
using System.Xml;
using System.Linq;
using umbraco.interfaces;
using umbraco.cms.businesslogic.datatype.controls;

namespace umbraco.cms.businesslogic.media
{
	/// <summary>
	/// A media represents a physical file and metadata on the file.
	///  
	/// By inheriting the Content class it has a generic datafields which enables custumization
	/// </summary>
	public class Media : Content
	{
        #region Constants and static members
        private const string m_SQLOptimizedMany = @"
			select 
				count(children.id) as children, umbracoNode.id, umbracoNode.uniqueId, umbracoNode.level, umbracoNode.parentId, umbracoNode.path, umbracoNode.sortOrder, umbracoNode.createDate, umbracoNode.nodeUser, umbracoNode.text, 
				cmsContentType.icon, cmsContentType.alias, cmsContentType.thumbnail, cmsContentType.description, cmsContentType.masterContentType, cmsContentType.nodeId as contentTypeId
			from umbracoNode 
			left join umbracoNode children on children.parentId = umbracoNode.id
			inner join cmsContent on cmsContent.nodeId = umbracoNode.id
			inner join cmsContentType on cmsContentType.nodeId = cmsContent.contentType
			where umbracoNode.nodeObjectType = @nodeObjectType AND {0}
			group by umbracoNode.id, umbracoNode.uniqueId, umbracoNode.level, umbracoNode.parentId, umbracoNode.path, umbracoNode.sortOrder, umbracoNode.createDate, umbracoNode.nodeUser, umbracoNode.text, 
				cmsContentType.icon, cmsContentType.alias, cmsContentType.thumbnail, cmsContentType.description, cmsContentType.masterContentType, cmsContentType.nodeId
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
        public static Media MakeNew(string Name, MediaType dct, BusinessLogic.User u, int ParentId)
        {
            Guid newId = Guid.NewGuid();
            // Updated to match level from base node
            CMSNode n = new CMSNode(ParentId);
            int newLevel = n.Level;
            newLevel++;
            CMSNode.MakeNew(ParentId, _objectType, u.Id, newLevel, Name, newId);
            Media tmp = new Media(newId);
            tmp.CreateContent(dct);

            NewEventArgs e = new NewEventArgs();
            tmp.OnNew(e);

            return tmp;
        }

        /// <summary>
        /// Retrieve a list of all toplevel medias and folders
        /// </summary>
        /// <returns></returns>
        public static Media[] GetRootMedias()
        {
            Guid[] topNodeIds = CMSNode.TopMostNodeIds(_objectType);

            Media[] retval = new Media[topNodeIds.Length];
            for (int i = 0; i < topNodeIds.Length; i++)
            {
                Media d = new Media(topNodeIds[i]);
                retval[i] = d;
            }
            return retval;
        }

        public static List<Media> GetChildrenForTree(int nodeId)
        {

            List<Media> tmp = new List<Media>();
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
            return tmp;
        }

        public static IEnumerable<Media> GetMediaOfMediaType(int mediaTypeId)
        {
            var tmp = new List<Media>();
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

            return tmp.ToArray();
        }

        /// <summary>
        /// Deletes all medias of the given type, used when deleting a mediatype
        /// 
        /// Use with care.
        /// </summary>
        /// <param name="dt"></param>
        public static void DeleteFromType(MediaType dt)
        {
            //get all document for the document type and order by level (top level first)
            var medias = Media.GetMediaOfMediaType(dt.Id)
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
            }
        }
        
        #endregion

        #region Public Properties
        /// <summary>
        /// Retrieve a list of all medias underneath the current
        /// </summary>
        public new Media[] Children
        {
            get
            {
                //return refactored optimized method
                return Media.GetChildrenForTree(this.Id).ToArray();
            }
        } 
        #endregion

        #region Public methods

        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {

                base.Save();

                XmlDocument xd = new XmlDocument();
                XmlGenerate(xd);

                // generate preview for blame history?
                if (UmbracoSettings.EnableGlobalPreviewStorage)
                {
                    // Version as new guid to ensure different versions are generated as media are not versioned currently!
                    savePreviewXml(generateXmlWithoutSaving(xd), Guid.NewGuid());
                }

                FireAfterSave(e);
            }
        }

        /// <summary>
        /// Moves the media to the trash
        /// </summary>
        public override void delete()
        {
            MoveToTrash();
        }

        /// <summary>
        /// With either move the media to the trash or permanently remove it from the database.
        /// </summary>
        /// <param name="deletePermanently">flag to set whether or not to completely remove it from the database or just send to trash</param>
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

        public override IEnumerable GetDescendants()
        {
            var tmp = new List<Media>();
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

            return tmp.ToArray();
        }

        #endregion

        #region Protected methods
        protected void PopulateMediaFromReader(IRecordsReader dr)
        {
            bool _hc = false;
            if (dr.GetInt("children") > 0)
                _hc = true;
            int? masterContentType = null;
            if (!dr.IsNull("masterContentType"))
                masterContentType = dr.GetInt("masterContentType");
            SetupMediaForTree(dr.GetGuid("uniqueId")
                , dr.GetShort("level")
                , dr.GetInt("parentId")
                , dr.GetInt("nodeUser")
                , dr.GetString("path")
                , dr.GetString("text")
                , dr.GetDateTime("createDate")
                , dr.GetString("icon")
                , _hc
                , dr.GetString("alias")
                , dr.GetString("thumbnail")
                , dr.GetString("description")
                , masterContentType
                , dr.GetInt("contentTypeId"));
        } 
        #endregion

        #region Private methods
        private void SetupMediaForTree(Guid uniqueId, int level, int parentId, int user, string path,
                                          string text, DateTime createDate, string icon, bool hasChildren, string contentTypeAlias, string contentTypeThumb,
                                            string contentTypeDesc, int? masterContentType, int contentTypeId)
        {
            SetupNodeForTree(uniqueId, _objectType, level, parentId, user, path, text, createDate, hasChildren);
            ContentType = new ContentType(contentTypeId, contentTypeAlias, icon, contentTypeThumb, masterContentType);
            ContentTypeIcon = icon;
        }

        /// <summary>
        /// Used internally to permanently delete the data from the database
        /// </summary>      
        /// <returns>returns true if deletion isn't cancelled</returns>
        private bool DeletePermanently()
        {
            DeleteEventArgs e = new DeleteEventArgs();

            FireBeforeDelete(e);

            if (!e.Cancel)
            {
                foreach (Media m in Children.ToList())
                {
                    m.DeletePermanently();
                }               

                // Remove all files
                DeleteAssociatedMediaFiles();

                base.delete();

                FireAfterDelete(e);
            }
            return !e.Cancel;
        }

        /// <summary>
        /// Used internally to move the node to the recyle bin
        /// </summary>
        /// <returns>Returns true if the move was not cancelled</returns>
        private bool MoveToTrash()
        {
            MoveToTrashEventArgs e = new MoveToTrashEventArgs();
            FireBeforeMoveToTrash(e);

            if (!e.Cancel)
            {
                Move((int)RecycleBin.RecycleBinType.Media);

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
