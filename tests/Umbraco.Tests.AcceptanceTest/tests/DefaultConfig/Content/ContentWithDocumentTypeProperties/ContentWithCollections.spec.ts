import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const childDocumentTypeName = 'TestChildDocumentType';
const firstChildContentName = 'First Child Content';
const secondChildContentName = 'Second Child Content';
const dataTypeName = 'List View - Content';
let dataTypeData;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  dataTypeData  = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content configured as a collection', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDocumentTypeWithCollectionId(documentTypeName, dataTypeData.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  await umbracoUi.content.isTabNameVisible('Child items');
  await umbracoUi.content.doesContentListHaveNoItemsInList();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
});

test('can create child content in a collection', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedNames = [firstChildContentName];
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, dataTypeData.id);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(childDocumentTypeName);
  await umbracoUi.content.enterContentName(firstChildContentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  const childData = await umbracoApi.document.getChildren(contentId);
  expect(childData.length).toBe(expectedNames.length);
  expect(childData[0].variants[0].name).toBe(firstChildContentName);
  // verify that the child content displays in collection grid after reloading tree
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickReloadChildrenActionMenuOption();
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.isContentWithNameVisibleInGrid(expectedNames[0]);

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstChildContentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can create multiple child nodes in a collection', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedNames = [secondChildContentName, firstChildContentName];
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, dataTypeData.id);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(firstChildContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(childDocumentTypeName);
  await umbracoUi.content.enterContentName(secondChildContentName);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  const childData = await umbracoApi.document.getChildren(contentId);
  expect(childData.length).toBe(expectedNames.length);
  expect(childData[0].variants[0].name).toBe(firstChildContentName);
  expect(childData[1].variants[0].name).toBe(secondChildContentName);
  // verify that the child content displays in collection grid after reloading tree
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickReloadChildrenActionMenuOption();
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.isContentWithNameVisibleInGrid(expectedNames[0]);
  await umbracoUi.content.isContentWithNameVisibleInGrid(expectedNames[1]);

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstChildContentName);
  await umbracoApi.document.ensureNameNotExists(secondChildContentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can search in a collection of content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const searchKeyword = 'First';
  const expectedSearchResult = firstChildContentName;
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, dataTypeData.id);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(firstChildContentName, childDocumentTypeId, contentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondChildContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.searchByKeywordInCollection(searchKeyword);

  // Assert
  await umbracoUi.content.isContentWithNameVisibleInGrid(expectedSearchResult);

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstChildContentName);
  await umbracoApi.document.ensureNameNotExists(secondChildContentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can open a child from the grid view', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, dataTypeData.id);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const childDocumentId = await umbracoApi.document.createDefaultDocumentWithParent(firstChildContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.isDocumentGridViewVisible();

  // Act
  // An early click can be lost while the collection re-renders, leaving the workspace unopened.
  // Retry until the edit route is actually loaded.
  await expect(async () => {
    await umbracoUi.content.clickContentCardWithName(firstChildContentName);
    await umbracoUi.content.waitForWorkspaceEditRoute('document');
  }).toPass({timeout: ConstantHelper.timeout.veryLong});

  // Assert
  // The click must have opened the child's own edit workspace, not just any document route.
  await expect(umbracoUi.content.page).toHaveURL(new RegExp(childDocumentId));

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstChildContentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can navigate into a nested collection', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const grandChildDocumentTypeName = 'TestGrandChildDocumentType';
  const grandChildContentName = 'Grandchild Content';
  const grandChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(grandChildDocumentTypeName);
  // The child document type is itself configured as a collection, so opening it from the parent's
  // grid must land on its own collection view rather than a plain workspace.
  const childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(childDocumentTypeName, grandChildDocumentTypeId, dataTypeData.id);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, dataTypeData.id);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const childContentId = await umbracoApi.document.createDefaultDocumentWithParent(firstChildContentName, childDocumentTypeId, contentId);
  await umbracoApi.document.createDefaultDocumentWithParent(grandChildContentName, grandChildDocumentTypeId, childContentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.isDocumentGridViewVisible();

  // Act
  // An early click can be lost while the collection re-renders, leaving the workspace unopened.
  // Retry until the edit route is actually loaded.
  await expect(async () => {
    await umbracoUi.content.clickContentCardWithName(firstChildContentName);
    await umbracoUi.content.waitForWorkspaceEditRoute('document');
  }).toPass({timeout: ConstantHelper.timeout.veryLong});

  // Assert
  // The click must have opened the nested collection's own edit workspace, not just any document route.
  await expect(umbracoUi.content.page).toHaveURL(new RegExp(childContentId));
  await umbracoUi.content.isDocumentGridViewVisible();
  await umbracoUi.content.isContentWithNameVisibleInGrid(grandChildContentName);

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstChildContentName);
  await umbracoApi.document.ensureNameNotExists(grandChildContentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(grandChildDocumentTypeName);
});

test('can open a child from the table view', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNodeAndCollectionId(documentTypeName, childDocumentTypeId, dataTypeData.id);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const childDocumentId = await umbracoApi.document.createDefaultDocumentWithParent(firstChildContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.changeToListView();
  await umbracoUi.content.isDocumentListViewVisible();

  // Act
  // An early click can be lost while the table re-renders, leaving the workspace unopened.
  // Retry until the edit route is actually loaded.
  await expect(async () => {
    await umbracoUi.content.goToContentInListViewWithName(firstChildContentName);
    await umbracoUi.content.waitForWorkspaceEditRoute('document');
  }).toPass({timeout: ConstantHelper.timeout.veryLong});

  // Assert
  // The click must have opened the child's own edit workspace, not just any document route.
  await expect(umbracoUi.content.page).toHaveURL(new RegExp(childDocumentId));

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstChildContentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});
