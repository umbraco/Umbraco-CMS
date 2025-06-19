import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom Approved Color';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Approved Color';
const colorValue = {label: "Test Label", value: "038c33"};
let dataTypeId = null;

test.beforeEach(async ({umbracoApi}) => {
  dataTypeId = await umbracoApi.dataType.createApprovedColorDataTypeWithOneItem(customDataTypeName, colorValue.label, colorValue.value);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can render content with an approved color with label', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const templateId = await umbracoApi.template.createTemplateWithDisplayingApprovedColorValue(templateName, AliasHelper.toAlias(propertyName));
  const contentKey = await umbracoApi.document.createPublishedDocumentWithValue(contentName, colorValue, dataTypeId, templateId, propertyName, documentTypeName);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(colorValue.label);
});

test('can render content with an approved color without label', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const templateId = await umbracoApi.template.createTemplateWithDisplayingApprovedColorValue(templateName, AliasHelper.toAlias(propertyName), false);
  const contentKey = await umbracoApi.document.createPublishedDocumentWithValue(contentName, colorValue, dataTypeId, templateId, propertyName, documentTypeName);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(colorValue.value);
});
