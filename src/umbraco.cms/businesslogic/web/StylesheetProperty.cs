using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco.cms.businesslogic.cache;
using Umbraco.Core.Models.Rdbms;
using umbraco.DataLayer;

namespace umbraco.cms.businesslogic.web
{
    [Obsolete("Do not use this, use the Umbraco.Core.Services.IFileService instead to manipulate stylesheets")]
    public class StylesheetProperty : CMSNode
    {
     
        internal Umbraco.Core.Models.Stylesheet StylesheetItem;
        internal Umbraco.Core.Models.StylesheetProperty StylesheetProp;

        internal StylesheetProperty(Umbraco.Core.Models.Stylesheet sheet, Umbraco.Core.Models.StylesheetProperty prop)
            : base(int.MaxValue, true)
        {
            StylesheetItem = sheet;
            StylesheetProp = prop;
        }

        public StylesheetProperty(int id) : base(id)
        {
        }

        public StylesheetProperty(Guid id) : base(id)
        {
        }

        /// <summary>
        /// Sets up the internal data of the CMSNode, used by the various constructors
        /// </summary>
        protected override void setupNode()
        {
            web.StyleSheet.ThrowNotSupported<StylesheetProperty>();
        }

        public StyleSheet StyleSheet() 
        {
            return new StyleSheet(StylesheetItem);
        }

        public void RefreshFromFile()
        {
            var name = StylesheetItem.Name;
            StylesheetItem = ApplicationContext.Current.Services.FileService.GetStylesheetByName(name);
            if (StylesheetItem == null) throw new ArgumentException(string.Format("No stylesheet exists with name '{0}'", name));

            StylesheetProp = StylesheetItem.Properties.FirstOrDefault(x => x.Alias == StylesheetProp.Alias);
        }

        /// <summary>
        /// Human readable name/label
        /// </summary>
        public override string Text
        {
            get { return StylesheetProp.Name; }
            set
            {
                //Changing the name requires removing the current property and then adding another new one

                if (StylesheetProp.Name != value)
                {
                    StylesheetItem.RemoveProperty(StylesheetProp.Name);
                    var newProp = new Umbraco.Core.Models.StylesheetProperty(value, StylesheetProp.Alias, StylesheetProp.Value);
                    StylesheetItem.AddProperty(newProp);
                    StylesheetProp = newProp;
                }
            }
        }

        public string Alias 
        {
            get { return StylesheetProp.Alias; }
            set { StylesheetProp.Alias = value; }
        }

        public string value 
        {
            get { return StylesheetProp.Value; }
            set { StylesheetProp.Value = value; }
        }

        public static StylesheetProperty MakeNew(string Text, StyleSheet sheet, BusinessLogic.User user)
        {
            //we need to create it with a temp place holder!
            var prop = new Umbraco.Core.Models.StylesheetProperty(Text, "#" + Text.ToSafeAlias(), "");
            sheet.StylesheetEntity.AddProperty(prop);
            ApplicationContext.Current.Services.FileService.SaveStylesheet(sheet.StylesheetEntity);

            var ssp = new StylesheetProperty(sheet.StylesheetEntity, prop);
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
                
                StylesheetItem.RemoveProperty(Text);
                ApplicationContext.Current.Services.FileService.SaveStylesheet(StylesheetItem);

                FireAfterDelete(e);
            }
        }

        public override void Save()
        {
            var e = new SaveEventArgs();
            FireBeforeSave(e);

            if (!e.Cancel)
            {
                ApplicationContext.Current.Services.FileService.SaveStylesheet(StylesheetItem);

                FireAfterSave(e);
            }
        }

        public override string ToString()
        {
            return String.Format("{0} {{\n{1}\n}}\n", this.Alias, this.value);
        }


        public static StylesheetProperty GetStyleSheetProperty(int id)
        {
            web.StyleSheet.ThrowNotSupported<StylesheetProperty>();
            return null;
        }

        // EVENTS
        /// <summary>
        /// The save event handler
        /// </summary>
        public delegate void SaveEventHandler(StylesheetProperty sender, SaveEventArgs e);
        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewEventHandler(StylesheetProperty sender, NewEventArgs e);
        /// <summary>
        /// The delete event handler
        /// </summary>
        public delegate void DeleteEventHandler(StylesheetProperty sender, DeleteEventArgs e);


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
