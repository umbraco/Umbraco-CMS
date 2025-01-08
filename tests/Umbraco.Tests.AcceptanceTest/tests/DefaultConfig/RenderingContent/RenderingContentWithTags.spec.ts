import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'Test Rendering Content';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Tags';
const templateName = 'TestTemplateForContent';
const propertyName = 'Test Tags';
let dataTypeData = null;

test.beforeEach(async ({umbracoApi}) => {
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

const tags = [
  {type: 'an empty tag', value: []},
  {type: 'a non-empty tag', value: ['test tag']},
  {type: 'multiple tags', value: ['test tag 1', 'test tag 2']},
];

for (const tag of tags) {
  test(`can render content with ${tag.type}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const tagValue = tag.value;
    const templateId = await umbracoApi.template.createTemplateWithDisplayingMulitpleStringValue(templateName, AliasHelper.toAlias(propertyName));
    await umbracoApi.document.createPublishedDocumentWithValue(contentName, tagValue, dataTypeData.id, templateId, propertyName, documentTypeName);
    const contentData = await umbracoApi.document.getByName(contentName);
    const contentURL = contentData.urls[0].url;

    // Act
    await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

    // Assert
    tagValue.forEach(async value => {
      await umbracoUi.contentRender.doesContentRenderValueContainText(value);
    });
  });
}
