import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'True/false';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test TrueFalse';
let dataTypeData = null;

test.beforeEach(async ({umbracoApi}) => {
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

const trueFalseValues = [
  {type: 'true value ', value: true, expectedValue: 'True'},
  {type: 'false value', value: false, expectedValue: 'False'},
];

for (const trueFalse of trueFalseValues) {
  test(`can render content with ${trueFalse.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const templateId = await umbracoApi.template.createTemplateWithDisplayingStringValue(templateName, AliasHelper.toAlias(propertyName));
    const contentKey = await umbracoApi.document.createPublishedDocumentWithValue(contentName, trueFalse.value, dataTypeData.id, templateId, propertyName, documentTypeName);
    const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    await umbracoUi.contentRender.doesContentRenderValueContainText(trueFalse.expectedValue);
  });
}
