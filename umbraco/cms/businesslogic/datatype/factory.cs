using System;
using System.Collections;
using System.Web;

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

        private static readonly Dictionary<Guid, Type> _controls = new Dictionary<Guid, Type>();

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
            IDataType newObject = Activator.CreateInstance(_controls[DataEditorId]) as IDataType;
            return newObject;
        }

        /// <summary>
        /// Retrieve a complete list of all registered IDataType's
        /// </summary>
        /// <returns>A list of IDataType's</returns>
        public IDataType[] GetAll()
        {
            IDataType[] retVal = new IDataType[_controls.Count];
            int c = 0;

            foreach (Guid id in _controls.Keys)
            {
                retVal[c] = GetNewObject(id);
                c++;
            }

            return retVal;
        }

        private static void Initialize()
        {
            // Get all datatypes from interface
            List<Type> types = TypeFinder.FindClassesOfType<IDataType>(true);
            getDataTypes(types);
        }

        private static void getDataTypes(List<Type> types)
        {
            foreach (Type t in types)
            {
                IDataType typeInstance = null;
                try
                {
                    if (t.IsVisible)
                    {
                        typeInstance = Activator.CreateInstance(t) as IDataType;
                    }
                }
                catch { }
                if (typeInstance != null)
                {
                    try
                    {
                        _controls.Add(typeInstance.Id, t);
                    }
                    catch (Exception ee)
                    {
                        BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Error, -1, "Can't import datatype '" + t.FullName + "': " + ee.ToString());
                    }
                }
            }
        }
    }
}