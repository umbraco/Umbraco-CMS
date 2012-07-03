using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.Files
{
    public class NotAnImageException : Exception
    {
        public NotAnImageException()
            : base()
        {

        }

        public NotAnImageException(string message) : base(message)
        {
            
        }
    }
}
