import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Media Type Folder tests @smoke', () => {
  const mediaTypeFolderName = 'TestMediaTypeFolder';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.mediaType.ensureNameNotExists(mediaTypeFolderName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.mediaType.ensureNameNotExists(mediaTypeFolderName);
  });

  test('can create a empty media type folder', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.mediaType.clickActionsMenuForName('Media Types');
    await umbracoUi.mediaType.createFolder(mediaTypeFolderName);

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    const folder = await umbracoApi.mediaType.getByName(mediaTypeFolderName);
    expect(folder.name).toBe(mediaTypeFolderName);
    // Checks if the folder is in the root
    await umbracoUi.mediaType.clickRootFolderCaretButton();
    expect(umbracoUi.mediaType.isTreeItemVisible(mediaTypeFolderName)).toBeTruthy();
  });

  test('can delete a media type folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.mediaType.createFolder(mediaTypeFolderName);

    // Act
    await umbracoUi.mediaType.clickRootFolderCaretButton();
    await umbracoUi.mediaType.clickActionsMenuForName(mediaTypeFolderName);
    await umbracoUi.mediaType.deleteFolder();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    await umbracoApi.mediaType.doesNameExist(mediaTypeFolderName);
  });

  test('can rename a media type folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const oldFolderName = 'OldName';
    await umbracoApi.mediaType.createFolder(oldFolderName);

    // Act
    await umbracoUi.mediaType.clickRootFolderCaretButton();
    await umbracoUi.mediaType.clickActionsMenuForName(oldFolderName);

    await umbracoUi.mediaType.clickRenameFolderButton();
    await umbracoUi.mediaType.enterFolderName(mediaTypeFolderName);
    await umbracoUi.mediaType.clickUpdateFolderButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    const folder = await umbracoApi.mediaType.getByName(mediaTypeFolderName);
    expect(folder.name).toBe(mediaTypeFolderName);
  });


  // Currently it is not possible to create a folder in a folder
  test.skip('can create a media type folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'ChildFolder';
    await umbracoApi.mediaType.ensureNameNotExists(childFolderName);
    await umbracoApi.mediaType.createFolder(mediaTypeFolderName);

    // Act
    await umbracoUi.mediaType.clickRootFolderCaretButton();
    await umbracoUi.mediaType.clickActionsMenuForName(mediaTypeFolderName);

    await umbracoUi.mediaType.createFolder(childFolderName);

    // Clean
    await umbracoApi.mediaType.ensureNameNotExists(childFolderName);
  });

  // Currently it is not possible to create a folder in a folder
  test.skip('can create a media type folder in a folder in a folder', async ({page, umbracoApi, umbracoUi}) => {
  });
});
