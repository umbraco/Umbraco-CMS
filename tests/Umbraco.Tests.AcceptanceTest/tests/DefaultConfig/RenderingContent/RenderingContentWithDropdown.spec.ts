import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

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
  {type: 'an empty dropdown list', value: [], isMultiple: false},
  {type: 'a single dropdown value', value: ['Test checkbox'], isMultiple: false},
  {type: 'multiple dropdown values', value: ['Test option 1', 'Test option 2'], isMultiple: true}
];

for (const dropdown of dropdownValues) {
  test(`can render content with ${dropdown.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeId = await umbracoApi.dataType.createDropdownDataType(customDataTypeName, dropdown.isMultiple, dropdown.value);
    let templateId = '';
    if (dropdown.isMultiple) {
      templateId = await umbracoApi.template.createTemplateWithDisplayingMulitpleStringValue(templateName, AliasHelper.toAlias(propertyName));
    } else {
      templateId = await umbracoApi.template.createTemplateWithDisplayingStringValue(templateName, AliasHelper.toAlias(propertyName));
    }  
    await umbracoApi.document.createPublishedDocumentWithValue(contentName, dropdown.value, dataTypeId, templateId, propertyName, documentTypeName);
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = contentData.urls[0].url;

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    dropdown.value.forEach(async value => {
      await umbracoUi.contentRender.doesContentRenderValueContainText(value);
    }); 
  });
}