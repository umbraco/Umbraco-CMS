import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Document Blueprints tests', () => {
  const documentBlueprintName = 'TestDocumentBlueprints';
  const documentTypeName = 'DocumentTypeForBlueprint';
  let documentTypeId = '';

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.documentBlueprints.ensureNameNotExists(documentBlueprintName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    documentTypeId = await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.documentBlueprints.goToSettingsTreeItem('Document Blueprints');
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.documentBlueprints.ensureNameNotExists(documentBlueprintName);
    await umbracoApi.documentType.delete(documentTypeId);
  });

  test('can create a document blueprint from the Settings menu @smoke', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.documentBlueprints.goToSettingsTreeItem('Document Blueprints');
    await umbracoUi.documentBlueprints.clickActionsMenuAtRoot();
    await umbracoUi.documentBlueprints.clickCreateDocumentBlueprintButton();
    await umbracoUi.documentBlueprints.clickTextButtonWithName(documentTypeName);
    await umbracoUi.documentBlueprints.enterDocumentBlueprintName(documentBlueprintName);
    await umbracoUi.documentBlueprints.clickSaveButton();

    // Assert
    await umbracoUi.documentBlueprints.isSuccessNotificationVisible();
    expect(await umbracoApi.documentBlueprints.doesNameExist(documentBlueprintName)).toBeTruthy();
    await umbracoUi.documentBlueprints.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, true);
  });

  test('can rename a document blueprint name @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongDocumentBlueprintName = 'Wrong Document Blueprint';
    await umbracoApi.documentBlueprints.ensureNameNotExists(wrongDocumentBlueprintName);
    await umbracoApi.documentBlueprints.createDefaultDocumentBlueprints(wrongDocumentBlueprintName, documentTypeId);
    expect(await umbracoApi.documentBlueprints.doesNameExist(wrongDocumentBlueprintName)).toBeTruthy();

    // Act
    await umbracoUi.documentBlueprints.goToSettingsTreeItem('Document Blueprints');
    await umbracoUi.documentBlueprints.goToDocumentBlueprints(wrongDocumentBlueprintName);
    await umbracoUi.documentBlueprints.enterDocumentBlueprintName(documentBlueprintName);
    await umbracoUi.documentBlueprints.clickSaveButton();

    // Assert
    expect(await umbracoApi.documentBlueprints.doesNameExist(documentBlueprintName)).toBeTruthy();
    expect(await umbracoApi.documentBlueprints.doesNameExist(wrongDocumentBlueprintName)).toBeFalsy();
    await umbracoUi.documentBlueprints.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, true);
    await umbracoUi.documentBlueprints.isDocumentBlueprintRootTreeItemVisible(wrongDocumentBlueprintName, false);
  });

  test('can delete a document blueprint @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.documentBlueprints.createDefaultDocumentBlueprints(documentBlueprintName, documentTypeId);
    expect(await umbracoApi.documentBlueprints.doesNameExist(documentBlueprintName)).toBeTruthy();

    // Act
    await umbracoUi.documentBlueprints.goToSettingsTreeItem('Document Blueprints');
    await umbracoUi.documentBlueprints.clickRootFolderCaretButton();
    await umbracoUi.documentBlueprints.clickActionsMenuForDocumentBlueprints(documentBlueprintName);
    await umbracoUi.documentBlueprints.clickDeleteAndConfirmButton();

    // Assert
    await umbracoUi.documentBlueprints.isSuccessNotificationVisible();
    expect(await umbracoApi.documentBlueprints.doesNameExist(documentBlueprintName)).toBeFalsy();
    await umbracoUi.documentBlueprints.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, false);
  });

  // TODO: update this test after merging Content branch
  test.skip('can create a document blueprint from the Content menu @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange

    // Act
    await umbracoUi.documentBlueprints.goToSettingsTreeItem('Document Blueprints');
    await umbracoUi.documentBlueprints.clickActionsMenuAtRoot();
    await umbracoUi.documentBlueprints.clickCreateDocumentBlueprintButton();
    await umbracoUi.documentBlueprints.clickTextButtonWithName(documentTypeName);
    await umbracoUi.documentBlueprints.enterDocumentBlueprintName(documentBlueprintName);
    await umbracoUi.documentBlueprints.clickSaveButton();

    // Assert
    await umbracoUi.documentBlueprints.isSuccessNotificationVisible();
    expect(await umbracoApi.documentBlueprints.doesNameExist(documentBlueprintName)).toBeTruthy();
    await umbracoUi.documentBlueprints.isDocumentBlueprintRootTreeItemVisible(documentBlueprintName, true);
  });
});
