using System.Web;

namespace Umbraco.Core
{
    public interface IHttpContextAccessor
    {
        HttpContextBase Value { get; }
    }
}
