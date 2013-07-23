using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace umbraco.cms.businesslogic.datatype
{
    [Obsolete("This class is no longer used and will be removed from the codebase in the future.")]
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class DataEditorSetting: System.Attribute
    {
        string name;
        public string description;
        public Type type;
        public string prevalues;
        public object defaultValue;
        public bool isRequired;
        public string regexValidationStatement;

        public DataEditorSetting(string name)
        {
            this.name = name;
            description = "";
            type = System.Web.Compilation.BuildManager.GetType("umbraco.editorControls.SettingControls.TextField, umbraco.editorControls",false);
            prevalues = "";
            isRequired = false;
            regexValidationStatement = "";
        }

        public string GetName()
        {
            return name;
        }

        public List<string> GetPrevalues()
        {
            List<string> list = new List<string>();
            list.AddRange(prevalues.Split(','));

            return list;
        }

        public DataEditorSettingType GetDataEditorSettingType()
        {
            //Assembly a = string.IsNullOrEmpty(assembly) ? Assembly.GetExecutingAssembly() : Assembly.Load(assembly);
            //DataEditorSettingType dst = (DataEditorSettingType)a.CreateInstance(control);

            //Type t = System.Web.Compilation.BuildManager.GetType(type, false);
            DataEditorSettingType dst = (DataEditorSettingType)System.Activator.CreateInstance(type, true);

            if(defaultValue != null)
                dst.DefaultValue = defaultValue.ToString();

            if (dst != null)
                dst.Prevalues = GetPrevalues();

            dst.IsRequired = isRequired;
            dst.RegexValidationStatement = regexValidationStatement;


            return dst;
        }
    }
}
