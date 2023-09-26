using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Filters;

internal abstract class SwaggerFilterBase<TBaseController>
    where TBaseController : Controller
{
    protected bool CanApply(OperationFilterContext context)
        => CanApply(context.MethodInfo);

    protected bool CanApply(ParameterFilterContext context)
        => CanApply(context.ParameterInfo.Member);

    private bool CanApply(MemberInfo member)
        => member.DeclaringType?.Implements<TBaseController>() is true;
}
