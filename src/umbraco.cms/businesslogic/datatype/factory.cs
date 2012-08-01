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
    public class Factory
    {
        #region Declarations

		internal static readonly ConcurrentDictionary<Guid, Type> _controls = new ConcurrentDictionary<Guid, Type>();

        #endregion

        #region Constructors

        static Factory()
        {
            Initialize();
        }

        #endregion

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
            if (DataEditorId == Guid.Empty)
            {
                throw new ArgumentException("DataEditorId is empty. This usually means that no data editor was defined for the data type. To correct this update the entry in the cmsDataType table to ensure it matches a Guid from an installed data editor.");
            }
            if (_controls.ContainsKey(DataEditorId))
            {
            	var newObject = PluginTypeResolver.Current.CreateInstance<IDataType>(_controls[DataEditorId]);
                return newObject;
            }
            else
            {
                throw new ArgumentException("Could not find a IDataType control matching DataEditorId " + DataEditorId.ToString() + " in the controls collection. To correct this, check the data type definition in the developer section or ensure that the package/control is installed correctly.");
            }
        }

        /// <summary>
        /// Retrieve a complete list of all registered IDataType's
        /// </summary>
        /// <returns>A list of IDataType's</returns>
        public IDataType[] GetAll()
        {
            var retVal = new IDataType[_controls.Count];
            var c = 0;

            foreach (var id in _controls.Keys)
            {
                retVal[c] = GetNewObject(id);
                c++;
            }

            return retVal;
        }

        private static void Initialize()
        {
            // Get all datatypes from interface
        	var types = PluginTypeResolver.Current.ResolveDataTypes().ToArray();
			foreach (var t in types)
			{
				var instance = PluginTypeResolver.Current.CreateInstance<IDataType>(t);
				if (instance != null)
				{
					_controls.TryAdd(instance.Id, t);				
				}				
			}			
        }
        
    }
}