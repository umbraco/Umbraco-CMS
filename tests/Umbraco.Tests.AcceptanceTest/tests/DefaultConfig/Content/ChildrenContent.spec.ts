import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let documentTypeId = '';
let childDocumentTypeId = '';
let contentId = '';
const contentName = 'TestContent';
const childContentName = 'ChildContent';
const documentTypeName = 'DocumentTypeForContent';
const childDocumentTypeName = 'ChildDocumentType';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create child node', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(childDocumentTypeName);
  await umbracoUi.content.enterContentName(childContentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(childContentName)).toBeTruthy();
  const childData = await umbracoApi.document.getChildren(contentId);
  expect(childData[0].variants[0].name).toBe(childContentName);
  // verify that the child content displays in the tree after reloading children
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickReloadChildrenButton();
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.doesContentTreeHaveName(childContentName);

  // Clean
  await umbracoApi.document.ensureNameNotExists(childContentName);
});

test('can create child node in child node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childOfChildContentName = 'ChildOfChildContent';
  const childOfChildDocumentTypeName = 'ChildOfChildDocumentType';
  let childOfChildDocumentTypeId: any;
  let childContentId: any;
  await umbracoApi.documentType.ensureNameNotExists(childOfChildDocumentTypeName);
  childOfChildDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childOfChildDocumentTypeName);
  childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(childDocumentTypeName, childOfChildDocumentTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  childContentId = await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.clickActionsMenuForContent(childContentName);
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(childOfChildDocumentTypeName);
  // This wait is needed
  await umbracoUi.content.enterContentName(childOfChildContentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  const childOfChildData = await umbracoApi.document.getChildren(childContentId);
  expect(childOfChildData[0].variants[0].name).toBe(childOfChildContentName);
  // verify that the child content displays in the tree
  await umbracoUi.content.clickCaretButtonForContentName(childContentName);
  await umbracoUi.content.doesContentTreeHaveName(childOfChildContentName);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(childOfChildDocumentTypeName);
  await umbracoApi.document.ensureNameNotExists(childOfChildContentName);
});

test('cannot publish child if the parent is not published', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.clickActionsMenuForContent(childContentName);
  await umbracoUi.content.clickPublishButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(childContentName);
  expect(contentData.variants[0].state).toBe('Draft');
});

test('can publish with descendants', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childContentName, childDocumentTypeId, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickPublishButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  const childContentData = await umbracoApi.document.getByName(contentName);
  expect(childContentData.variants[0].state).toBe('Published');
});
