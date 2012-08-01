using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using Umbraco.Core;
using umbraco.BusinessLogic.Utils;
using umbraco.interfaces;
using System.Collections.Generic;

namespace umbraco.cms.businesslogic.datatype.controls
{
    /// <summary>
    /// IDataType factory, handles the registering and retrieval of IDatatypes.
    /// 
    /// Then registering is done using reflection.
    /// </summary>
	[Obsolete("Use Umbraco.Core.DataTypesResolver instead")]
    public class Factory
    {        

        /// <summary>
        /// Retrieves the IDataType specified by it's unique ID
        /// </summary>
        /// <param name="DataTypeId">The IDataType id</param>
        /// <returns></returns>
        public IDataType DataType(Guid DataTypeId)
        {
            return GetNewObject(DataTypeId);
        }

        /// <summary>
        /// Retrieves the IDataType specified by it's unique ID
        /// </summary>
        /// <param name="DataEditorId">The IDataType id</param>
        /// <returns></returns>
        public IDataType GetNewObject(Guid DataEditorId)
        {
        	return DataTypesResolver.Current.GetById(DataEditorId);
        }

        /// <summary>
        /// Retrieve a complete list of all registered IDataType's
        /// </summary>
        /// <returns>A list of IDataType's</returns>
        public IDataType[] GetAll()
        {
        	return DataTypesResolver.Current.DataTypes.ToArray();
        }

        
    }
}