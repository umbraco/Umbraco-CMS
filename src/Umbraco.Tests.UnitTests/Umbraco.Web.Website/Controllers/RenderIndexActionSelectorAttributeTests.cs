using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Website.Controllers
{
    [TestFixture]
    public class RenderIndexActionSelectorAttributeTests
    {
        [Test]
        public void IsValidForRequest__ensure_caching_works()
        {
            var sut = new RenderIndexActionSelectorAttribute();

            var actionDescriptor =
                GetRenderMvcControllerIndexMethodFromCurrentType(typeof(MatchesDefaultIndexController)).First();
            var actionDescriptorCollectionProviderMock = new Mock<IActionDescriptorCollectionProvider>();
            actionDescriptorCollectionProviderMock.Setup(x => x.ActionDescriptors)
                .Returns(new ActionDescriptorCollection(Array.Empty<ActionDescriptor>(), 1));

            var routeContext = CreateRouteContext(actionDescriptorCollectionProviderMock.Object);

            // Call the method multiple times
            for (var i = 0; i < 1; i++)
            {
                sut.IsValidForRequest(routeContext, actionDescriptor);
            }

            //Ensure the underlying ActionDescriptors is only called once.
            actionDescriptorCollectionProviderMock.Verify(x=>x.ActionDescriptors, Times.Once);
        }

        [Test]
        [TestCase(typeof(MatchesDefaultIndexController),
            "Index", new[] { typeof(ContentModel) }, typeof(IActionResult), ExpectedResult = true)]
        [TestCase(typeof(MatchesOverriddenIndexController),
            "Index", new[] { typeof(ContentModel) }, typeof(IActionResult), ExpectedResult = true)]
        [TestCase(typeof(MatchesCustomIndexController),
            "Index", new[] { typeof(ContentModel), typeof(int) }, typeof(IActionResult), ExpectedResult = false)]
        [TestCase(typeof(MatchesAsyncIndexController),
            "Index", new[] { typeof(ContentModel) }, typeof(Task<IActionResult>), ExpectedResult = false)]
        public bool IsValidForRequest__must_return_the_expected_result(Type controllerType, string actionName, Type[] parameterTypes, Type returnType)
        {
            //Fake all IActionDescriptor's that will be returned by IActionDescriptorCollectionProvider
            var actionDescriptors = GetRenderMvcControllerIndexMethodFromCurrentType(controllerType);

            // Find the one that match the current request
            var actualActionDescriptor = actionDescriptors.Single(x => x.ActionName == actionName
               && x.ControllerTypeInfo.Name == controllerType.Name
               && x.MethodInfo.ReturnType == returnType
               && x.MethodInfo.GetParameters().Select(m => m.ParameterType).SequenceEqual(parameterTypes));

            //Fake the IActionDescriptorCollectionProvider and add it to the service collection on httpcontext
            var sut = new RenderIndexActionSelectorAttribute();

            var routeContext = CreateRouteContext(new TestActionDescriptorCollectionProvider(actionDescriptors));

            //Act
            var result = sut.IsValidForRequest(routeContext, actualActionDescriptor);
            return result;
        }

        private ControllerActionDescriptor[] GetRenderMvcControllerIndexMethodFromCurrentType(Type controllerType)
        {
            var actions = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName
                            && m.GetCustomAttribute<NonActionAttribute>() is null
                            && m.Module.Name.Contains("Umbraco"));

            var actionDescriptors = actions
                .Select(x => new ControllerActionDescriptor()
                {
                    ControllerTypeInfo = controllerType.GetTypeInfo(),
                    ActionName = x.Name,
                    MethodInfo = x
                }).ToArray();

            return actionDescriptors;
        }

        private static RouteContext CreateRouteContext(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            //Fake the IActionDescriptorCollectionProvider and add it to the service collection on httpcontext
            var httpContext = new DefaultHttpContext();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(actionDescriptorCollectionProvider);
            httpContext.RequestServices =
                new DefaultServiceProviderFactory(new ServiceProviderOptions())
                    .CreateServiceProvider(serviceCollection);

            // Put the fake httpcontext on the route context.
            var routeContext = new RouteContext(httpContext);
            return routeContext;
        }

        private class TestActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
        {
            public TestActionDescriptorCollectionProvider(IReadOnlyList<ActionDescriptor> items)
            {
                ActionDescriptors = new ActionDescriptorCollection(items, 1);
            }

            public ActionDescriptorCollection ActionDescriptors { get; }
        }

        private class MatchesDefaultIndexController : RenderMvcController
        {
            public MatchesDefaultIndexController(ILogger<RenderMvcController> logger,
                ICompositeViewEngine compositeViewEngine) : base(logger, compositeViewEngine)
            {
            }
        }

        private class MatchesOverriddenIndexController : RenderMvcController
        {
            public override IActionResult Index(ContentModel model)
            {
                return base.Index(model);
            }

            public MatchesOverriddenIndexController(ILogger<RenderMvcController> logger,
                ICompositeViewEngine compositeViewEngine) : base(logger, compositeViewEngine)
            {
            }
        }

        private class MatchesCustomIndexController : RenderMvcController
        {
            public IActionResult Index(ContentModel model, int page)
            {
                return base.Index(model);
            }

            public MatchesCustomIndexController(ILogger<RenderMvcController> logger,
                ICompositeViewEngine compositeViewEngine) : base(logger, compositeViewEngine)
            {
            }
        }

        private class MatchesAsyncIndexController : RenderMvcController
        {
            public new async Task<IActionResult> Index(ContentModel model)
            {
                return await Task.FromResult(base.Index(model));
            }

            public MatchesAsyncIndexController(ILogger<RenderMvcController> logger,
                ICompositeViewEngine compositeViewEngine) : base(logger, compositeViewEngine)
            {
            }
        }
    }
}
