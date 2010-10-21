using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.datatype
{
    public abstract class DataEditorSettingType : ProviderBase, IDataEditorSettingType
    {
        public virtual string Value { get; set; }
        public abstract System.Web.UI.Control RenderControl(DataEditorSetting setting);

        private List<string> m_prevalues = new List<string>();
        public List<string> Prevalues
        {
            get { return m_prevalues; }
            set { m_prevalues = value; }
        }

        public string DefaultValue { get; set; }
    }
}
