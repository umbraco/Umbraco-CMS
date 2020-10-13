using Microsoft.AspNetCore.Builder;


namespace Umbraco.Web.BackOffice.Filters
{
    public class UnhandledExceptionLoggerFilter
    {
        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<UnhandledExceptionLoggerMiddleware>();
        }
    }
}
