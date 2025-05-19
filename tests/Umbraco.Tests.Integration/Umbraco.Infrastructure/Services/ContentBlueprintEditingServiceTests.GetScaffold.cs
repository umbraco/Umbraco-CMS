using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentBlueprintEditingServiceTests
{
    public static void AddScaffoldedNotificationHandler(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<ContentScaffoldedNotification, ContentScaffoldedNotificationHandler>();

    [TestCase(true)]
    [TestCase(false)]
    [ConfigureBuilder(ActionName = nameof(AddScaffoldedNotificationHandler))]
    public async Task Can_Get_Scaffold(bool variant)
    {
        var blueprint = await (variant ? CreateVariantContentBlueprint() : CreateInvariantContentBlueprint());
        try
        {
            ContentScaffoldedNotificationHandler.ContentScaffolded = notification =>
            {
                foreach (var propertyValue in notification.Scaffold.Properties.SelectMany(property => property.Values))
                {
                    propertyValue.EditedValue += " scaffolded edited";
                    propertyValue.PublishedValue += " scaffolded published";
                }
            };
            var result = await ContentBlueprintEditingService.GetScaffoldedAsync(blueprint.Key);
            Assert.IsNotNull(result);
            Assert.AreEqual(blueprint.Key, result.Key);

            var propertyValues = result.Properties.SelectMany(property => property.Values).ToArray();
            Assert.IsNotEmpty(propertyValues);
            Assert.Multiple(() =>
            {
                Assert.IsTrue(propertyValues.All(value => value.EditedValue is string stringValue && stringValue.EndsWith(" scaffolded edited")));
                Assert.IsTrue(propertyValues.All(value => value.PublishedValue is string stringValue && stringValue.EndsWith(" scaffolded published")));
            });
        }
        finally
        {
            ContentScaffoldedNotificationHandler.ContentScaffolded = null;
        }
    }

    [Test]
    public async Task Cannot_Get_Non_Existing_Scaffold()
    {
        var result = await ContentBlueprintEditingService.GetScaffoldedAsync(Guid.NewGuid());
        Assert.IsNull(result);
    }

    public class ContentScaffoldedNotificationHandler : INotificationHandler<ContentScaffoldedNotification>
    {
        public static Action<ContentScaffoldedNotification>? ContentScaffolded { get; set; }

        public void Handle(ContentScaffoldedNotification notification) => ContentScaffolded?.Invoke(notification);
    }
}
