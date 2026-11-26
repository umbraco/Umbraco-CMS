import {AliasHelper, test} from '@umbraco/acceptance-test-helpers';

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
  {type: 'an empty list of checkboxes', options: []},
  {type: 'one checkbox', options: [{value: 'Test checkbox', label: 'Test checkbox label'}]},
  {type: 'multiple checkboxes', options: [{value: 'Test checkbox 1', label: 'Test checkbox 1 label'}, {value: 'Test checkbox 2', label: 'Test checkbox 2 label'}, {value: 'Test checkbox 3', label: 'Test checkbox 3 label'}]},
];

for (const checkbox of checkboxList) {
  test(`can render content with ${checkbox.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeId = await umbracoApi.dataType.createCheckboxListDataType(customDataTypeName, checkbox.options);
    const templateId = await umbracoApi.template.createTemplateWithDisplayingMulitpleStringValue(templateName, AliasHelper.toAlias(propertyName));
    const contentKey = await umbracoApi.document.createPublishedDocumentWithValue(contentName, checkbox.options.map(option => option.value), dataTypeId, templateId, propertyName, documentTypeName);
    const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    for (const option of checkbox.options) {
      await umbracoUi.contentRender.doesContentRenderValueContainText(option.value);
    }
  });
}
