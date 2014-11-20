using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace umbraco.businesslogic.Exceptions
{
    public class InvalidUmbracoPackageException : Exception
    {
        public override string ToString()
        {
            return "The package does not contain package.xml";
        }
    }
}
