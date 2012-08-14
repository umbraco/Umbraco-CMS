using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using umbraco.BusinessLogic.Utils;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.BusinessLogic 
{
    /// <summary>
    /// ApplicationBase provides an easy to use base class to install event handlers in umbraco.
    /// Class inhiriting from ApplcationBase are automaticly registered and instantiated by umbraco on application start.
    /// To use, inhirite the ApplicationBase Class and add an empty constructor. 
    /// </summary>
    [Obsolete("ApplicationBase has been depricated. Please use ApplicationStartupHandler instead.")]
    public abstract class ApplicationBase : IApplicationStartupHandler
    { }

}
