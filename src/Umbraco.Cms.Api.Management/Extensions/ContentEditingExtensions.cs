using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Extensions;

internal static class ContentEditingExtensions
{
    public static bool DoesNotVaryByCulture(this IHasCultureAndSegment model)
        => model.VariesByCulture() == false;

    public static bool DoesNotVaryBySegment(this IHasCultureAndSegment model)
        => model.VariesBySegment() == false;

    public static bool VariesByCulture(this IHasCultureAndSegment model)
        => model.Culture is not null;

    public static bool VariesBySegment(this IHasCultureAndSegment model)
        => model.Segment is not null;
}
