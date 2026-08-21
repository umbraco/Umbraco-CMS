using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

[TestFixture]
public class SchemaLockdownConventionTests
{
    [EntityType(Constants.UdiEntityType.DocumentType)]
    private class DocumentTypeController
    {
        public void Get()
        {
        }

        public void Post()
        {
        }

        [SchemaOperation(SchemaOperation.Read)]
        public void PostButReadOnly()
        {
        }

        public void NoVerb()
        {
        }
    }

    private class NoEntityTypeController
    {
        public void Post()
        {
        }
    }

    [EntityType(Constants.UdiEntityType.MediaType)]
    private class BaseControllerWithEntityType
    {
    }

    private class DerivedControllerWithoutOwnAttribute : BaseControllerWithEntityType
    {
        public void Post()
        {
        }
    }

    private static SchemaLockdownConvention CreateConvention()
    {
        var accessor = new SchemaLockdownMatrixAccessor(
            Options.Create(new SchemaLockdownSettings()),
            new SchemaLockdownConfiguratorCollection(() => []));

        return new SchemaLockdownConvention(accessor);
    }

    private static ControllerModel CreateControllerModel(Type controllerType)
    {
        TypeInfo typeInfo = controllerType.GetTypeInfo();

        // Mirrors how DefaultApplicationModelProvider.CreateControllerModel populates ControllerModel.Attributes,
        // so a test built this way reflects what the framework would actually hand the convention.
        object[] attributes = typeInfo.GetCustomAttributes(inherit: true);
        return new ControllerModel(typeInfo, attributes);
    }

    private static ActionModel AddAction(ControllerModel controller, MethodInfo method, string? httpMethod)
    {
        object[] attributes = method.GetCustomAttributes(inherit: true);
        var action = new ActionModel(method, attributes) { Controller = controller };

        var selector = new SelectorModel();
        if (httpMethod is not null)
        {
            selector.ActionConstraints.Add(new HttpMethodActionConstraint([httpMethod]));
        }

        action.Selectors.Add(selector);
        controller.Actions.Add(action);
        return action;
    }

    private static (string EntityType, SchemaOperation Operation) ReadFilterState(object filter)
    {
        Type filterType = typeof(SchemaLockdownFilter);
        var entityType = (string)filterType
            .GetField("_entityType", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(filter)!;
        var operation = (SchemaOperation)filterType
            .GetField("_operation", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(filter)!;
        return (entityType, operation);
    }

    [Test]
    public void No_EntityType_Attribute_Leaves_Filters_Empty()
    {
        ControllerModel controller = CreateControllerModel(typeof(NoEntityTypeController));
        ActionModel action = AddAction(
            controller,
            typeof(NoEntityTypeController).GetMethod(nameof(NoEntityTypeController.Post))!,
            HttpMethods.Post);

        CreateConvention().Apply(controller);

        Assert.That(action.Filters, Is.Empty);
    }

    [Test]
    public void Read_Action_Gets_No_Filter()
    {
        ControllerModel controller = CreateControllerModel(typeof(DocumentTypeController));
        ActionModel action = AddAction(
            controller,
            typeof(DocumentTypeController).GetMethod(nameof(DocumentTypeController.Get))!,
            HttpMethods.Get);

        CreateConvention().Apply(controller);

        Assert.That(action.Filters, Is.Empty);
    }

    [Test]
    public void Mutating_Action_Gets_Filter_With_Resolved_Entity_Type_And_Operation()
    {
        ControllerModel controller = CreateControllerModel(typeof(DocumentTypeController));
        ActionModel action = AddAction(
            controller,
            typeof(DocumentTypeController).GetMethod(nameof(DocumentTypeController.Post))!,
            HttpMethods.Post);

        CreateConvention().Apply(controller);

        Assert.That(action.Filters, Has.Count.EqualTo(1));

        var filter = action.Filters[0];
        Assert.That(filter, Is.TypeOf<SchemaLockdownFilter>());

        (string entityType, SchemaOperation operation) = ReadFilterState(filter);
        Assert.Multiple(() =>
        {
            Assert.That(entityType, Is.EqualTo(Constants.UdiEntityType.DocumentType));
            Assert.That(operation, Is.EqualTo(SchemaOperation.Create));
        });
    }

    [Test]
    public void Action_Without_A_Recognisable_Verb_Gets_Filter_Carrying_Unknown()
    {
        ControllerModel controller = CreateControllerModel(typeof(DocumentTypeController));
        ActionModel action = AddAction(
            controller,
            typeof(DocumentTypeController).GetMethod(nameof(DocumentTypeController.NoVerb))!,
            httpMethod: null);

        CreateConvention().Apply(controller);

        Assert.That(action.Filters, Has.Count.EqualTo(1));

        (string entityType, SchemaOperation operation) = ReadFilterState(action.Filters[0]);
        Assert.Multiple(() =>
        {
            Assert.That(entityType, Is.EqualTo(Constants.UdiEntityType.DocumentType));
            Assert.That(operation, Is.EqualTo(SchemaOperation.Unknown));
        });
    }

    [Test]
    public void Declared_Read_Operation_Overrides_Post_Verb_And_Skips_Filter()
    {
        ControllerModel controller = CreateControllerModel(typeof(DocumentTypeController));
        ActionModel action = AddAction(
            controller,
            typeof(DocumentTypeController).GetMethod(nameof(DocumentTypeController.PostButReadOnly))!,
            HttpMethods.Post);

        CreateConvention().Apply(controller);

        Assert.That(action.Filters, Is.Empty);
    }

    [Test]
    public void EntityType_Attribute_On_Base_Controller_Is_Visible_Through_Attributes_Alone()
    {
        // Justifies removing SchemaLockdownConvention's ControllerType.GetCustomAttributes fallback: this asserts
        // the primary controller.Attributes lookup already finds a base-class-only [EntityType] declaration,
        // built the same way DefaultApplicationModelProvider populates Attributes (GetCustomAttributes(inherit: true)).
        ControllerModel controller = CreateControllerModel(typeof(DerivedControllerWithoutOwnAttribute));

        EntityTypeAttribute? found = controller.Attributes.OfType<EntityTypeAttribute>().FirstOrDefault();

        Assert.That(found, Is.Not.Null);
        Assert.That(found!.EntityType, Is.EqualTo(Constants.UdiEntityType.MediaType));
    }

    [Test]
    public void EntityType_Declared_Only_On_Base_Controller_Still_Governs_Derived_Actions()
    {
        ControllerModel controller = CreateControllerModel(typeof(DerivedControllerWithoutOwnAttribute));
        ActionModel action = AddAction(
            controller,
            typeof(DerivedControllerWithoutOwnAttribute).GetMethod(nameof(DerivedControllerWithoutOwnAttribute.Post))!,
            HttpMethods.Post);

        CreateConvention().Apply(controller);

        Assert.That(action.Filters, Has.Count.EqualTo(1));
    }
}
