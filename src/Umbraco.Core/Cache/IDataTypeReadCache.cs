using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Cache;

public interface IDataTypeReadCache
{
    IDataType? GetDataType(int id);
}
