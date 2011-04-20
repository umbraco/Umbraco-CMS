using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.businesslogic.Exceptions
{
    /// <summary>
    /// Exception class when an Umbraco user either has wrong credentials or insufficient permissions
    /// </summary>
    public class UserAuthorizationException : Exception
    {
        public UserAuthorizationException()
        {
            
        }

        public UserAuthorizationException(string message) : base(message)
        {
            
        }
    }
}
