using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace umbraco.editorControls.userControlGrapper
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class usercontrolData : umbraco.cms.businesslogic.datatype.DefaultData
    {
        public usercontrolData(umbraco.cms.businesslogic.datatype.BaseDataType DataType) : base(DataType) { }


        public override System.Xml.XmlNode ToXMl(System.Xml.XmlDocument data)
        {

            if (this.Value != null)
            {

                XmlDocument xd = new XmlDocument();

                try
                {
                    xd.LoadXml(this.Value.ToString());
                    return data.ImportNode(xd.DocumentElement, true);
                }
                catch
                {
                     return base.ToXMl(data);
                }

            }
            else
            {

                return base.ToXMl(data);

            }

        }
    }
}
