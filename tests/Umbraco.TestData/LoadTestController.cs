using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

// see https://github.com/Shazwazza/UmbracoScripts/tree/master/src/LoadTesting

namespace Umbraco.TestData;

/// <summary>
/// Controller for load testing Umbraco CMS.
/// Provides endpoints for creating, managing, and stress-testing content operations.
/// </summary>
/// <remarks>
/// This controller is intended for testing purposes only and should not be used in production environments.
/// See <see href="https://github.com/Shazwazza/UmbracoScripts/tree/master/src/LoadTesting"/> for more information.
/// </remarks>
public class LoadTestController : Controller
{
    private const string ContainerAlias = "LoadTestContainer";
    private const string ContentAlias = "LoadTestContent";
    private const int MaxCreate = 1000;

    private const string FootHtml = @"</body>
</html>";

    private static readonly Random _random = new();
    private static readonly Lock _locko = new();

    private static volatile int _containerId = -1;

    private static readonly string _headHtml = @"<html>
<head>
  <title>LoadTest</title>
  <style>
    body { font-family: arial; }
    a,a:visited { color: blue; }
    h1 { margin: 0; padding: 0; font-size: 120%; font-weight: bold; }
    h1 a { text-decoration: none; }
    div.block { margin: 20px 0; }
    ul { margin:0; }
    div.ver { font-size: 80%; }
    div.head { padding:0 0 10px 0; margin: 0 0 20px 0; border-bottom: 1px solid #cccccc; }
  </style>
</head>
<body>
  <div class=""head"">
  <h1><a href=""/LoadTest"">LoadTest</a></h1>
  <div class=""ver"">@_umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild()</div>
  </div>
";

    private static readonly string _containerTemplateText = @"
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
@inject Umbraco.Cms.Core.Configuration.IUmbracoVersion _umbracoVersion
@{
    Layout = null;
    var container = Umbraco.ContentAtRoot().OfTypes(""" + ContainerAlias + @""").FirstOrDefault();
    var contents = container.Children().ToArray();
    var groups = contents.GroupBy(x => x.Value<string>(""origin""));
    var id = contents.Length > 0 ? contents[0].Id : -1;
    var wurl = Context.Request.Query[""u""] == ""1"";
    var missing = contents.Length > 0 && contents[contents.Length - 1].Id - contents[0].Id >= contents.Length;
}
" + _headHtml + @"
<div class=""block"">
<span @Html.Raw(missing ? ""style=\""color:red;\"""" : """")>@contents.Length items</span>
<ul>
@foreach (var group in groups)
{
    <li>@group.Key: @group.Count()</li>
}
</ul></div>
<div class=""block"">
@foreach (var content in contents)
{
	while (content.Id > id)
	{
		<div style=""color:red;"">@id :: MISSING</div>
		id++;
	}
    if (wurl)
    {
        <div>@content.Id :: @content.Name :: @content.Url()</div>
    }
    else
    {
        <div>@content.Id :: @content.Name</div>
    }	id++;
}
</div>
" + FootHtml;

    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly ITemplateService _templateService;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IShortStringHelper _shortStringHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadTestController"/> class.
    /// </summary>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="contentService">The content service.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="templateService">The template service.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="hostEnvironment">The host environment.</param>
    /// <param name="hostApplicationLifetime">The host application lifetime.</param>
    public LoadTestController(
        IContentTypeService contentTypeService,
        IContentService contentService,
        IDataTypeService dataTypeService,
        ITemplateService templateService,
        IShortStringHelper shortStringHelper,
        IHostEnvironment hostEnvironment,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _contentTypeService = contentTypeService;
        _contentService = contentService;
        _dataTypeService = dataTypeService;
        _templateService = templateService;
        _shortStringHelper = shortStringHelper;
        _hostEnvironment = hostEnvironment;
        _hostApplicationLifetime = hostApplicationLifetime;
    }


    /// <summary>
    /// Displays the load test index page with available actions.
    /// </summary>
    /// <returns>An HTML page listing available load test operations.</returns>
    public IActionResult Index()
    {
        var res = EnsureInitialize();
        if (res != null)
        {
            return res;
        }

        var html = @"Welcome. You can:
<ul>
    <li><a href=""/LoadTestContainer"">List existing contents</a> (u:url)</li>
    <li><a href=""/LoadTest/Create?o=browser"">Create a content</a> (o:origin, r:restart, n:number)</li>
    <li><a href=""/LoadTest/Clear"">Clear all contents</a></li>
    <li><a href=""/LoadTest/Domains"">List the current domains in w3wp.exe</a></li>
    <li><a href=""/LoadTest/Restart"">Restart the current AppDomain</a></li>
    <li><a href=""/LoadTest/Recycle"">Recycle the AppPool</a></li>
    <li><a href=""/LoadTest/Die"">Cause w3wp.exe to die</a></li>
</ul>
";

        return ContentHtml(html);
    }

    private IActionResult EnsureInitialize()
    {
        if (_containerId > 0)
        {
            return null;
        }

        lock (_locko)
        {
            if (_containerId > 0)
            {
                return null;
            }

            var contentType = _contentTypeService.Get(ContentAlias);
            if (contentType == null)
            {
                return ContentHtml("Not installed, first you must <a href=\"/LoadTest/Install\">install</a>.");
            }

            var containerType = _contentTypeService.Get(ContainerAlias);
            if (containerType == null)
            {
                return ContentHtml("Panic! Container type is missing.");
            }

            var container = _contentService.GetPagedOfType(containerType.Id, 0, 100, out _, null).FirstOrDefault();
            if (container == null)
            {
                return ContentHtml("Panic! Container is missing.");
            }

            _containerId = container.Id;
            return null;
        }
    }

    private IActionResult ContentHtml(string s) => Content(_headHtml + s + FootHtml, "text/html");

    /// <summary>
    /// Installs the load test content types, templates, and initial container content.
    /// </summary>
    /// <returns>An HTML response indicating the installation status.</returns>
    public async Task<IActionResult> Install()
    {
        var contentType = new ContentType(_shortStringHelper, -1)
        {
            Alias = ContentAlias,
            Name = "LoadTest Content",
            Description = "Content for LoadTest",
            Icon = "icon-document"
        };
        var def = await _dataTypeService.GetAsync(Constants.DataTypes.Guids.TextstringGuid);
        contentType.AddPropertyType(new PropertyType(_shortStringHelper, def)
        {
            Name = "Origin",
            Alias = "origin",
            Description = "The origin of the content."
        });
        await _contentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var containerTemplate = await ImportTemplateAsync(
            "LoadTestContainer",
            "LoadTestContainer",
            _containerTemplateText);

        var containerType = new ContentType(_shortStringHelper, -1)
        {
            Alias = ContainerAlias,
            Name = "LoadTest Container",
            Description = "Container for LoadTest content",
            Icon = "icon-document",
            AllowedAsRoot = true,
            ListView = Constants.DataTypes.Guids.ListViewContentGuid,
        };
        containerType.AllowedContentTypes = containerType.AllowedContentTypes.Union(new[]
        {
            new ContentTypeSort(contentType.Key, 0, contentType.Alias)
        });
        containerType.AllowedTemplates = containerType.AllowedTemplates.Union(new[] { containerTemplate });
        containerType.SetDefaultTemplate(containerTemplate);
        await _contentTypeService.CreateAsync(containerType, Constants.Security.SuperUserKey);

        var content = _contentService.Create("LoadTestContainer", -1, ContainerAlias);
        _contentService.Save(content);
        _contentService.Publish(content, content.AvailableCultures.ToArray());

        return ContentHtml("Installed.");
    }

    /// <summary>
    /// Creates and imports a template asynchronously.
    /// </summary>
    /// <param name="name">The name of the template.</param>
    /// <param name="alias">The alias of the template.</param>
    /// <param name="text">The template content.</param>
    /// <returns>The created template.</returns>
    private async Task<ITemplate> ImportTemplateAsync(string name, string alias, string text)
    {
        var result = await _templateService.CreateAsync(name, alias, text, Constants.Security.SuperUserKey);
        if (result.Success is false)
        {
            throw new InvalidOperationException($"Failed to create template '{name}' with alias '{alias}'.");
        }
        return result.Result;
    }

    /// <summary>
    /// Creates one or more content items for load testing.
    /// </summary>
    /// <param name="n">The number of content items to create (default: 1, max: 1000).</param>
    /// <param name="r">The restart probability percentage (0-100). If triggered, the application will restart after creating content.</param>
    /// <param name="o">The origin identifier to tag created content with.</param>
    /// <returns>An HTML response indicating the number of items created.</returns>
    public IActionResult Create(int n = 1, int r = 0, string o = null)
    {
        var res = EnsureInitialize();
        if (res != null)
        {
            return res;
        }

        if (r < 0)
        {
            r = 0;
        }

        if (r > 100)
        {
            r = 100;
        }

        var restart = GetRandom(0, 100) > 100 - r;

        if (n < 1)
        {
            n = 1;
        }

        if (n > MaxCreate)
        {
            n = MaxCreate;
        }

        for (var i = 0; i < n; i++)
        {
            var name = Guid.NewGuid().ToString("N").ToUpper() + "-" + (restart ? "R" : "X") + "-" + o;
            var content = _contentService.Create(name, _containerId, ContentAlias);
            content.SetValue("origin", o);
            _contentService.Save(content);
            _contentService.Publish(content, content.AvailableCultures.ToArray());
        }

        if (restart)
        {
            DoRestart();
        }

        return ContentHtml("Created " + n + " content"
                           + (restart ? ", and restarted" : string.Empty)
                           + ".");
    }

    private static int GetRandom(int minValue, int maxValue)
    {
        lock (_locko)
        {
            return _random.Next(minValue, maxValue);
        }
    }

    /// <summary>
    /// Clears all load test content items from the container.
    /// </summary>
    /// <returns>An HTML response indicating the content has been cleared.</returns>
    public IActionResult Clear()
    {
        var res = EnsureInitialize();
        if (res != null)
        {
            return res;
        }

        var contentType = _contentTypeService.Get(ContentAlias);
        _contentService.DeleteOfType(contentType.Id);

        return ContentHtml("Cleared.");
    }

    private void DoRestart()
    {
        HttpContext.User = null;
        Thread.CurrentPrincipal = null;
        _hostApplicationLifetime.StopApplication();
    }

    /// <summary>
    /// Performs a cold boot restart by clearing the distributed cache and restarting the application.
    /// </summary>
    /// <returns>A text response indicating the application has been cold boot restarted.</returns>
    public IActionResult ColdBootRestart()
    {
        Directory.Delete(
            _hostEnvironment.MapPathContentRoot(Path.Combine(Constants.SystemDirectories.TempData, "DistCache")),
            true);

        DoRestart();

        return Content("Cold Boot Restarted.");
    }

    /// <summary>
    /// Restarts the application.
    /// </summary>
    /// <returns>An HTML response indicating the application has been restarted.</returns>
    public IActionResult Restart()
    {
        DoRestart();

        return ContentHtml("Restarted.");
    }

    /// <summary>
    /// Causes the application to crash by throwing an unhandled exception.
    /// </summary>
    /// <returns>An HTML response indicating the application is dying.</returns>
    /// <remarks>
    /// WARNING: This action will cause the worker process to terminate unexpectedly.
    /// Use with caution and only in controlled testing environments.
    /// </remarks>
    public IActionResult Die()
    {
        var timer = new Timer(_ => throw new Exception("die!"));
        _ = timer.Change(100, 0);

        return ContentHtml("Dying.");
    }

    /// <summary>
    /// Lists information about the current process and application domains.
    /// </summary>
    /// <returns>An HTML response containing process and domain information.</returns>
    public IActionResult Domains()
    {
        var currentDomain = AppDomain.CurrentDomain;
        var currentName = currentDomain.FriendlyName;
        var pos = currentName.IndexOf('-');
        if (pos > 0)
        {
            currentName = currentName[..pos];
        }

        var text = new StringBuilder();
        text.Append("<div class=\"block\">Process ID: " + Process.GetCurrentProcess().Id + "</div>");
        text.Append("<div class=\"block\">");

        // TODO (V9): Commented out as I assume not available?
        ////text.Append("<div>IIS Site: " + HostingEnvironment.ApplicationHost.GetSiteName() + "</div>");

        text.Append("<div>App ID: " + currentName + "</div>");
        //text.Append("<div>AppPool: " + Zbu.WebManagement.AppPoolHelper.GetCurrentApplicationPoolName() + "</div>");
        text.Append("</div>");

        text.Append("<div class=\"block\">Domains:<ul>");
        text.Append("<li>Not implemented.</li>");
        /*
        foreach (var domain in Zbu.WebManagement.AppDomainHelper.GetAppDomains().OrderBy(x => x.Id))
        {
            var name = domain.FriendlyName;
            pos = name.IndexOf('-');
            if (pos > 0) name = name.Substring(0, pos);
            text.Append("<li style=\""
                + (name != currentName ? "color: #cccccc;" : "")
                //+ (domain.Id == currentDomain.Id ? "" : "")
                + "\">"
                +"[" + domain.Id + "] " + name
                + (domain.IsDefaultAppDomain() ? " (default)" : "")
                + (domain.Id == currentDomain.Id ? " (current)" : "")
                + "</li>");
        }
        */
        text.Append("</ul></div>");

        return ContentHtml(text.ToString());
    }
}
