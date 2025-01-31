import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Multiple Media Picker';
const firstMediaFileName = 'TestFirstMedia';
const secondMediaFileName = 'TestSecondMedia';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.media.ensureNameNotExists(firstMediaFileName);
  await umbracoApi.media.ensureNameNotExists(secondMediaFileName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.media.ensureNameNotExists(firstMediaFileName);
  await umbracoApi.media.ensureNameNotExists(secondMediaFileName);
});

test('can render content with multiple media picker value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeName = 'Multiple Media Picker';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  // Create multiple images
  const firstMediaFileId = await umbracoApi.media.createDefaultMediaFile(firstMediaFileName);
  const secondMediaFileId = await umbracoApi.media.createDefaultMediaWithArticle(secondMediaFileName);
  // Create a published document with multiple media picker value
  const templateId = await umbracoApi.template.createTemplateWithDisplayingMultipleMediaPickerValue(templateName, AliasHelper.toAlias(propertyName));
  await umbracoApi.document.createPublishedDocumentWithTwoMediaPicker(contentName, firstMediaFileId, secondMediaFileId, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentData = await umbracoApi.document.getByName(contentName);
  const contentURL = contentData.urls[0].url;

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(firstMediaFileName);
  await umbracoUi.contentRender.doesContentRenderValueContainText(secondMediaFileName);
});

test('can render content with multiple image media picker value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeName = 'Multiple Image Media Picker';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  // Create multiple images
  const firstMediaFileId = await umbracoApi.media.createDefaultMediaWithImage(firstMediaFileName);
  const secondMediaFileId = await umbracoApi.media.createDefaultMediaWithImage(secondMediaFileName);
  // Create a published document with multiple image media picker value
  const templateId = await umbracoApi.template.createTemplateWithDisplayingMultipleMediaPickerValue(templateName, AliasHelper.toAlias(propertyName));
  await umbracoApi.document.createPublishedDocumentWithTwoMediaPicker(contentName, firstMediaFileId, secondMediaFileId, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentData = await umbracoApi.document.getByName(contentName);
  const contentURL = contentData.urls[0].url;

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(firstMediaFileName);
  await umbracoUi.contentRender.doesContentRenderValueContainText(secondMediaFileName);
});