using System;
using Umbraco.Tests.Shared.Builders.Markers;

namespace Umbraco.Tests.Shared.Builders.Extensions
{
    public static class BuilderExtensions
    {
        public static T WithId<T>(this T builder, int id)
            where T : IWithIdBuilder
        {
            builder.Id = id;
            return builder;
        }

        public static T WithCreateDate<T>(this T builder, DateTime createDate)
            where T : IWithCreateDateBuilder
        {
            builder.CreateDate = createDate;
            return builder;
        }

        public static T WithUpdateDate<T>(this T builder, DateTime updateDate)
            where T : IWithUpdateDateBuilder
        {
            builder.UpdateDate = updateDate;
            return builder;
        }

        public static T WithAlias<T>(this T builder, string alias)
            where T : IWithAliasBuilder
        {
            builder.Alias = alias;
            return builder;
        }

        public static T WithName<T>(this T builder, string name)
            where T : IWithNameBuilder
        {
            builder.Name = name;
            return builder;
        }
    }
}
