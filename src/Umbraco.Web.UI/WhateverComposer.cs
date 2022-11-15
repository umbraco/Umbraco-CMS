using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;

public class WhateverComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.Configure<ContentSettings>(options =>
        {
            options.AllowedUploadFiles = new[] { "pdf", "mp3" };
        });
    }
}
