import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Content Picker';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Content Picker';
const contentPickerDocumentTypeName = 'DocumentTypeForContentPicker';
const contentPickerName = 'TestContentPickerName';
let dataTypeData = null;
let contentPickerDocumentTypeId = '';
let contentPickerId = '';

test.beforeEach(async ({umbracoApi}) => {
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  contentPickerDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(contentPickerDocumentTypeName);
  contentPickerId = await umbracoApi.document.createDefaultDocument(contentPickerName, contentPickerDocumentTypeId);
  await umbracoApi.document.publish(contentPickerId);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName); 
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentPickerName); 
  await umbracoApi.documentType.ensureNameNotExists(contentPickerDocumentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can render content with content picker value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const templateId = await umbracoApi.template.createTemplateWithDisplayingContentPickerValue(templateName, AliasHelper.toAlias(propertyName));
  await umbracoApi.document.createPublishedDocumentWithValue(contentName, contentPickerId, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentData = await umbracoApi.document.getByName(contentName);
  const contentURL = contentData.urls[0].url;

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(contentPickerName);
});
