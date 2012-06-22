using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlCE4Umbraco
{
    public class SqlCeProviderException : Exception
    {
        public SqlCeProviderException() : base()
        {

        }

        public SqlCeProviderException(string message) : base(message)
        {

        }
    }
}
