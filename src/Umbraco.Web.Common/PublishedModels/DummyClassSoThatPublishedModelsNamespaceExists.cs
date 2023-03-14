namespace Umbraco.Cms.Web.Common.PublishedModels;

// this is here so that Umbraco.Web.PublishedModels namespace exists in views
// even if people are not using models at all - because we are referencing it
// when compiling views - hopefully noone will ever create an actual model
// with that name
internal class DummyClassSoThatPublishedModelsNamespaceExists
{
}
