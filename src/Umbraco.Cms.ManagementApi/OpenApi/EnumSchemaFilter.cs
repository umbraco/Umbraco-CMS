﻿using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Umbraco.Cms.ManagementApi.OpenApi;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            model.Enum.Clear();
            foreach (var name in Enum.GetNames(context.Type))
            {
                var actualName = context.Type.GetField(name)?.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? name;
                model.Enum.Add(new OpenApiString(actualName));
            }
        }
    }
}
