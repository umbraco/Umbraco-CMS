using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.businesslogic.Exceptions
{
	[Obsolete("This class has been superceded by Umbraco.Core.IO.FileSecurityException")]
	public class FileSecurityException : Umbraco.Core.IO.FileSecurityException
    {
        public FileSecurityException()
			: base()
        {
            
        }

        public FileSecurityException(string message) 
			: base(message)
        {
            
        }
    }
}
