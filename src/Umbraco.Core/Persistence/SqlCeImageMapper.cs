using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Linq;
using System.Reflection;
using NPoco;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Custom NPoco mapper for SqlCe
    /// </summary>
    /// <remarks>
    /// Work arounds to handle special columns
    /// </remarks>
    internal class SqlCeImageMapper : DefaultMapper
    {
        public override Func<object, object> GetToDbConverter(Type destType, MemberInfo sourceMemberInfo)
        {
            if (sourceMemberInfo.GetMemberInfoType() == typeof(byte[]))
            {
                return x =>
                {
                    var pd = Current.SqlContext.PocoDataFactory.ForType(sourceMemberInfo.DeclaringType);
                    if (pd == null) return null;
                    var col = pd.AllColumns.FirstOrDefault(x => x.MemberInfoData.MemberInfo == sourceMemberInfo);
                    if (col == null) return null;

                    return new SqlCeParameter
                    {
                        SqlDbType = SqlDbType.Image,
                        Value = x ?? Array.Empty<byte>()
                    };
                };
            }
            return base.GetToDbConverter(destType, sourceMemberInfo);
        }

        public override Func<object, object> GetParameterConverter(DbCommand dbCommand, Type sourceType)
        {
            if (sourceType == typeof(byte[]))
            {
                return x =>
                {
                    var param = new SqlCeParameter
                    {
                        SqlDbType = SqlDbType.Image,
                        Value = x
                    };
                    return param;
                };

            }
            return base.GetParameterConverter(dbCommand, sourceType);
        }
    }
}
