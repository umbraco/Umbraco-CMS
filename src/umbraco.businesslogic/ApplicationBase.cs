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
    /// ApplicationBase provides an easy to use base class to install event handlers in Umbraco.
    /// Classes inheriting from ApplcationBase are automatically registered and instantiated by Umbraco on application start.
    /// To use, inherit the ApplicationBase Class and add an empty constructor. 
    /// </summary>
    [Obsolete("ApplicationBase has been deprecated. Please use ApplicationStartupHandler instead.")]
    public abstract class ApplicationBase : IApplicationStartupHandler
    { }

}
