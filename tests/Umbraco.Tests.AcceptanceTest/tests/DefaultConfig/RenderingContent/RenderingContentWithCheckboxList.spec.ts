import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom Checkbox List';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Checkbox List';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

const checkboxList = [
  {type: 'an empty list of checkboxes', value: []},
  {type: 'one checkbox', value: ['Test checkbox']},
  {type: 'multiple checkboxes', value: ['Test checkbox 1', 'Test checkbox 2', 'Test checkbox 3']},
];

for (const checkbox of checkboxList) {
  test(`can render content with ${checkbox.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const checkboxValue = checkbox.value;
    const dataTypeId = await umbracoApi.dataType.createCheckboxListDataType(customDataTypeName, checkboxValue);
    const templateId = await umbracoApi.template.createTemplateWithDisplayingMulitpleStringValue(templateName, AliasHelper.toAlias(propertyName));
    await umbracoApi.document.createPublishedDocumentWithValue(contentName, checkboxValue, dataTypeId, templateId, propertyName, documentTypeName);
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = contentData.urls[0].url;

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    checkboxValue.forEach(async value => {
      await umbracoUi.contentRender.doesContentRenderValueContainText(value);
    });
  });
}
