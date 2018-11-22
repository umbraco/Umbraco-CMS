using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.Files
{
    [Obsolete("This is no longer used ane will be removed from the codebase in the future")]
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
