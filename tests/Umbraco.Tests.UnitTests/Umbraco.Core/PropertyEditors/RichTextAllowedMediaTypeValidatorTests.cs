using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
internal class RichTextAllowedMediaTypeValidatorTests
{
    private static readonly Guid AllowedTypeKey = Guid.NewGuid();

    [TestCase(null)]
    [TestCase("")]
    public void No_Filter_Configured_Allows_All(string? allowedMediaTypes)
    {
        var (validator, _, _) = CreateValidator();

        var result = Validate(validator, BuildMarkup(Guid.NewGuid()), allowedMediaTypes);

        Assert.IsEmpty(result);
    }

    [Test]
    public void No_Media_References_Passes()
    {
        var (validator, _, _) = CreateValidator();

        var result = Validate(validator, "<p>No images here</p>");

        Assert.IsEmpty(result);
    }

    [Test]
    public void Allowed_Media_Type_Passes()
    {
        var (validator, mediaServiceMock, mediaTypeServiceMock) = CreateValidator();
        SetupMedia(mediaServiceMock, mediaTypeServiceMock, "Image", AllowedTypeKey);

        var result = Validate(validator, BuildMarkup(Guid.NewGuid()));

        Assert.IsEmpty(result);
    }

    [Test]
    public void Disallowed_Media_Type_Fails()
    {
        var (validator, mediaServiceMock, mediaTypeServiceMock) = CreateValidator();
        SetupMedia(mediaServiceMock, mediaTypeServiceMock, "File", Guid.NewGuid());

        var result = Validate(validator, BuildMarkup(Guid.NewGuid()));

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void One_Disallowed_Among_Multiple_Fails()
    {
        var (validator, mediaServiceMock, mediaTypeServiceMock) = CreateValidator();

        var allowed = CreateMediaMock("Image");
        var disallowed = CreateMediaMock("File");
        mediaServiceMock.Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>()))
            .Returns([allowed.Object, disallowed.Object]);
        SetupMediaType(mediaTypeServiceMock, "Image", AllowedTypeKey);
        SetupMediaType(mediaTypeServiceMock, "File", Guid.NewGuid());

        var result = Validate(validator, BuildMarkup(Guid.NewGuid()) + BuildMarkup(Guid.NewGuid()));

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Media_Not_Found_Passes()
    {
        var (validator, mediaServiceMock, _) = CreateValidator();
        mediaServiceMock.Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>())).Returns([]);

        var result = Validate(validator, BuildMarkup(Guid.NewGuid()));

        Assert.IsEmpty(result);
    }

    [Test]
    public void Media_Type_Not_Found_Fails()
    {
        var (validator, mediaServiceMock, mediaTypeServiceMock) = CreateValidator();
        var media = CreateMediaMock("Unknown");
        mediaServiceMock.Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>())).Returns([media.Object]);
        mediaTypeServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns((IMediaType?)null);

        var result = Validate(validator, BuildMarkup(Guid.NewGuid()));

        Assert.That(result.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Multiple_Allowed_Types_In_Config()
    {
        var secondTypeKey = Guid.NewGuid();
        var (validator, mediaServiceMock, mediaTypeServiceMock) = CreateValidator();
        SetupMedia(mediaServiceMock, mediaTypeServiceMock, "Video", secondTypeKey);

        var result = Validate(validator, BuildMarkup(Guid.NewGuid()), $"{AllowedTypeKey},{secondTypeKey}");

        Assert.IsEmpty(result);
    }

    [Test]
    public void Allowed_Type_Key_Comparison_Is_Case_Insensitive()
    {
        var (validator, mediaServiceMock, mediaTypeServiceMock) = CreateValidator();
        SetupMedia(mediaServiceMock, mediaTypeServiceMock, "Image", AllowedTypeKey);

        var result = Validate(validator, BuildMarkup(Guid.NewGuid()), AllowedTypeKey.ToString().ToUpperInvariant());

        Assert.IsEmpty(result);
    }

    private static IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(
        RichTextAllowedMediaTypeValidator validator,
        string markup,
        string? allowedMediaTypes = "USE_DEFAULT") =>
        validator.Validate(
            $$"""{"markup":"{{markup.Replace("\"", "\\\"")}}","blocks":null}""",
            null,
            new RichTextConfiguration { AllowedMediaTypes = allowedMediaTypes == "USE_DEFAULT" ? AllowedTypeKey.ToString() : allowedMediaTypes },
            PropertyValidationContext.Empty());

    private static string BuildMarkup(Guid mediaKey) =>
        $"""<img src="/media/image.jpg" data-udi="umb://media/{mediaKey:N}" />""";

    private static Mock<IMedia> CreateMediaMock(string typeAlias)
    {
        var mock = new Mock<IMedia>();
        mock.SetupGet(x => x.ContentType.Alias).Returns(typeAlias);
        return mock;
    }

    private static void SetupMediaType(Mock<IMediaTypeService> mock, string alias, Guid key)
    {
        var mediaType = new Mock<IMediaType>();
        mediaType.Setup(x => x.Key).Returns(key);
        mock.Setup(x => x.Get(alias)).Returns(mediaType.Object);
    }

    private static void SetupMedia(Mock<IMediaService> mediaServiceMock, Mock<IMediaTypeService> mediaTypeServiceMock, string typeAlias, Guid typeKey)
    {
        var media = CreateMediaMock(typeAlias);
        mediaServiceMock.Setup(x => x.GetByIds(It.IsAny<IEnumerable<Guid>>())).Returns([media.Object]);
        SetupMediaType(mediaTypeServiceMock, typeAlias, typeKey);
    }

    private static (RichTextAllowedMediaTypeValidator Validator, Mock<IMediaService> MediaServiceMock, Mock<IMediaTypeService> MediaTypeServiceMock) CreateValidator()
    {
        var mediaServiceMock = new Mock<IMediaService>();
        var mediaTypeServiceMock = new Mock<IMediaTypeService>();

        var validator = new RichTextAllowedMediaTypeValidator(
            new HtmlImageSourceParser(_ => string.Empty),
            mediaServiceMock.Object,
            Mock.Of<ILocalizedTextService>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<Microsoft.Extensions.Logging.ILogger>(),
            new AllowedMediaTypeHelper(mediaTypeServiceMock.Object, AppCaches.Disabled));

        return (validator, mediaServiceMock, mediaTypeServiceMock);
    }
}
