using System;

namespace Umbraco.Tests.Shared.Builders
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
    }
}
