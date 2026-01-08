import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Content
const parentContentName = 'parentContentName';
// DocumentType
const documentTypeParentName = 'ParentDocumentType';
const documentTypeChildName = 'ChildDocumentType';
// DataType
const customDataTypeName = 'Custom List View';
const layoutName = 'My Collection View Table';
const layoutCollectionView = 'My.CollectionView.Document.Table';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(parentContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeParentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeChildName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can see the custom collection view when choosing layout for new collection data type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.createListViewContentDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);
  
  // Act
  await umbracoUi.dataType.addLayouts(layoutName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesListViewHaveLayout(customDataTypeName, layoutName, 'icon-list', layoutCollectionView)).toBeTruthy();
});

test('can see the pagination works when using custom collection view in content section', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const pageSize = 5;
  const totalItems = 7;
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  const dataTypeId = await umbracoApi.dataType.createListViewContentDataTypeWithLayoutAndPageSize(customDataTypeName,layoutCollectionView, layoutName, pageSize);
  const documentTypeChildId = await umbracoApi.documentType.createDefaultDocumentType(documentTypeChildName);
  const documentTypeParentId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeParentName, documentTypeChildId, dataTypeId);
  const documentParentId = await umbracoApi.document.createDefaultDocument(parentContentName, documentTypeParentId);
  for (let i = 1; i <= totalItems; i++) {
    await umbracoApi.document.createDefaultDocumentWithParent('Test child ' + i, documentTypeChildId, documentParentId);
  }
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(parentContentName);

  // Assert
  // Page 1
  await umbracoUi.content.doesListViewItemsHaveCount(pageSize);
  await umbracoUi.content.isListViewItemWithNameVisible('Test child 1', 0);
  await umbracoUi.content.isListViewItemWithNameVisible('Test child 5', 4);
  // Page 2
  await umbracoUi.content.clickPaginationNextButton();
  await umbracoUi.content.doesListViewItemsHaveCount(2);
  await umbracoUi.content.isListViewItemWithNameVisible('Test child 6', 0);
  await umbracoUi.content.isListViewItemWithNameVisible('Test child 7', 1);

  // Clean
  for (let i = 1; i <= totalItems; i++) {
    await umbracoApi.document.ensureNameNotExists('Test child ' + i);
  }
});