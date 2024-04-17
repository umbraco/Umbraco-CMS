import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Data Types Folder tests', () => {
  const dataTypeName = 'TestDataType';
  const dataTypeFolderName = 'TestDataTypeFolder';
  const editorAlias = 'Umbraco.DateTime';

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
  });

  test('can create a data type folder', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.dataType.clickActionsMenuAtRoot();
    await umbracoUi.dataType.clickCreateButton();
    await umbracoUi.dataType.clickNewDataTypeFolderButton();
    await umbracoUi.dataType.enterFolderName(dataTypeFolderName);
    await umbracoUi.dataType.clickCreateFolderButton();

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
  });

  // TODO: Remove skip due to the front-end changes. Need to update the rename folder locator.
  test.skip('can rename a data type folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongDataTypeFolderName = 'Wrong Folder';
    await umbracoApi.dataType.ensureNameNotExists(wrongDataTypeFolderName);
    await umbracoApi.dataType.createFolder(wrongDataTypeFolderName);
    expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeFolderName)).toBeTruthy();

    // Act
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.clickActionsMenuForDataType(wrongDataTypeFolderName);
    await umbracoUi.dataType.clickRenameFolderThreeDotsButton();
    await umbracoUi.dataType.enterFolderName(dataTypeFolderName);
    await umbracoUi.dataType.clickUpdateFolderButton();

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
    expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeFolderName)).toBeFalsy();
  });

  test('can delete a data type folder @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dataType.createFolder(dataTypeFolderName);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();

    // Act
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.deleteDataTypeFolder(dataTypeFolderName);

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeFalsy();
  });

  test.skip('can create a data type in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    let dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();

    // Act
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeFolderName);
    await umbracoUi.dataType.clickCreateButton();
    await umbracoUi.dataType.clickNewDataTypeThreeDotsButton();
    await umbracoUi.dataType.enterDataTypeName(dataTypeName);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
    const dataTypeChildren = await umbracoApi.dataType.getChildren(dataTypeFolderId);
    expect(dataTypeChildren[0].name).toBe(dataTypeName);
    expect(dataTypeChildren[0].isFolder).toBeFalsy();
  });

  //TODO: Remove skip when the frontend is ready
  test.skip('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'Child Folder';
    let dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();

    // Act
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeFolderName);
    await umbracoUi.dataType.clickCreateButton();
    await umbracoUi.dataType.clickNewDataTypeFolderButton();
    await umbracoUi.dataType.enterFolderName(childFolderName);
    await umbracoUi.dataType.clickCreateFolderButton();

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(childFolderName)).toBeTruthy();
    const dataTypeChildren = await umbracoApi.dataType.getChildren(dataTypeFolderId);
    expect(dataTypeChildren[0].name).toBe(childFolderName);
    expect(dataTypeChildren[0].isFolder).toBeTruthy();
  });

  //TODO: Remove skip when the frontend is ready
  test.skip('cannot delete a non-empty data type folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    let dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
    await umbracoApi.dataType.create(dataTypeName, editorAlias, [], dataTypeFolderId);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();

    // Act
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.deleteDataTypeFolder(dataTypeFolderName);

    // Assert
    await umbracoUi.dataType.isErrorNotificationVisible();
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
    const dataTypeChildren = await umbracoApi.dataType.getChildren(dataTypeFolderId);
    expect(dataTypeChildren[0].name).toBe(dataTypeName);
    expect(dataTypeChildren[0].isFolder).toBeFalsy();
  });

  test.skip('can move a data type to a data type folder', async ({}) => {
    // TODO: implement this test later when the front-end is ready
  });

  test.skip('can copy a data type to a data type folder', async ({}) => {
    // TODO: implement this test later when the front-end is ready
  });
});
