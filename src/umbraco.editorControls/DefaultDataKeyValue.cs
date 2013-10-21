using System;
using System.Linq;
using Umbraco.Core.Logging;

namespace umbraco.editorControls
{
    /// <summary>
    /// Summary description for cms.businesslogic.datatype.DefaultDataKeyValue.
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class DefaultDataKeyValue : cms.businesslogic.datatype.DefaultData
    {
        public DefaultDataKeyValue(cms.businesslogic.datatype.BaseDataType DataType)
            : base(DataType)
        { }

        /// <summary>
        /// Gets the values of from cmsDataTypePreValues table by id and puts them into a CDATA section
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>		
        public override System.Xml.XmlNode ToXMl(System.Xml.XmlDocument xmlDocument)
        {
            var value = string.Empty;
            try
            {
                // Don't query if there's nothing to query for..
                if (string.IsNullOrWhiteSpace(Value.ToString()) == false)
                {
                    var dr = SqlHelper.ExecuteReader(string.Format("Select [value] from cmsDataTypeprevalues where id in ({0})", SqlHelper.EscapeString(Value.ToString())));

                    while (dr.Read())
                    {
                        value += value.Length == 0
                            ? dr.GetString("value")
                            : string.Format(",{0}", dr.GetString("value"));
                    }

                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<DefaultDataKeyValue>("An exception occurred converting the property data to XML", ex);
            }

            return xmlDocument.CreateCDataSection(value);
        }
    }
}
