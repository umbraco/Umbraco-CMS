using System;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco.cms.businesslogic.cache;
using umbraco.DataLayer;

namespace umbraco.cms.businesslogic.web
{
    public class StylesheetProperty : CMSNode
    {
        private string _alias;
        private string _value;

        private static readonly Guid ModuleObjectType = new Guid("5555da4f-a123-42b2-4488-dcdfb25e4111");
        
        public StylesheetProperty(int id) : base(id)
        {
            InitProperty();
        }

        public StylesheetProperty(Guid id) : base(id)
        {
            InitProperty();
        }

        private  void InitProperty() 
        {
            var dr = SqlHelper.ExecuteReader("Select stylesheetPropertyAlias,stylesheetPropertyValue from cmsStylesheetProperty where nodeId = " + this.Id);
            if (dr.Read())
            {
                _alias = dr.GetString("stylesheetPropertyAlias");
                _value = dr.GetString("stylesheetPropertyValue");
            } 
            else
                throw new ArgumentException("NO DATA EXSISTS");
            dr.Close();
        }

        public StyleSheet StyleSheet() 
        {
            return new StyleSheet(this.Parent.Id, true, false);
        }

        public void RefreshFromFile() 
        {
            // ping the stylesheet
            var ss = new StyleSheet(this.Parent.Id);
            InitProperty();
        }

        public string Alias 
        {
            get { return _alias; }
            set 
            {
                SqlHelper.ExecuteNonQuery(String.Format("update cmsStylesheetProperty set stylesheetPropertyAlias = '{0}' where nodeId = {1}", value.Replace("'", "''"), this.Id));
                _alias=value;

                InvalidateCache();
            }
        }

        public string value 
        {
            get { return _value; }
            set
            {
                SqlHelper.ExecuteNonQuery(String.Format("update cmsStylesheetProperty set stylesheetPropertyValue = '{0}' where nodeId = {1}", value.Replace("'", "''"), this.Id));
                _value = value;

                InvalidateCache();
            }
        }

        public static StylesheetProperty MakeNew(string Text, StyleSheet sheet, BusinessLogic.User user)
        {
            var newNode = CMSNode.MakeNew(sheet.Id, ModuleObjectType, user.Id, 2, Text, Guid.NewGuid());
            SqlHelper.ExecuteNonQuery(String.Format("Insert into cmsStylesheetProperty (nodeId,stylesheetPropertyAlias,stylesheetPropertyValue) values ('{0}','{1}','')", newNode.Id, Text));
            var ssp = new StylesheetProperty(newNode.Id);
            var e = new NewEventArgs();
            ssp.OnNew(e);
            return ssp;
        }

        public override void delete() 
        {
            var e = new DeleteEventArgs();
            FireBeforeDelete(e);

            if (!e.Cancel) 
            {
                SqlHelper.ExecuteNonQuery("delete from cmsStylesheetProperty where nodeId = @nodeId", SqlHelper.CreateParameter("@nodeId", this.Id));
                base.delete();
                FireAfterDelete(e);
            }
        }

        public override void Save()
        {
            var e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {
                base.Save();

                FireAfterSave(e);
            }
        }

        public override string ToString()
        {
            return String.Format("{0} {{\n{1}\n}}\n", this.Alias, this.value);
        }


        public static StylesheetProperty GetStyleSheetProperty(int id)
        {
            return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                GetCacheKey(id),
                TimeSpan.FromMinutes(30), () =>
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

        [Obsolete("Umbraco automatically refreshes the cache when stylesheets and stylesheet properties are saved or deleted")]
        private void InvalidateCache()
        {
            ApplicationContext.Current.ApplicationCache.ClearCacheItem(GetCacheKey(Id));            
        }

        private static string GetCacheKey(int id)
        {
            return CacheKeys.StylesheetPropertyCacheKey + id;
        }

        // EVENTS
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
        public static event NewEventHandler New;
        /// <summary>
        /// Raises the <see cref="E:New"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnNew(NewEventArgs e) {
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
