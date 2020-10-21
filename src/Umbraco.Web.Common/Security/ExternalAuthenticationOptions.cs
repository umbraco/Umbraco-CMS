using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Web.Common.Security
{
    // TODO: We need to implement this and extend it to support the back office external login options
    public interface IExternalAuthenticationOptions
    {
        ExternalSignInAutoLinkOptions Get(string authenticationType);
    }

}
