using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

        public bool IsRequired { get; set; }
        public string RegexValidationStatement { get; set; }

        public virtual DataEditorSettingValidationResult Validate()
        {
            if (IsRequired && string.IsNullOrEmpty(Value))
                return new DataEditorSettingValidationResult("Value is required");
            else if (!string.IsNullOrEmpty(RegexValidationStatement) && !Regex.IsMatch(Value, RegexValidationStatement))
                return new DataEditorSettingValidationResult("Value does not match the required pattern");
            else
                return null;
        }
    }
}
