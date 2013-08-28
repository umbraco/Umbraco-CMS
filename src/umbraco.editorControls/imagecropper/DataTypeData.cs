﻿using System.Xml;

namespace umbraco.editorControls.imagecropper
{
    public class DataTypeData : umbraco.cms.businesslogic.datatype.DefaultData
    {
        public DataTypeData(umbraco.cms.businesslogic.datatype.BaseDataType DataType) : base(DataType) { }

        public override XmlNode ToXMl(XmlDocument data)
        {
            if (Value!=null && Value.ToString() != "") {
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(Value.ToString());
                return data.ImportNode(xd.DocumentElement, true);
            } else {
                return base.ToXMl(data);
            }

        }
    }
}