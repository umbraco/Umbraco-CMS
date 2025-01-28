import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content with allowed child node enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeName = 'Test Child Document Type';
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
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
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('cannot create child content if allowed child node is disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const noAllowedDocumentTypeAvailableMessage = 'There are no allowed Document Types available for creating content here';
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCreateButton();

  // Assert
  await umbracoUi.content.isDocumentTypeNameVisible(documentTypeName, false);
  await umbracoUi.content.doesModalHaveText(noAllowedDocumentTypeAvailableMessage);
});

test('can create multiple child nodes with different document types', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstChildDocumentTypeName = 'First Child Document Type';
  const secondChildDocumentTypeName = 'Second Child Document Type';
  const firstChildContentName = 'First Child Content';
  const secondChildContentName = 'Second Child Content';
  const firstChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(firstChildDocumentTypeName);
  const secondChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(secondChildDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedTwoChildNodes(documentTypeName, firstChildDocumentTypeId, secondChildDocumentTypeId);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(firstChildContentName, firstChildDocumentTypeId, contentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(secondChildDocumentTypeName);
  await umbracoUi.content.enterContentName(secondChildContentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(secondChildContentName)).toBeTruthy();
  const childData = await umbracoApi.document.getChildren(contentId);
  expect(childData.length).toBe(2);
  expect(childData[0].variants[0].name).toBe(firstChildContentName);
  expect(childData[1].variants[0].name).toBe(secondChildContentName);
  // verify that the child content displays in the tree after reloading children
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickReloadButton();
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.doesContentTreeHaveName(firstChildContentName);
  await umbracoUi.content.doesContentTreeHaveName(secondChildContentName);

  // Clean
  await umbracoApi.document.ensureNameNotExists(firstChildContentName);
  await umbracoApi.document.ensureNameNotExists(secondChildContentName);
  await umbracoApi.documentType.ensureNameNotExists(firstChildDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondChildDocumentTypeName);
});
