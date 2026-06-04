import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const parentDocumentTypeName = 'ParentDocumentType';
const childDocumentTypeName = 'ChildDocumentType';
const parentDocumentName = 'ParentDocument';
const firstChildDocumentName = 'FirstChildDocument';
const secondChildDocumentName = 'SecondChildDocument';
const firstRootDocumentName = 'FirstRootDocument';
const secondRootDocumentName = 'SecondRootDocument';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(parentDocumentName);
  await umbracoApi.document.ensureNameNotExists(firstRootDocumentName);
  await umbracoApi.document.ensureNameNotExists(secondRootDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(parentDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(parentDocumentName);
  await umbracoApi.document.ensureNameNotExists(firstRootDocumentName);
  await umbracoApi.document.ensureNameNotExists(secondRootDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(parentDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

test('can sort children in a document with drag and drop', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const parentDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(parentDocumentTypeName, childDocumentTypeId);
  const parentId = await umbracoApi.document.createDefaultDocument(parentDocumentName, parentDocumentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(firstChildDocumentName, childDocumentTypeId, parentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondChildDocumentName, childDocumentTypeId, parentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(parentDocumentName);
  await umbracoUi.content.clickSortChildrenActionMenuOption();
  const firstChildLocator = umbracoUi.content.getTextLocatorWithName(firstChildDocumentName);
  const secondChildLocator = umbracoUi.content.getTextLocatorWithName(secondChildDocumentName);
  await umbracoUi.content.dragAndDrop(secondChildLocator, firstChildLocator);
  await umbracoUi.content.clickSortButton();

  // Assert
  await umbracoUi.content.openContentCaretButtonForName(parentDocumentName);
  await umbracoUi.content.doesIndexDocumentInTreeContainName(parentDocumentName, secondChildDocumentName, 0);
  await umbracoUi.content.doesIndexDocumentInTreeContainName(parentDocumentName, firstChildDocumentName, 1);
});

test('can sort documents at root with drag and drop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const rootDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(parentDocumentTypeName);
  await umbracoApi.document.createDefaultDocument(firstRootDocumentName, rootDocumentTypeId);
  await umbracoApi.document.createDefaultDocument(secondRootDocumentName, rootDocumentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(ConstantHelper.sections.content);
  await umbracoUi.content.clickSortChildrenActionMenuOption();
  const firstRootLocator = umbracoUi.content.getTextLocatorWithName(firstRootDocumentName);
  const secondRootLocator = umbracoUi.content.getTextLocatorWithName(secondRootDocumentName);
  await umbracoUi.content.dragAndDrop(secondRootLocator, firstRootLocator);
  await umbracoUi.content.clickSortButton();

  // Assert
  const rootNames = await umbracoApi.document.getAllAtRootNames();
  expect(rootNames.indexOf(secondRootDocumentName)).toBeLessThan(rootNames.indexOf(firstRootDocumentName));
});
