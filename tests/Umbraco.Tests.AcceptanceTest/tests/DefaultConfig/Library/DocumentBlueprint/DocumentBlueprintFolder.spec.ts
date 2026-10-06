import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const documentBlueprintName = 'TestDocumentBlueprint';
const documentTypeName = 'DocumentTypeForBlueprint';
const documentBlueprintFolderName = 'TestBlueprintFolder';
const wrongDocumentBlueprintFolderName = 'Wrong Blueprint Folder';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintFolderName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(wrongDocumentBlueprintFolderName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentBlueprint.goToSection(ConstantHelper.sections.library);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintFolderName);
  await umbracoApi.documentBlueprint.ensureNameNotExists(wrongDocumentBlueprintFolderName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create a document blueprint folder', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.documentBlueprint.clickActionsMenuAtRoot();
  await umbracoUi.documentBlueprint.createDocumentBlueprintFolderAndWaitForFolderToBeCreated(documentBlueprintFolderName);

  // Assert
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintFolderName)).toBeTruthy();
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(documentBlueprintFolderName, true);
});

test('can rename a document blueprint folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentBlueprint.createFolder(wrongDocumentBlueprintFolderName);
  await umbracoUi.documentBlueprint.reloadDocumentBlueprintsTree();

  // Act
  await umbracoUi.documentBlueprint.clickActionsMenuForDocumentBlueprints(wrongDocumentBlueprintFolderName);
  await umbracoUi.documentBlueprint.clickRenameActionMenuOption();
  await umbracoUi.documentBlueprint.enterRenameFolderName(documentBlueprintFolderName);
  await umbracoUi.documentBlueprint.clickConfirmRenameButtonAndWaitForFolderToBeRenamed();

  // Assert
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintFolderName)).toBeTruthy();
  expect(await umbracoApi.documentBlueprint.doesNameExist(wrongDocumentBlueprintFolderName)).toBeFalsy();
});

test('can delete a document blueprint folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentBlueprint.createFolder(documentBlueprintFolderName);
  await umbracoUi.documentBlueprint.reloadDocumentBlueprintsTree();

  // Act
  await umbracoUi.documentBlueprint.clickActionsMenuForDocumentBlueprints(documentBlueprintFolderName);
  await umbracoUi.documentBlueprint.clickDeleteActionMenuOption();
  await umbracoUi.documentBlueprint.clickConfirmToDeleteButtonAndWaitForFolderToBeDeleted();

  // Assert
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintFolderName)).toBeFalsy();
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(documentBlueprintFolderName, false);
});

test('can create a document blueprint folder in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childFolderName = 'ChildBlueprintFolder';
  await umbracoApi.documentBlueprint.ensureNameNotExists(childFolderName);
  const parentFolderId = await umbracoApi.documentBlueprint.createFolder(documentBlueprintFolderName);
  await umbracoUi.documentBlueprint.reloadDocumentBlueprintsTree();

  // Act
  await umbracoUi.documentBlueprint.clickActionsMenuForDocumentBlueprints(documentBlueprintFolderName);
  await umbracoUi.documentBlueprint.createDocumentBlueprintFolderAndWaitForFolderToBeCreated(childFolderName);

  // Assert
  const children = await umbracoApi.documentBlueprint.getChildren(parentFolderId);
  expect(children.length).toBe(1);
  expect(children[0].name).toBe(childFolderName);
});

test('can create a document blueprint in a folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  const documentBlueprintFolderId = await umbracoApi.documentBlueprint.createFolder(documentBlueprintFolderName);
  await umbracoUi.documentBlueprint.reloadDocumentBlueprintsTree();

  // Act
  await umbracoUi.documentBlueprint.clickActionsMenuForDocumentBlueprints(documentBlueprintFolderName);
  await umbracoUi.documentBlueprint.clickCreateActionMenuOption();
  await umbracoUi.documentBlueprint.clickCreateNewDocumentBlueprintButton();
  await umbracoUi.documentBlueprint.chooseDocumentTypeForDocumentBlueprint(documentTypeName);
  await umbracoUi.documentBlueprint.enterDocumentBlueprintName(documentBlueprintName);
  await umbracoUi.documentBlueprint.clickSaveButtonAndWaitForDocumentBlueprintToBeCreated();

  // Assert
  const children = await umbracoApi.documentBlueprint.getChildren(documentBlueprintFolderId);
  expect(children.length).toBe(1);
  expect(children[0].name).toBe(documentBlueprintName);
});
