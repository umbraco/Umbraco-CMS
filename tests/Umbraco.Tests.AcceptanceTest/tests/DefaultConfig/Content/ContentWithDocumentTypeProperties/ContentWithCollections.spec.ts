import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
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
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isTabNameVisible('Collection');
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
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(childDocumentTypeName);
  await umbracoUi.content.enterContentName(firstChildContentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  const childData = await umbracoApi.document.getChildren(contentId);
  expect(childData.length).toBe(expectedNames.length);
  expect(childData[0].variants[0].name).toBe(firstChildContentName);
  // verify that the child content displays in collection list after reloading tree
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickReloadButton();
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesDocumentTableColumnNameValuesMatch(expectedNames);

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
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(childDocumentTypeName);
  await umbracoUi.content.enterContentName(secondChildContentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  const childData = await umbracoApi.document.getChildren(contentId);
  expect(childData.length).toBe(expectedNames.length);
  expect(childData[0].variants[0].name).toBe(firstChildContentName);
  expect(childData[1].variants[0].name).toBe(secondChildContentName);
  // verify that the child content displays in collection list after reloading tree
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickReloadButton();
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesDocumentTableColumnNameValuesMatch(expectedNames);

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstChildContentName);
  await umbracoApi.document.ensureNameNotExists(secondChildContentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can search in a collection of content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const searchKeyword = 'First';
  const expectedSearchResult = [firstChildContentName];
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
  await umbracoUi.content.doesDocumentTableColumnNameValuesMatch(expectedSearchResult);

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstChildContentName);
  await umbracoApi.document.ensureNameNotExists(secondChildContentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});
