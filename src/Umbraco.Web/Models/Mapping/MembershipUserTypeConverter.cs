using System.Web.Security;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// A converter to go from a <see cref="MembershipUser"/> to a <see cref="MemberDisplay"/>
    /// </summary>
    internal class MembershipUserTypeConverter : ITypeConverter<MembershipUser, MemberDisplay>
    {
        public MemberDisplay Convert(MembershipUser source, MemberDisplay destination, ResolutionContext context)
        {
            //first convert to IMember
            var member = Mapper.Map<IMember>(source);
            //then convert to MemberDisplay
            return Mapper.Map<MemberDisplay>(member);
        }
    }
}
