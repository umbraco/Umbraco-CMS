import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'EntityPickerWithTree';
const treeDataSourceAlias = 'My.PickerDataSource.Tree';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('can create empty content with an entity picker using the tree data source', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const dataTypeId = await umbracoApi.dataType.createEntityDataPickerDataType(dataTypeName, treeDataSourceAlias);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  await umbracoApi.document.doesVariantHaveState(contentData, expectedState);
  await umbracoApi.document.doesHaveValueCount(contentData, 0);
});

test('can create content with an entity picker using the tree data source that has an item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const dataTypeId = await umbracoApi.dataType.createEntityDataPickerDataType(dataTypeName, treeDataSourceAlias);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.chooseTreeMenuItemWithName('Example 1');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(umbracoApi.document.getOnlyPropertyValue(contentData).ids[0]).toEqual('1');
});

test('can create content with an entity picker using the tree data source that has multiple items', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const dataTypeId = await umbracoApi.dataType.createEntityDataPickerDataType(dataTypeName, treeDataSourceAlias);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.chooseTreeMenuItemWithName('Example 1');
  await umbracoUi.content.chooseTreeMenuItemWithName('Example 3');
  await umbracoUi.content.chooseTreeMenuItemWithName('Example 5', ['Example Folder 1']);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(umbracoApi.document.getOnlyPropertyValue(contentData).ids[0]).toEqual('1');
  expect(umbracoApi.document.getOnlyPropertyValue(contentData).ids[1]).toEqual('3');
  expect(umbracoApi.document.getOnlyPropertyValue(contentData).ids[2]).toEqual('5');
});

test('can not create content with an entity picker using the tree data source that has more items than max amount', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeId = await umbracoApi.dataType.createEntityDataPickerDataTypeWithMinAndMaxValues(dataTypeName, treeDataSourceAlias, 0, 2);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.chooseTreeMenuItemWithName('Example 1');
  await umbracoUi.content.isChooseButtonVisible(true);
  await umbracoUi.content.chooseTreeMenuItemWithName('Example 5', ['Example Folder 1']);

  // Assert
  // The choose button should be disabled when the max amount is reached
  await umbracoUi.content.isChooseButtonVisible(false);
});

test.skip('can not create content with an entity picker using the tree data source that has less items than min amount', {annotation: {type: 'issue', description: "Skip this test due to this issue: https://github.com/umbraco/Umbraco-CMS/issues/22121"}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const dataTypeId = await umbracoApi.dataType.createEntityDataPickerDataTypeWithMinAndMaxValues(dataTypeName, treeDataSourceAlias, 2, 5);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.chooseTreeMenuItemWithName('Example 1');
  await umbracoUi.content.isTextWithExactNameVisible('This field need more items');
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  await umbracoUi.content.chooseTreeMenuItemWithName('Example 5', ['Example Folder 1']);

  // Assert
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();
  const contentData = await umbracoApi.document.getByName(contentName);
  await umbracoApi.document.doesVariantHaveState(contentData, expectedState);
});
