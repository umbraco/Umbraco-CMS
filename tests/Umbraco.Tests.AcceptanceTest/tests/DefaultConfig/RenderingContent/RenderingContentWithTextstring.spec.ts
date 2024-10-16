import {test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Textstring';
const templateName = 'TestTemplateForContent';
let dataTypeData;

test.beforeEach(async ({umbracoApi}) => {
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName); 
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName); 
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

const textstrings = [
  {type: 'an empty textstring', value: ''},
  {type: 'a non-empty textstring', value: 'Welcome to Umbraco site'},
  {type: 'a textstring contains special characters', value: '@#^&*()_+[]{};:"<>,./?'},
  {type: 'a numeric textstring', value: '0123456789'},
  {type: 'a textstring contains an SQL injection', value: "' OR '1'='1'; --"},
  {type: 'a textstring contains a cross-site scripting', value: "<script>alert('XSS')</script>"}
];

for (const textstring of textstrings) {
  test(`can render content with ${textstring.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const textstringValue = textstring.value;
    await umbracoApi.document.createPublishedDocumentWithValue(contentName, textstringValue, dataTypeData.id, documentTypeName, templateName);
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = contentData.urls[0].url;

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    await umbracoUi.contentRender.doesContentRenderValueHaveText(textstringValue);
  });
}

