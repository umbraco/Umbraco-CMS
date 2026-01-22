import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const documentBlueprintName = 'TestDocumentBlueprints';
const documentTypeName = 'DocumentTypeForBlueprint';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create a document blueprint from the settings menu', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentBlueprint.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentBlueprint.clickActionsMenuAtRoot();
  await umbracoUi.documentBlueprint.clickCreateActionMenuOption();
  await umbracoUi.documentBlueprint.clickCreateNewDocumentBlueprintButton();
  await umbracoUi.documentBlueprint.clickTextButtonWithName(documentTypeName);
  await umbracoUi.documentBlueprint.clickChooseButton();
  // We need to wait for a bit while the new document is being loaded
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.documentBlueprint.enterDocumentBlueprintName(documentBlueprintName);
  await umbracoUi.documentBlueprint.clickSaveButtonAndWaitForDocumentBlueprintToBeCreated();

  // Assert
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintName)).toBeTruthy();
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, true);
});

test('can rename a document blueprint', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  const wrongDocumentBlueprintName = 'Wrong Document Blueprint';
  await umbracoApi.documentBlueprint.ensureNameNotExists(wrongDocumentBlueprintName);
  await umbracoApi.documentBlueprint.createDefaultDocumentBlueprint(wrongDocumentBlueprintName, documentTypeId);
  expect(await umbracoApi.documentBlueprint.doesNameExist(wrongDocumentBlueprintName)).toBeTruthy();
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentBlueprint.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentBlueprint.goToDocumentBlueprint(wrongDocumentBlueprintName);
  await umbracoUi.documentBlueprint.enterDocumentBlueprintName(documentBlueprintName);
  await umbracoUi.documentBlueprint.clickSaveButtonAndWaitForDocumentBlueprintToBeUpdated();

  // Assert
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintName)).toBeTruthy();
  expect(await umbracoApi.documentBlueprint.doesNameExist(wrongDocumentBlueprintName)).toBeFalsy();
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, true, false);
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(wrongDocumentBlueprintName, false, false);
});

test('can delete a document blueprint', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoApi.documentBlueprint.createDefaultDocumentBlueprint(documentBlueprintName, documentTypeId);
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintName)).toBeTruthy();
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentBlueprint.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentBlueprint.reloadDocumentBlueprintsTree();
  await umbracoUi.documentBlueprint.clickActionsMenuForDocumentBlueprints(documentBlueprintName);
  await umbracoUi.documentBlueprint.clickDeleteActionMenuOption();
  await umbracoUi.documentBlueprint.clickConfirmToDeleteButtonAndWaitForDocumentBlueprintToBeDeleted();

  // Assert
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintName)).toBeFalsy();
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, false, false);
});

test('can create a document blueprint from the content menu', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoApi.document.createDefaultDocument(documentBlueprintName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(documentBlueprintName);
  await umbracoUi.content.clickCreateBlueprintActionMenuOption();
  await umbracoUi.content.clickSaveModalButtonAndWaitForDocumentBlueprintToBeCreated();

  // Assert
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintName)).toBeTruthy();
  await umbracoUi.documentBlueprint.goToSettingsTreeItem('Document Blueprints');
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, true);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create a variant document blueprint', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.createDanishLanguage();
  await umbracoApi.documentType.createDocumentTypeWithAllowVaryByCulture(documentTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.documentBlueprint.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentBlueprint.clickActionsMenuAtRoot();
  await umbracoUi.documentBlueprint.clickCreateActionMenuOption();
  await umbracoUi.documentBlueprint.clickCreateNewDocumentBlueprintButton();
  await umbracoUi.documentBlueprint.clickTextButtonWithName(documentTypeName);
  await umbracoUi.documentBlueprint.clickChooseButton();
  // We need to wait for a bit while the new document is being loaded
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.documentBlueprint.enterDocumentBlueprintName(documentBlueprintName);
  await umbracoUi.documentBlueprint.clickSaveButtonAndWaitForDocumentBlueprintToBeCreated();

  // Assert
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintName)).toBeTruthy();
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, true);
  await umbracoUi.documentBlueprint.page.on('console', message => {
    expect(message.type()).not.toBe('error');
  });

  // Clean
  await umbracoApi.language.ensureIsoCodeNotExists('da');
});
