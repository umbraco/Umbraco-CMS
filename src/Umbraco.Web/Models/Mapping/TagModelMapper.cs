using System.Net;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;

namespace Umbraco.Web.Models.Mapping
{
    internal class TagModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            //We want to de-code the text here because we know it's stored in the database as encoded values,
            // which ensures that the data cannot be tampered with for XSS, however when we want to return the tag
            // values via the tag query, we want to give the user the true value since it's possible this could be 
            // used outside of a web request or possible that a user will be rendering these tags in Razor which will
            // by default html encode the values - thus double encoding will occur, it's also an issue: http://issues.umbraco.org/issue/U4-9320
            // for us in the back office if we leave this encoded value since it will show up in the back office as double
            // encoded when performing look-aheads.
            config.CreateMap<ITag, TagModel>();
            //.ForMember(x => x.Text, expression => expression.MapFrom(tag => WebUtility.HtmlDecode(tag.Text)));
        }
    }
}