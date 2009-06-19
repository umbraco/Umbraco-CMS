using System;
using System.Data;
using umbraco.DataLayer;

using umbraco.cms.businesslogic.cache;

namespace umbraco.cms.businesslogic.web
{
	public class StylesheetProperty : CMSNode
	{
		private string _alias;
        private string _value;

        private static object stylesheetPropertyCacheSyncLock = new object();
        private static readonly string UmbracoStylesheetPropertyCacheKey = "UmbracoStylesheetProperty";

		private static Guid moduleObjectType = new Guid("5555da4f-a123-42b2-4488-dcdfb25e4111");
		// internal static moduleId = 

		public StylesheetProperty(int id) : base(id)
		{
			initProperty();
		}

		public StylesheetProperty(Guid id) : base(id)
		{
			initProperty();
		}
		private  void initProperty() {

            IRecordsReader dr = SqlHelper.ExecuteReader("Select stylesheetPropertyAlias,stylesheetPropertyValue from cmsStylesheetProperty where nodeId = " + this.Id.ToString());
			if (dr.Read())
			{
				_alias = dr.GetString("stylesheetPropertyAlias");
				_value = dr.GetString("stylesheetPropertyValue");
			} 
			else
				throw new ArgumentException("NO DATA EXSISTS");
			dr.Close();

		}


		public StyleSheet StyleSheet() {
			return new StyleSheet(this.Parent.Id, true, false);
		}

        public void RefreshFromFile() {
            // ping the stylesheet
            web.StyleSheet ss = new StyleSheet(this.Parent.Id);
            initProperty();
        }


		public string Alias {
			get{return _alias;}
			set {
				SqlHelper.ExecuteNonQuery("update cmsStylesheetProperty set stylesheetPropertyAlias = '"+ value.Replace("'","''")+"' where nodeId = " + this.Id);
				_alias=value;
                InvalidateCache();
            }
		}
	
		public string value {
			get {return _value;}
			set {
				SqlHelper.ExecuteNonQuery("update cmsStylesheetProperty set stylesheetPropertyValue = '"+ value.Replace("'","''")+"' where nodeId = " + this.Id);
				_value=value;
                InvalidateCache();
			}
		}

		public static StylesheetProperty MakeNew(string Text, StyleSheet sheet, BusinessLogic.User user) {
			CMSNode newNode = CMSNode.MakeNew(sheet.Id, moduleObjectType, user.Id, 2, Text, Guid.NewGuid());
			SqlHelper.ExecuteNonQuery("Insert into cmsStylesheetProperty (nodeId,stylesheetPropertyAlias,stylesheetPropertyValue) values ('"+ newNode.Id +"','" + Text+ "','')");
			StylesheetProperty ssp = new StylesheetProperty(newNode.Id);
            NewEventArgs e = new NewEventArgs();
            ssp.OnNew(e);
            return ssp;
		}

		new public void delete() 
		{
            DeleteEventArgs e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel) {
                InvalidateCache();
                SqlHelper.ExecuteNonQuery("delete from cmsStylesheetProperty where nodeId = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));
                base.delete();

                FireAfterDelete(e);
            }
		}

        public override void Save() {
            SaveEventArgs e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel) {
                base.Save();

                FireAfterSave(e);
            }
        }

		public override string ToString()
		{
			return this.Alias +" {\n"+ this.value+"\n}\n";
		}


        public static StylesheetProperty GetStyleSheetProperty(int id)
        {
            return Cache.GetCacheItem<StylesheetProperty>(GetCacheKey(id), stylesheetPropertyCacheSyncLock,
    TimeSpan.FromMinutes(30),
    delegate
    {
        try
        {
            return new StylesheetProperty(id);
        }
        catch
        {
            return null;
        }
    });

        }

        private void InvalidateCache()
        {
            Cache.ClearCacheItem(GetCacheKey(this.Id));
        }

        private static string GetCacheKey(int id)
        {
            return UmbracoStylesheetPropertyCacheKey + id;
        }



        //EVENTS
        /// <summary>
        /// The save event handler
        /// </summary>
        new public delegate void SaveEventHandler(StylesheetProperty sender, SaveEventArgs e);
        /// <summary>
        /// The new event handler
        /// </summary>
        new public delegate void NewEventHandler(StylesheetProperty sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        new public delegate void DeleteEventHandler(StylesheetProperty sender, DeleteEventArgs e);


        /// <summary>
        /// Occurs when [before save].
        /// </summary>
        new public static event SaveEventHandler BeforeSave;
        /// <summary>
        /// Raises the <see cref="E:BeforeSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        new protected virtual void FireBeforeSave(SaveEventArgs e) {
            if (BeforeSave != null)
                BeforeSave(this, e);
        }

        /// <summary>
        /// Occurs when [after save].
        /// </summary>
        new public static event SaveEventHandler AfterSave;
        /// <summary>
        /// Raises the <see cref="E:AfterSave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        new protected virtual void FireAfterSave(SaveEventArgs e) {
            if (AfterSave != null)
                AfterSave(this, e);
        }

        /// <summary>
        /// Occurs when [new].
        /// </summary>
        new public static event NewEventHandler New;
        /// <summary>
        /// Raises the <see cref="E:New"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        new protected virtual void OnNew(NewEventArgs e) {
            if (New != null)
                New(this, e);
        }

        /// <summary>
        /// Occurs when [before delete].
        /// </summary>
        new public static event DeleteEventHandler BeforeDelete;
        /// <summary>
        /// Raises the <see cref="E:BeforeDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        new protected virtual void FireBeforeDelete(DeleteEventArgs e) {
            if (BeforeDelete != null)
                BeforeDelete(this, e);
        }

        /// <summary>
        /// Occurs when [after delete].
        /// </summary>
        new public static event DeleteEventHandler AfterDelete;
        /// <summary>
        /// Raises the <see cref="E:AfterDelete"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        new protected virtual void FireAfterDelete(DeleteEventArgs e) {
            if (AfterDelete != null)
                AfterDelete(this, e);
        }
	}
}
