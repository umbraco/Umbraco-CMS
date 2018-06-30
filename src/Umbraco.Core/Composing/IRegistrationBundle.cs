using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Composing
{
    public interface IRegistrationBundle
    {
        // fixme - rename to register? better to "register" stuff than "compose" everything even tho it's not using "composite" pattern?
        void Compose(IContainer container);
    }
}
