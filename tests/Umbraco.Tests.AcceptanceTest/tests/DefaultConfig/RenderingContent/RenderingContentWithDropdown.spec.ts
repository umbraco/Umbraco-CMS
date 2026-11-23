import {AliasHelper, test} from '@umbraco/acceptance-test-helpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom Dropdown';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Dropdown';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

const dropdownValues = [
  {type: 'an empty dropdown list', options: [], isMultiple: false},
  {type: 'a single dropdown value', options: [{value: 'Test checkbox', label: 'Test checkbox label'}], isMultiple: false},
  {type: 'multiple dropdown values', options: [{value: 'Test option 1', label: 'Test option 1 label'}, {value: 'Test option 2', label: 'Test option 2 label'}], isMultiple: true}
];

for (const dropdown of dropdownValues) {
  test(`can render content with ${dropdown.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeId = await umbracoApi.dataType.createDropdownDataType(customDataTypeName, dropdown.isMultiple, dropdown.options);
    let templateId = '';
    if (dropdown.isMultiple) {
      templateId = await umbracoApi.template.createTemplateWithDisplayingMulitpleStringValue(templateName, AliasHelper.toAlias(propertyName));
    } else {
      templateId = await umbracoApi.template.createTemplateWithDisplayingStringValue(templateName, AliasHelper.toAlias(propertyName));
    }
    const contentKey = await umbracoApi.document.createPublishedDocumentWithValue(contentName, dropdown.options.map(option => option.value), dataTypeId, templateId, propertyName, documentTypeName);
    const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    for (const option of dropdown.options) {
      await umbracoUi.contentRender.doesContentRenderValueContainText(option.value);
    }
  });
}
