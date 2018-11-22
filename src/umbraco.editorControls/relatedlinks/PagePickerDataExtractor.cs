using System;
using System.Collections.Generic;
using System.Text;
using umbraco.interfaces;

namespace umbraco.editorControls.relatedlinks
{
    /// <summary>
    /// Allows for the extraction of the link ID for the selected node of the
    /// PagePicker (aka content picker) class for integration of the PagePicker
    /// in another datatype.
    /// This class replaces the database linkup that normally holds the data and
    /// stores the data locally in memory and allows for easy access to the
    /// data (after the IDataEditor has performed a save()).
    /// This class was not designed for, but might work equally well for other datatypes.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    class PagePickerDataExtractor : IData
    {
        private object _value;

        public PagePickerDataExtractor() { }
        public PagePickerDataExtractor(object o)
        {
            Value = o;
        }

        #region IData Members

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void MakeNew(int PropertyId)
        {
            throw new NotImplementedException();
        }

        public int PropertyId
        {
            set { throw new NotImplementedException(); }
        }

        public System.Xml.XmlNode ToXMl(System.Xml.XmlDocument d)
        {
            throw new NotImplementedException();
        }

        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        #endregion
    }
}
