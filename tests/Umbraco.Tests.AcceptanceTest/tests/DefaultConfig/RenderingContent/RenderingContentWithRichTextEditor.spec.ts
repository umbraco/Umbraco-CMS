import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Richtext editor';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Rich Text Editor';
let dataTypeData = null;

test.beforeEach(async ({umbracoApi}) => {
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can render content with rich text editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const richTextValue = {markup: '<p>Welcome to Umbraco Rich Text Editor</p>'};
  const expectedRenderedText = 'Welcome to Umbraco Rich Text Editor';
  const templateId = await umbracoApi.template.createTemplateWithDisplayingStringValue(templateName, AliasHelper.toAlias(propertyName));
  const contentKey = await umbracoApi.document.createPublishedDocumentWithValue(contentName, richTextValue, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(expectedRenderedText);
});

// This tests for regression issue: https://github.com/umbraco/Umbraco-CMS/issues/20454
test('can render default value for empty rich text editor', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const defaultValue = 'This is default value of RTE';
  const templateId = await umbracoApi.template.createTemplateWithDisplayingDefaultValue(templateName, AliasHelper.toAlias(propertyName), defaultValue);
  const contentKey = await umbracoApi.document.createPublishedDocumentWithValue(contentName, null, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(defaultValue);
});
