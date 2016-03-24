using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;

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

        public static StylesheetProperty MakeNew(string Text, StyleSheet sheet, IUser user)
        {
            //we need to create it with a temp place holder!
            var prop = new Umbraco.Core.Models.StylesheetProperty(Text, "#" + Text.ToSafeAlias(), "");
            sheet.StylesheetEntity.AddProperty(prop);
            ApplicationContext.Current.Services.FileService.SaveStylesheet(sheet.StylesheetEntity);

            var ssp = new StylesheetProperty(sheet.StylesheetEntity, prop);
            return ssp;
        }

        public override void delete() 
        {
            StylesheetItem.RemoveProperty(Text);
            ApplicationContext.Current.Services.FileService.SaveStylesheet(StylesheetItem);

        }

        public override void Save()
        {
            ApplicationContext.Current.Services.FileService.SaveStylesheet(StylesheetItem);
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
        
    }
}
