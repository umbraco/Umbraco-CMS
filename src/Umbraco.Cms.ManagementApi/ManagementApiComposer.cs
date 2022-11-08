using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.ManagementApi.Configuration;
using Umbraco.Cms.ManagementApi.DependencyInjection;
using Umbraco.Cms.ManagementApi.OpenApi;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core;
using Umbraco.New.Cms.Core.Models.Configuration;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.ManagementApi;

public class ManagementApiComposer : IComposer
{
    private const string ApiTitle = "Umbraco Backoffice API";
    private const string ApiDefaultDocumentName = "v1";

    private ApiVersion DefaultApiVersion => new(1, 0);

    public void Compose(IUmbracoBuilder builder)
    {
        // TODO Should just call a single extension method that can be called fromUmbracoTestServerTestBase too, instead of calling this method

        IServiceCollection services = builder.Services;

        builder
            .AddNewInstaller()
            .AddUpgrader()
            .AddSearchManagement()
            .AddFactories()
            .AddTrees()
            .AddFactories()
            .AddServices()
            .AddMappers()
            .AddBackOfficeAuthentication();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = DefaultApiVersion;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.UseApiBehavior = false;
        });

        services.AddSwaggerGen(swaggerGenOptions =>
        {
            swaggerGenOptions.SwaggerDoc(
                ApiDefaultDocumentName,
                new OpenApiInfo
                {
                    Title = ApiTitle,
                    Version = DefaultApiVersion.ToString(),
                    Description = "This shows all APIs available in this version of Umbraco - including all the legacy apis that are available for backward compatibility"
                });

            swaggerGenOptions.DocInclusionPredicate((_, api) => !string.IsNullOrWhiteSpace(api.GroupName));

            swaggerGenOptions.TagActionsBy(api => new [] { api.GroupName });

            // see https://github.com/domaindrivendev/Swashbuckle.AspNetCore#change-operation-sort-order-eg-for-ui-sorting
            string ActionSortKeySelector(ApiDescription apiDesc)
                => $"{apiDesc.GroupName}_{apiDesc.ActionDescriptor.AttributeRouteInfo?.Template ?? apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.ActionDescriptor.RouteValues["action"]}_{apiDesc.HttpMethod}";
            swaggerGenOptions.OrderActionsBy(ActionSortKeySelector);

            swaggerGenOptions.AddSecurityDefinition("OAuth", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Umbraco",
                Type = SecuritySchemeType.OAuth2,
                Description = "Umbraco Authentication",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(Controllers.Security.Paths.BackOfficeApiAuthorizationEndpoint, UriKind.Relative),
                        TokenUrl = new Uri(Controllers.Security.Paths.BackOfficeApiTokenEndpoint, UriKind.Relative)
                    }
                }
            });

            swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                // this weird looking construct works because OpenApiSecurityRequirement
                // is a specialization of Dictionary<,>
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "OAuth",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string> { }
                }
            });

            swaggerGenOptions.DocumentFilter<MimeTypeDocumentFilter>();
            swaggerGenOptions.SchemaFilter<EnumSchemaFilter>();

            swaggerGenOptions.CustomSchemaIds(SchemaIdGenerator.Generate);
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.DefaultApiVersion = DefaultApiVersion;
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
            options.AddApiVersionParametersWhenVersionNeutral = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
        });
        services.AddControllers();
        builder.Services.ConfigureOptions<ConfigureMvcOptions>();

        // TODO: when this is moved to core, make the AddUmbracoOptions extension private again and remove core InternalsVisibleTo for Umbraco.Cms.ManagementApi
        builder.AddUmbracoOptions<NewBackOfficeSettings>();
        builder.Services.AddSingleton<IValidateOptions<NewBackOfficeSettings>, NewBackOfficeSettingsValidator>();

        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter(
                "BackofficeSwagger",
                applicationBuilder =>
                {
                    // Only use the API exception handler when we are requesting an API
                    applicationBuilder.UseWhen(
                        httpContext =>
                        {
                            GlobalSettings? settings = httpContext.RequestServices.GetRequiredService<IOptions<GlobalSettings>>().Value;
                            IHostingEnvironment hostingEnvironment = httpContext.RequestServices.GetRequiredService<IHostingEnvironment>();
                            var officePath = settings.GetBackOfficePath(hostingEnvironment);

                            return httpContext.Request.Path.Value?.StartsWith($"{officePath}/management/api/") ?? false;
                        },
                        innerBuilder =>
                        {
                            innerBuilder.UseExceptionHandler(exceptionBuilder => exceptionBuilder.Run(async context =>
                            {
                                Exception? exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error;
                                if (exception is null)
                                {
                                    return;
                                }

                                var response = new ProblemDetails
                                {
                                    Title = exception.Message,
                                    Detail = exception.StackTrace,
                                    Status = StatusCodes.Status500InternalServerError,
                                    Instance = exception.GetType().Name,
                                    Type = "Error",
                                };
                                await context.Response.WriteAsJsonAsync(response);
                            }));
                        });
                },
                applicationBuilder =>
                {
                    IServiceProvider provider = applicationBuilder.ApplicationServices;
                    IWebHostEnvironment webHostEnvironment = provider.GetRequiredService<IWebHostEnvironment>();

                    if (!webHostEnvironment.IsProduction())
                    {
                        GlobalSettings? settings = provider.GetRequiredService<IOptions<GlobalSettings>>().Value;
                        IHostingEnvironment hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
                        var officePath = settings.GetBackOfficePath(hostingEnvironment);

                        applicationBuilder.UseSwagger(swaggerOptions =>
                        {
                            swaggerOptions.RouteTemplate = $"{officePath.TrimStart(Core.Constants.CharArrays.ForwardSlash)}/swagger/{{documentName}}/swagger.json";
                        });
                        applicationBuilder.UseSwaggerUI(swaggerUiOptions =>
                        {
                            swaggerUiOptions.SwaggerEndpoint($"{officePath}/swagger/v1/swagger.json", $"{ApiTitle} {DefaultApiVersion}");
                            swaggerUiOptions.RoutePrefix = $"{officePath.TrimStart(Core.Constants.CharArrays.ForwardSlash)}/swagger";

                            swaggerUiOptions.OAuthClientId(Constants.OauthClientIds.Swagger);
                            swaggerUiOptions.OAuthUsePkce();
                        });
                    }
                },
                applicationBuilder =>
                {
                    IServiceProvider provider = applicationBuilder.ApplicationServices;

                    applicationBuilder.UseEndpoints(endpoints =>
                    {
                        GlobalSettings? settings = provider.GetRequiredService<IOptions<GlobalSettings>>().Value;
                        IHostingEnvironment hostingEnvironment = provider.GetRequiredService<IHostingEnvironment>();
                        var officePath = settings.GetBackOfficePath(hostingEnvironment);
                        // Maps attribute routed controllers.
                        endpoints.MapControllers();

                        // Serve contract
                        endpoints.MapGet($"{officePath}/management/api/openapi.json",async  context =>
                        {
                            await context.Response.SendFileAsync(new EmbeddedFileProvider(this.GetType().Assembly).GetFileInfo("OpenApi.json"));
                        });
                    });
                }
            ));
        });
    }
}
