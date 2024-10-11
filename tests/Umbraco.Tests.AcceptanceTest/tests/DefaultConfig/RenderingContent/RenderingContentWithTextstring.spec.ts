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

const textStrings = [
  {type: 'an empty textstring', value: ''},
  {type: 'a non-empty textstring', value: 'Welcome to Umbraco site'},
  {type: 'a textstring contains special characters', value: '@#^&*()_+[]{};:"<>,./?'},
  {type: 'a numeric textstring', value: '0123456789'},
  {type: 'a textstring contains an SQL injection', value: "' OR '1'='1'; --"},
  {type: 'a textstring contains a cross-site scripting', value: "<script>alert('XSS')</script>"}
];

for (const textString of textStrings) {
  test(`can render content with ${textString.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const textStringValue = textString.value;
    await umbracoApi.document.createPublishedDocumentWithValue(contentName, textStringValue, dataTypeData.id, templateName);
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = contentData.urls[0].url;

    // Act
    await umbracoUi.contentRender.goToContentRenderPage(contentURL);

    // Assert
    await umbracoUi.contentRender.doesContentRenderValueHaveText(textStringValue);
  });
}

