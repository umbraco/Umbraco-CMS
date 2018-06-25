using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Composing
{
    public interface IComposition
    {
        void Compose(IServiceCollection services);
    }
}
