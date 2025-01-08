import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Numeric';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Numeric';
let dataTypeData = null;

test.beforeEach(async ({umbracoApi}) => {
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

const numerics = [
  {type: 'a positive integer', value: '1234567890'},
  {type: 'a negative integer', value: '-1234567890'},
];

for (const numeric of numerics) {
  test(`can render content with ${numeric.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const numericValue = numeric.value;
    const templateId = await umbracoApi.template.createTemplateWithDisplayingStringValue(templateName, AliasHelper.toAlias(propertyName));
    await umbracoApi.document.createPublishedDocumentWithValue(contentName, numericValue, dataTypeData.id, templateId, propertyName, documentTypeName);
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = contentData.urls[0].url;

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    await umbracoUi.contentRender.doesContentRenderValueContainText(numericValue);
  });
}
