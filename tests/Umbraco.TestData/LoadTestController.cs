using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

// see https://github.com/Shazwazza/UmbracoScripts/tree/master/src/LoadTesting

namespace Umbraco.TestData;

public class LoadTestController : Controller
{
    private const string ContainerAlias = "LoadTestContainer";
    private const string ContentAlias = "LoadTestContent";
    private const int TextboxDefinitionId = -88;
    private const int MaxCreate = 1000;

    private const string FootHtml = @"</body>
</html>";

    private static readonly Random s_random = new();
    private static readonly object s_locko = new();

    private static volatile int s_containerId = -1;

    private static readonly string s_headHtml = @"<html>
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

    private static readonly string s_containerTemplateText = @"
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
" + s_headHtml + @"
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
    private readonly IFileService _fileService;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IShortStringHelper _shortStringHelper;

    public LoadTestController(
        IContentTypeService contentTypeService,
        IContentService contentService,
        IDataTypeService dataTypeService,
        IFileService fileService,
        IShortStringHelper shortStringHelper,
        IHostingEnvironment hostingEnvironment,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _contentTypeService = contentTypeService;
        _contentService = contentService;
        _dataTypeService = dataTypeService;
        _fileService = fileService;
        _shortStringHelper = shortStringHelper;
        _hostingEnvironment = hostingEnvironment;
        _hostApplicationLifetime = hostApplicationLifetime;
    }


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
        if (s_containerId > 0)
        {
            return null;
        }

        lock (s_locko)
        {
            if (s_containerId > 0)
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

            s_containerId = container.Id;
            return null;
        }
    }

    private IActionResult ContentHtml(string s) => Content(s_headHtml + s + FootHtml, "text/html");

    public IActionResult Install()
    {
        var contentType = new ContentType(_shortStringHelper, -1)
        {
            Alias = ContentAlias,
            Name = "LoadTest Content",
            Description = "Content for LoadTest",
            Icon = "icon-document"
        };
        var def = _dataTypeService.GetDataType(TextboxDefinitionId);
        contentType.AddPropertyType(new PropertyType(_shortStringHelper, def)
        {
            Name = "Origin",
            Alias = "origin",
            Description = "The origin of the content."
        });
        _contentTypeService.Save(contentType);

        var containerTemplate = ImportTemplate(
            "LoadTestContainer",
            "LoadTestContainer",
            s_containerTemplateText);

        var containerType = new ContentType(_shortStringHelper, -1)
        {
            Alias = ContainerAlias,
            Name = "LoadTest Container",
            Description = "Container for LoadTest content",
            Icon = "icon-document",
            AllowedAsRoot = true,
            IsContainer = true
        };
        containerType.AllowedContentTypes = containerType.AllowedContentTypes.Union(new[]
        {
            new ContentTypeSort(new Lazy<int>(() => contentType.Id), 0, contentType.Alias)
        });
        containerType.AllowedTemplates = containerType.AllowedTemplates.Union(new[] { containerTemplate });
        containerType.SetDefaultTemplate(containerTemplate);
        _contentTypeService.Save(containerType);

        var content = _contentService.Create("LoadTestContainer", -1, ContainerAlias);
        _contentService.SaveAndPublish(content);

        return ContentHtml("Installed.");
    }

    private Template ImportTemplate(string name, string alias, string text, ITemplate master = null)
    {
        var t = new Template(_shortStringHelper, name, alias) { Content = text };
        if (master != null)
        {
            t.SetMasterTemplate(master);
        }

        _fileService.SaveTemplate(t);
        return t;
    }

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
            var content = _contentService.Create(name, s_containerId, ContentAlias);
            content.SetValue("origin", o);
            _contentService.SaveAndPublish(content);
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
        lock (s_locko)
        {
            return s_random.Next(minValue, maxValue);
        }
    }

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

    public IActionResult ColdBootRestart()
    {
        Directory.Delete(
            _hostingEnvironment.MapPathContentRoot(Path.Combine(Constants.SystemDirectories.TempData, "DistCache")),
            true);

        DoRestart();

        return Content("Cold Boot Restarted.");
    }

    public IActionResult Restart()
    {
        DoRestart();

        return ContentHtml("Restarted.");
    }

    public IActionResult Die()
    {
        var timer = new Timer(_ => throw new Exception("die!"));
        _ = timer.Change(100, 0);

        return ContentHtml("Dying.");
    }

    public IActionResult Domains()
    {
        var currentDomain = AppDomain.CurrentDomain;
        var currentName = currentDomain.FriendlyName;
        var pos = currentName.IndexOf('-');
        if (pos > 0)
        {
            currentName = currentName.Substring(0, pos);
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
