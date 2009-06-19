using System;

namespace umbraco.cms.businesslogic.media
{
	/// <summary>
	/// A media represents a physical file and metadata on the file.
	///  
	/// By inheriting the Content class it has a generic datafields which enables custumization
	/// </summary>
	public class Media : Content
	{
		/// <summary>
		/// Contructs a media object given the Id
		/// </summary>
		/// <param name="id">Identifier</param>
		public Media(int id) : base(id)
		{
		}

		/// <summary>
		/// Contructs a media object given the Id
		/// </summary>
		/// <param name="id">Identifier</param>
		public Media(Guid id) : base(id)
		{}


        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel) {

                base.Save();


                FireAfterSave(e);
            }
        }


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
			CMSNode.MakeNew(ParentId,_objectType, u.Id, newLevel,  Name, newId);
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
			for (int i = 0;i < topNodeIds.Length;i++) 
			{
				Media d = new Media(topNodeIds[i]);
				retval[i] = d;
			}
			return retval;
		}


		/// <summary>
		/// Retrieve a list of all medias underneath the current
		/// </summary>
		new public Media[] Children 
		{
			get
			{
				BusinessLogic.console.IconI[] tmp = base.Children;
				Media[] retval = new Media[tmp.Length];
				for (int i = 0; i < tmp.Length; i++) retval[i] = new Media(tmp[i].UniqueId);
				return retval;
			}
		}

		/// <summary>
		/// Deletes all medias of the given type, used when deleting a mediatype
		/// 
		/// Use with care.
		/// </summary>
		/// <param name="dt"></param>
		public static void DeleteFromType(MediaType dt) 
		{
			foreach (Content c in Media.getContentOfContentType(dt)) 
			{
				// due to recursive structure document might already been deleted..
				if (CMSNode.IsNode(c.UniqueId)) 
				{
					Media tmp = new Media(c.UniqueId);
					tmp.delete();
				}
			}
		}
		/// <summary>
		/// Deletes the current media and all children.
		/// </summary>
		new public void delete() 
		{
            DeleteEventArgs e = new DeleteEventArgs();

            FireBeforeDelete(e);

            if (!e.Cancel) {
                foreach (Media d in this.Children) {
                    d.delete();
                }

                // Remove all files
                interfaces.IDataType uploadField = new cms.businesslogic.datatype.controls.Factory().GetNewObject(new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));
                foreach (cms.businesslogic.property.Property p in this.getProperties)
                    if (p.PropertyType.DataTypeDefinition.DataType.Id == uploadField.Id &&
                        p.Value.ToString() != "" &&
                        System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(p.Value.ToString()))
                        )
                    {
                        
                        System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath(p.Value.ToString()));

                        string file = p.Value.ToString();
                        string extension = ((string)file.Substring(file.LastIndexOf(".") + 1, file.Length - file.LastIndexOf(".") - 1)).ToLower();
                        
                        //check for thumbnail
                        if (",jpeg,jpg,gif,bmp,png,tiff,tif,".IndexOf("," + extension + ",") > -1)
                        {
                            string thumbnailfile = file.Replace("." + extension, "_thumb");

                            if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(thumbnailfile + ".jpg")))
                                System.IO.File.Delete(System.Web.HttpContext.Current.Server.MapPath(thumbnailfile + ".jpg"));

                           //should also delete extra thumbnails
                        }

                    }

                       
                
                base.delete();

                FireAfterDelete(e);
            }
		}


        //EVENTS
        /// <summary>
        /// The save event handler
        /// </summary>
        public new delegate void SaveEventHandler(Media sender, SaveEventArgs e);
        /// <summary>
        /// The new  event handler
        /// </summary>
        public new delegate void NewEventHandler(Media sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public new delegate void DeleteEventHandler(Media sender, DeleteEventArgs e);


        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        public new static event SaveEventHandler BeforeSave;
        /// <summary>
        /// Raises the <see cref="E:BeforeSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected new virtual void FireBeforeSave(SaveEventArgs e) {
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
        protected new virtual void FireAfterSave(SaveEventArgs e) {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        /// <summary>
        /// Occurs when [new].
        /// </summary>
        public new static event NewEventHandler New;
        /// <summary>
        /// Raises the <see cref="E:New"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected new virtual void OnNew(NewEventArgs e) {
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
        protected new virtual void FireBeforeDelete(DeleteEventArgs e) {
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
        protected new virtual void FireAfterDelete(DeleteEventArgs e) {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }
    
    }
}
