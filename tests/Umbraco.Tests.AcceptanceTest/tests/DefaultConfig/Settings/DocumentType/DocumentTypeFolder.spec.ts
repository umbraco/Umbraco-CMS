import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('Document Type Folder tests', () => {
  const documentFolderName = 'TestDocumentTypeFolder';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentFolderName);
    await umbracoUi.goToBackOffice();

  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentFolderName);
  });

  test('can create a empty document type folder', async ({page, umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.clickActionsMenuForName('Document Types');
    await umbracoUi.documentType.createFolder(documentFolderName);

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    const folder = await umbracoApi.documentType.getByName(documentFolderName);
    expect(folder.name).toBe(documentFolderName);
    // Checks if the folder is in the root
    await umbracoUi.documentType.clickRootFolderCaretButton();
    expect(umbracoUi.documentType.isUniqueTreeItemVisible(documentFolderName)).toBeTruthy();
  });

  test('can delete a document type folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.documentType.createFolder(documentFolderName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.clickRootFolderCaretButton();
    await umbracoUi.documentType.clickActionsMenuForName(documentFolderName);
    await umbracoUi.documentType.deleteFolder();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    await umbracoApi.documentType.doesNameExist(documentFolderName);
  });

  test('can rename a document type folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const oldFolderName = 'OldName';
    await umbracoApi.documentType.createFolder(oldFolderName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.clickRootFolderCaretButton();
    await umbracoUi.documentType.clickActionsMenuForName(oldFolderName);

    await umbracoUi.documentType.clickRenameFolderButton();
    await umbracoUi.documentType.enterFolderName(documentFolderName);
    await umbracoUi.documentType.clickUpdateFolderButton();

    // await page.ge etByLabel('Update Folder').click();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    const folder = await umbracoApi.documentType.getByName(documentFolderName);
    expect(folder.name).toBe(documentFolderName);
  });


  // Currently it is not possible to create a folder in a folder
  test.skip('can create a document type folder in a folder', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolder';
    await umbracoApi.documentType.ensureNameNotExists(childFolderName);
    await umbracoApi.documentType.createFolder(documentFolderName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.clickRootFolderCaretButton();
    await umbracoUi.documentType.clickActionsMenuForName(documentFolderName);

    await page.pause();

    await umbracoUi.documentType.createFolder(childFolderName);

    // Clean
    await umbracoApi.documentType.ensureNameNotExists(childFolderName);
  });

  // Currently it is not possible to create a folder in a folder
  test.skip('can create a document type folder in a folder in a folder', async ({page, umbracoApi, umbracoUi}) => {
  });
});
