using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Configuration.Grid
{
    public interface IGridConfig
    {

        IGridEditorsConfig EditorsConfig { get; }

    }
}
