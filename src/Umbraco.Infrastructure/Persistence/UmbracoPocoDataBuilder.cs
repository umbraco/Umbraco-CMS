using System;
using System.Reflection;
using NPoco;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Umbraco's implementation of NPoco <see cref="PocoDataBuilder"/>.
    /// </summary>
    /// <remarks>
    /// <para>NPoco PocoDataBuilder analyzes DTO classes and returns infos about the tables and
    /// their columns.</para>
    /// <para>In some very special occasions, a class may expose a column that we do not want to
    /// use. This is essentially when adding a column to the User table: if the code wants the
    /// column to exist, and it does not exist yet in the database, because a given migration has
    /// not run, then the user cannot log into the site, and cannot upgrade = catch 22.</para>
    /// <para>So far, this is very manual. We don't try to be clever and figure out whether the
    /// columns exist already. We just ignore it.</para>
    /// <para>Beware, the application MUST restart when this class behavior changes.</para>
    /// </remarks>
    internal class UmbracoPocoDataBuilder : PocoDataBuilder
    {
        private readonly bool _upgrading;

        public UmbracoPocoDataBuilder(Type type, MapperCollection mapper, bool upgrading)
            : base(type, mapper)
        {
            _upgrading = upgrading;
        }

        protected override ColumnInfo GetColumnInfo(MemberInfo mi, Type type)
        {
            var columnInfo = base.GetColumnInfo(mi, type);

            if (_upgrading)
            {
                if (type == typeof(UserDto) && mi.Name == "TourData") columnInfo.IgnoreColumn = true;
            }

            return columnInfo;
        }
    }
}
