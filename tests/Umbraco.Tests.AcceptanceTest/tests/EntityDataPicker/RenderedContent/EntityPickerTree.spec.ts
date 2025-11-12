import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const templateName = 'EntityPickerTreeTemplate';
const dataTypeName = 'EntityPickerWithTree';
const propertyName = 'TestProperty';
const treeDataSourceAlias = 'My.PickerDataSource.Tree';

// Ids for Example 4 and Example 2
const items = {ids: ['4', '2']};

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('can render content with an entity picker using the tree data source', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeId = await umbracoApi.dataType.createEntityDataPickerDataType(dataTypeName, treeDataSourceAlias);
  const templateId = await umbracoApi.template.createTemplateWithEntityDataPickerValue(templateName, propertyName);
  const contentKey = await umbracoApi.document.createPublishedDocumentWithValue(contentName, items, dataTypeId, templateId, propertyName, documentTypeName);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentKey);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesDataSourceRenderValueHaveText(treeDataSourceAlias);
  for (const value of items.ids) {
    await umbracoUi.contentRender.doesContentRenderValueContainText(value);
  }
});
