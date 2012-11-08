using System;
using System.Collections;

namespace Umbraco.Core.Macros
{
	/// <summary>
	/// NOTE: This is legacy code, might require a cleanup
	/// </summary>
    [Serializable]
    internal class PersistableMacroProperty
    {
        #region Private Fields
        private string _name;
        private string _alias;
        private string _value;
        private string _assemblyName;
        private string _typeName;
        #endregion

        #region Properties
        /// <summary>
        /// Macro Caption
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Macro Alias
        /// </summary>
        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        /// <summary>
        /// Macro Value
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// AssemblyName of the Property of teh Macro
        /// </summary>
        public string AssemblyName
        {
            get { return _assemblyName; }
            set { _assemblyName = value; }
        }

        /// <summary>
        /// TypeName of the property of the macro
        /// </summary>
        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }
        #endregion

    }
}
