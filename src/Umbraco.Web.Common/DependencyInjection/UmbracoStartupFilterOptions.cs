using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Umbraco.Cms.Web.Common.DependencyInjection
{
    public class UmbracoStartupFilterOptions
    {
        /// <summary>
        /// Represents the pipeline that is executed before umbraco. By default this pipeline only adds UseDeveloperExceptionPage when the environments is Development.
        /// </summary>
        public Action<IApplicationBuilder> PreUmbracoPipeline { get; set; } = app =>
        {
            IWebHostEnvironment env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        };
    }
}
