using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides container (folder) management services for data types.
/// </summary>
/// <remarks>
///     This service manages the folder structure used to organize data types
///     in the backoffice Settings section.
/// </remarks>
public interface IDataTypeContainerService : IEntityTypeContainerService<IDataType>
{
}
