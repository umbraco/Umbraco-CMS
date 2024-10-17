import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Date Picker';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName); 
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can render content with a date without time', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dateValue = '2024-10-29 00:00:00';
  const dataTypeName = 'Date Picker';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName); 
  const templateId = await umbracoApi.template.createTemplateWithDisplayingStringValue(templateName, AliasHelper.toAlias(propertyName));
  await umbracoApi.document.createPublishedDocumentWithValue(contentName, dateValue, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentData = await umbracoApi.document.getByName(contentName);
  const contentURL = contentData.urls[0].url;

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueHaveText(dateValue);
});

test('can render content with a date with time', async ({umbracoApi, umbracoUi}) => {
  const dateValue = '2024-10-29 09:09:09 PM';
  const dataTypeName = 'Date Picker';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName); 
  const templateId = await umbracoApi.template.createTemplateWithDisplayingStringValue(templateName, AliasHelper.toAlias(propertyName));
  await umbracoApi.document.createPublishedDocumentWithValue(contentName, dateValue, dataTypeData.id, templateId, propertyName, documentTypeName);
  const contentData = await umbracoApi.document.getByName(contentName);
  const contentURL = contentData.urls[0].url;

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueHaveText(dateValue);
});


