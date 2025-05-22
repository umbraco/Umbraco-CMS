import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Multi URL Picker';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Member Picker';
let dataTypeData = null;

test.beforeEach(async ({umbracoApi}) => {
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can render content with document link value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create a document to link
  const documentTypeForLinkedDocumentName = 'TestDocumentType';
  const documentTypeForLinkedDocumentId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeForLinkedDocumentName);
  const linkedDocumentName = 'LinkedDocument';
  const linkedDocumentId = await umbracoApi.document.createDefaultDocument(linkedDocumentName, documentTypeForLinkedDocumentId);
  await umbracoApi.document.publish(linkedDocumentId);
  // Create a published document with document link value
  const templateId = await umbracoApi.template.createTemplateWithDisplayingMultiURLPickerValue(templateName, AliasHelper.toAlias(propertyName));
  const contentKey = await umbracoApi.document.createPublishedDocumentWithDocumentLinkURLPicker(contentName, linkedDocumentName, linkedDocumentId, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(linkedDocumentName);

  // Clean
  await umbracoApi.document.ensureNameNotExists(linkedDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeForLinkedDocumentName);
});

test('can render content with media link value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create a media to pick
  const mediaFileName = 'TestMediaFileForContent';
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
  const mediaFileId = await umbracoApi.media.createDefaultMediaWithImage(mediaFileName);
  // Create a published document with media link value
  const templateId = await umbracoApi.template.createTemplateWithDisplayingMultiURLPickerValue(templateName, AliasHelper.toAlias(propertyName));
  const contentKey = await umbracoApi.document.createPublishedDocumentWithImageLinkURLPicker(contentName, mediaFileName, mediaFileId, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(mediaFileName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
});

test('can render content with external link value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const linkUrl = 'https://docs.umbraco.com';
  const linkTitle = 'Umbraco Documentation';
  // Create a published document with external link value
  const templateId = await umbracoApi.template.createTemplateWithDisplayingMultiURLPickerValue(templateName, AliasHelper.toAlias(propertyName));
  const contentKey = await umbracoApi.document.createPublishedDocumentWithExternalLinkURLPicker(contentName, linkTitle, linkUrl, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(linkTitle);
});

test('can render content with multiple url value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const linkUrl = 'https://docs.umbraco.com';
  const linkTitle = 'Umbraco Documentation';
  // Create a media to pick
  const mediaFileName = 'TestMediaFileForContent';
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
  const mediaFileId = await umbracoApi.media.createDefaultMediaWithImage(mediaFileName);
  // Create a published document with external link value and image url value
  const templateId = await umbracoApi.template.createTemplateWithDisplayingMultiURLPickerValue(templateName, AliasHelper.toAlias(propertyName));
  const contentKey = await umbracoApi.document.createPublishedDocumentWithImageLinkAndExternalLink(contentName, mediaFileName, mediaFileId, linkTitle, linkUrl, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(linkTitle);
  await umbracoUi.contentRender.doesContentRenderValueContainText(mediaFileName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
});
