using System;
using System.Xml;

namespace umbraco.interfaces
{

    /// <summary>
    /// The IData is part of the IDataType interface for creating new data types in the umbraco backoffice. 
    /// The IData represents the actual value entered by the user.
    /// </summary>
	public interface IData 
	{
        /// <summary>
        /// Gets or sets the property id.
        /// </summary>
        /// <value>The property id.</value>
		int PropertyId{set;}

        /// <summary>
        /// Converts the data to XML.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The data as XML.</returns>
		XmlNode ToXMl(XmlDocument data);

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
		object Value {get;set;}

        /// <summary>
        /// Creates a new value
        /// </summary>
        /// <param name="PropertyId">The property id.</param>
		void MakeNew(int PropertyId);

        /// <summary>
        /// Deletes this instance.
        /// </summary>
		void Delete();
	}
}
