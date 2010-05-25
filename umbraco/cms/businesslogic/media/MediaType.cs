using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using umbraco.DataLayer;
using System.Linq;


namespace umbraco.cms.businesslogic.media
{
	/// <summary>
	/// The Mediatype
	/// 
	/// Due to the inheritance of the ContentType class,it enables definition of generic datafields on a Media.
	/// </summary>
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

        #endregion

        #region Constants and static members

        public static Guid _objectType = new Guid("4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e");       

        #endregion

        #region Static Methods
        /// <summary>
        /// Retrieve a MediaType by it's alias
        /// </summary>
        /// <param name="Alias">The alias of the MediaType</param>
        /// <returns>The MediaType with the alias</returns>
        public static new MediaType GetByAlias(string Alias)
        {
            return new MediaType(SqlHelper.ExecuteScalar<int>("SELECT nodeid from cmsContentType where alias = @alias",
                                                              SqlHelper.CreateParameter("@alias", Alias)));
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

        public static IEnumerable<MediaType> GetAllAsList()
        {
            
            var mediaTypes = new List<MediaType>();

            using (IRecordsReader dr =
                SqlHelper.ExecuteReader(m_SQLOptimizedGetAll, SqlHelper.CreateParameter("@nodeObjectType", MediaType._objectType)))
            {
                while (dr.Read())
                {
                    //check if the document id has already been added
                    if (mediaTypes.Where(x => x.Id == dr.Get<int>("id")).Count() == 0)
                    {
                        //create the DocumentType object without setting up
                        MediaType dt = new MediaType(dr.Get<int>("id"), true);
                        //populate it's CMSNode properties
                        dt.PopulateCMSNodeFromReader(dr);
                        //populate it's ContentType properties
                        dt.PopulateContentTypeNodeFromReader(dr);

                        mediaTypes.Add(dt);
                    }
                    else
                    {
                        //we've already created the document type with this id, so we'll add the rest of it's templates to itself
                        var dt = mediaTypes.Where(x => x.Id == dr.Get<int>("id")).Single();
                    }
                }
            }

            return mediaTypes.OrderBy(x => x.Text).ToList();

        } 

        /// <summary>
        /// Create a new Mediatype
        /// </summary>
        /// <param name="u">The Umbraco user context</param>
        /// <param name="Text">The name of the MediaType</param>
        /// <returns>The new MediaType</returns>
        public static MediaType MakeNew(BusinessLogic.User u, string Text)
        {

            int ParentId = -1;
            int level = 1;
            Guid uniqueId = Guid.NewGuid();
            CMSNode n = CMSNode.MakeNew(ParentId, _objectType, u.Id, level, Text, uniqueId);

            ContentType.Create(n.Id, Text, "");

            MediaType mt = new MediaType(n.Id);
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
                base.Save();

                FireBeforeSave(e);
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
                // delete all documents of this type
                Media.DeleteFromType(this);
                // Delete contentType
                base.delete();

                FireAfterDelete(e);
            }
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
