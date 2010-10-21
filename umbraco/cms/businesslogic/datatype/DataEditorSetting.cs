using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace umbraco.cms.businesslogic.datatype
{
    [System.AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class DataEditorSetting: System.Attribute
    {
        string name;
        public string description;
        public string type;
        public string prevalues;

        public DataEditorSetting(string name)
        {
            this.name = name;
            description = "";
            type = "umbraco.editorControls.SettingControls.TextField, umbraco.editorControls";

            prevalues = "";
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

            Type t = System.Web.Compilation.BuildManager.GetType(type, false);
            DataEditorSettingType dst = (DataEditorSettingType)System.Activator.CreateInstance(t, true);

          

            if (dst != null)
                dst.Prevalues = GetPrevalues();

            return dst;
        }
    }
}
