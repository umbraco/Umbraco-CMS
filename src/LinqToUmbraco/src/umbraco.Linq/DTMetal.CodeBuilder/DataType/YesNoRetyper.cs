using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.Linq.DTMetal.CodeBuilder.DataType
{
    /// <summary>
    /// A converter for the Yes/ No DataType
    /// </summary>
    [DataType("38b352c1-e9f8-4fd8-9324-9a2eab06d97a")]
    public sealed class YesNoRetyper : DataTypeRetyper
    {
        public override Type MemberType
        {
            get { return typeof(bool); }
        }
    }
}
