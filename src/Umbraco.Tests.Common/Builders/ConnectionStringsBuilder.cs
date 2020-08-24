using ConnectionStrings = Umbraco.Core.Configuration.Models.ConnectionStrings;

namespace Umbraco.Tests.Common.Builders
{
    public class ConnectionStringsBuilder : BuilderBase<ConnectionStrings>
    {
        private string _umbracoConnectionString;

        public ConnectionStringsBuilder WithUmbracoConnectionString(string umbracoConnectionString)
        {
            _umbracoConnectionString = umbracoConnectionString;
            return this;
        }

        public override ConnectionStrings Build()
        {
            var umbracoConnectionString = _umbracoConnectionString ?? string.Empty;

            return new ConnectionStrings
            {
                UmbracoConnectionString = umbracoConnectionString,             
            };
        }
    }
}
