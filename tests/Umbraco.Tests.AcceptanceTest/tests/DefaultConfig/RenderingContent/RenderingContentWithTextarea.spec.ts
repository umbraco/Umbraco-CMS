import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Textarea';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Textarea';
let dataTypeData = null;

test.beforeEach(async ({umbracoApi}) => {
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

const textareas = [
  {type: 'an empty textarea', value: ''},
  {type: 'a non-empty textarea', value: 'Welcome to Umbraco site'},
  {type: 'a textarea that contains special characters', value: '@#^&*()_+[]{};:"<>,./?'},
  {type: 'a textarea that contains multiple lines', value: 'First line\n Second line\n Third line'},
  {type: 'a textarea that contains an SQL injection', value: "' OR '1'='1'; --"},
  {type: 'a textarea that contains cross-site scripting', value: "<script>alert('XSS')</script>"}
];

for (const textarea of textareas) {
  test(`can render content with ${textarea.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const textareaValue = textarea.value;
    const templateId = await umbracoApi.template.createTemplateWithDisplayingStringValue(templateName, AliasHelper.toAlias(propertyName));
    const contentKey = await umbracoApi.document.createPublishedDocumentWithValue(contentName, textareaValue, dataTypeData.id, templateId, propertyName, documentTypeName);
    const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    await umbracoUi.contentRender.doesContentRenderValueContainText(textareaValue);
  });
}
