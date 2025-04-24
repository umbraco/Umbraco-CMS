import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const documentBlueprintName = 'TestDocumentBlueprints';
const documentTypeName = 'DocumentTypeForBlueprint';
let documentTypeId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentBlueprint.ensureNameNotExists(documentBlueprintName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create a document blueprint from the settings menu', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.documentBlueprint.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentBlueprint.clickActionsMenuAtRoot();
  await umbracoUi.documentBlueprint.clickCreateDocumentBlueprintButton();
  await umbracoUi.documentBlueprint.clickTextButtonWithName(documentTypeName);
  await umbracoUi.documentBlueprint.enterDocumentBlueprintName(documentBlueprintName);
  await umbracoUi.documentBlueprint.clickSaveButton();

  // Assert
  //await umbracoUi.documentBlueprint.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.documentBlueprint.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintName)).toBeTruthy();
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, true);
});

test('can rename a document blueprint', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongDocumentBlueprintName = 'Wrong Document Blueprint';
  await umbracoApi.documentBlueprint.ensureNameNotExists(wrongDocumentBlueprintName);
  await umbracoApi.documentBlueprint.createDefaultDocumentBlueprint(wrongDocumentBlueprintName, documentTypeId);
  expect(await umbracoApi.documentBlueprint.doesNameExist(wrongDocumentBlueprintName)).toBeTruthy();
  await umbracoUi.documentBlueprint.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentBlueprint.goToDocumentBlueprint(wrongDocumentBlueprintName);
  await umbracoUi.documentBlueprint.enterDocumentBlueprintName(documentBlueprintName);
  await umbracoUi.documentBlueprint.clickSaveButton();

  // Assert
  //await umbracoUi.documentBlueprint.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.documentBlueprint.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintName)).toBeTruthy();
  expect(await umbracoApi.documentBlueprint.doesNameExist(wrongDocumentBlueprintName)).toBeFalsy();
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, true, false);
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(wrongDocumentBlueprintName, false, false);
});

test('can delete a document blueprint', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentBlueprint.createDefaultDocumentBlueprint(documentBlueprintName, documentTypeId);
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintName)).toBeTruthy();
  await umbracoUi.documentBlueprint.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentBlueprint.reloadDocumentBlueprintsTree();
  await umbracoUi.documentBlueprint.clickActionsMenuForDocumentBlueprints(documentBlueprintName);
  await umbracoUi.documentBlueprint.clickDeleteMenuButton();
  await umbracoUi.documentBlueprint.clickConfirmToDeleteButton();

  // Assert
  //await umbracoUi.documentBlueprint.doesSuccessNotificationHaveText(NotificationConstantHelper.success.deleted);
  await umbracoUi.documentBlueprint.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintName)).toBeFalsy();
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, false, false);
});

test('can create a document blueprint from the content menu', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeName = 'DocumentTypeForContent';
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoApi.document.createDefaultDocument(documentBlueprintName, documentTypeId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(documentBlueprintName);
  await umbracoUi.content.clickCreateDocumentBlueprintButton();
  await umbracoUi.content.clickSaveModalButton();

  // Assert
  //await umbracoUi.documentBlueprint.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.documentBlueprint.isErrorNotificationVisible(false);
  expect(await umbracoApi.documentBlueprint.doesNameExist(documentBlueprintName)).toBeTruthy();
  await umbracoUi.documentBlueprint.goToSettingsTreeItem('Document Blueprints');
  await umbracoUi.documentBlueprint.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, true);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});
