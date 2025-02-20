import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom Radiobox';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Radiobox';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName); 
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

const radioboxValues = [
  {type: 'an empty radiobox', value: ''},
  {type: 'a radiobox value', value: 'Test radiobox option'}
];

for (const radiobox of radioboxValues) {
  test(`can render content with ${radiobox.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeId = await umbracoApi.dataType.createRadioboxDataType(customDataTypeName, [radiobox.value]);
    const templateId = await umbracoApi.template.createTemplateWithDisplayingStringValue(templateName, AliasHelper.toAlias(propertyName));
    await umbracoApi.document.createPublishedDocumentWithValue(contentName, radiobox.value, dataTypeId, templateId, propertyName, documentTypeName);
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = contentData.urls[0].url;

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    await umbracoUi.contentRender.doesContentRenderValueContainText(radiobox.value);
  });
}