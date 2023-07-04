using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]

public interface IDeliveryApiContentIndexFieldDefinitionBuilder
{
    FieldDefinitionCollection Build();
}
