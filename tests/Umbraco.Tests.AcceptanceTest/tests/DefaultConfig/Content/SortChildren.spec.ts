import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

const rootDocumentTypeName = 'RootDocumentTypeForSort';
const childDocumentTypeName = 'ChildDocumentTypeForSort';
const rootDocumentName = 'RootDocumentForSort';

let rootDocumentId = '';
let childDocumentTypeId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(rootDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
  const rootDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(rootDocumentTypeName, childDocumentTypeId);
  rootDocumentId = await umbracoApi.document.createDefaultDocument(rootDocumentName, rootDocumentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(rootDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
});

async function getChildNamesInOrder(umbracoApi): Promise<string[]> {
  const children = await umbracoApi.document.getChildren(rootDocumentId);
  return children.map((child) => child.variants[0].name);
}

test('can sort children individually by drag and drop', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstChildName = 'FirstChild';
  const secondChildName = 'SecondChild';
  await umbracoApi.document.createDefaultDocumentWithParent(firstChildName, childDocumentTypeId, rootDocumentId);
  await umbracoApi.document.createDefaultDocumentWithParent(secondChildName, childDocumentTypeId, rootDocumentId);
  expect(await getChildNamesInOrder(umbracoApi)).toEqual([firstChildName, secondChildName]);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickSortChildrenActionMenuOption();
  const firstChildLocator = umbracoUi.content.getTextLocatorWithName(firstChildName);
  const secondChildLocator = umbracoUi.content.getTextLocatorWithName(secondChildName);
  await umbracoUi.content.dragAndDrop(secondChildLocator, firstChildLocator);
  await umbracoUi.content.clickSortButton();

  // Assert
  await expect.poll(() => getChildNamesInOrder(umbracoApi)).toEqual([secondChildName, firstChildName]);
});

test('can sort all children by name in ascending order', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create children out of alphabetical order so the sort produces a visibly different result.
  await umbracoApi.document.createDefaultDocumentWithParent('Charlie', childDocumentTypeId, rootDocumentId);
  await umbracoApi.document.createDefaultDocumentWithParent('Alpha', childDocumentTypeId, rootDocumentId);
  await umbracoApi.document.createDefaultDocumentWithParent('Bravo', childDocumentTypeId, rootDocumentId);
  expect(await getChildNamesInOrder(umbracoApi)).toEqual(['Charlie', 'Alpha', 'Bravo']);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickSortChildrenActionMenuOption();
  await umbracoUi.content.prepareSortByField('Name', 'Ascending');
  await umbracoUi.content.clickSortButton();

  // Assert
  await expect.poll(() => getChildNamesInOrder(umbracoApi)).toEqual(['Alpha', 'Bravo', 'Charlie']);
});

test('can sort all children by name in descending order', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithParent('Alpha', childDocumentTypeId, rootDocumentId);
  await umbracoApi.document.createDefaultDocumentWithParent('Charlie', childDocumentTypeId, rootDocumentId);
  await umbracoApi.document.createDefaultDocumentWithParent('Bravo', childDocumentTypeId, rootDocumentId);
  expect(await getChildNamesInOrder(umbracoApi)).toEqual(['Alpha', 'Charlie', 'Bravo']);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(rootDocumentName);
  await umbracoUi.content.clickSortChildrenActionMenuOption();
  await umbracoUi.content.prepareSortByField('Name', 'Descending');
  await umbracoUi.content.clickSortButton();

  // Assert
  await expect.poll(() => getChildNamesInOrder(umbracoApi)).toEqual(['Charlie', 'Bravo', 'Alpha']);
});
