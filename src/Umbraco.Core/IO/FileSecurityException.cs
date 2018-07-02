using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.IO
{
    public class FileSecurityException : Exception
    {
        public FileSecurityException()
        {

        }

        public FileSecurityException(string message) : base(message)
        {

        }
    }
}
