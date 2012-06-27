using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.Linq.DTMetal.CodeBuilder.DataType
{
    public abstract class DataTypeRetyper
    {
        /// <summary>
        /// Gets the .NET type of the DataType
        /// </summary>
        /// <value>The type of the member.</value>
        public abstract Type MemberType { get; }

        /// <summary>
        /// Creates the name for the retyped member. Overload if custom naming is required
        /// </summary>
        /// <param name="baseName">Name used for the standard implementation</param>
        /// <returns>baseName + MemberType.Name</returns>
        public virtual string MemberName(string baseName)
        {
            return baseName + this.MemberType.Name;
        }
    }
}
