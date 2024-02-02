import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Data Types basic functionalities tests', () => {
  const dataTypeName = 'TestDataType';
  const dataTypeFolderName = 'TestDataTypeFolder';
  const editorAlias = 'Umbraco.DateTime';

  test.beforeEach(async ({umbracoUi}) => {
    await umbracoUi.goToBackOffice();
    await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
    await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
  });

  test('can create a data type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
    
    // Act
    await umbracoUi.dataType.clickActionsMenuAtRoot();
    await umbracoUi.dataType.clickCreateThreeDotsButton();
    await umbracoUi.dataType.clickNewDataTypeButton();
    await umbracoUi.dataType.enterDataTypeName(dataTypeName);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  });

  test('can update a data type name', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongDataTypeName = 'Wrong Data Type';
    await umbracoApi.dataType.ensureNameNotExists(wrongDataTypeName);
    await umbracoApi.dataType.create(wrongDataTypeName, editorAlias, []);
    expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeName)).toBeTruthy();

    // Act
    await umbracoUi.dataType.goToDataType(wrongDataTypeName);
    await umbracoUi.dataType.enterDataTypeName(dataTypeName);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
    expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeName)).toBeFalsy();
  });

  test('can delete a data type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
    await umbracoApi.dataType.create(dataTypeName, editorAlias, []);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();

    // Act
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.deleteDataType(dataTypeName);

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeFalsy();
  });

  test('can change Property Editor in a data type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const updatedEditorName = 'Text Area';
    const updatedEditorAlias = 'Umbraco.TextArea';
    const updatedEditorUiAlias = 'Umb.PropertyEditorUi.TextArea';
    
    await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
    await umbracoApi.dataType.create(dataTypeName, editorAlias, []);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();

    // Act
    await umbracoUi.dataType.goToDataType(dataTypeName);
    await umbracoUi.dataType.clickChangeButton();
    await umbracoUi.dataType.selectPropertyEditorUIByName(updatedEditorName);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    expect(dataTypeData.editorAlias).toBe(updatedEditorAlias);
    expect(dataTypeData.editorUiAlias).toBe(updatedEditorUiAlias);
  });

  test('can create a data type folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
    
    // Act
    await umbracoUi.dataType.clickActionsMenuAtRoot();
    await umbracoUi.dataType.clickCreateThreeDotsButton();
    await umbracoUi.dataType.clickNewDataTypeFolderButton();
    await umbracoUi.dataType.enterFolderName(dataTypeFolderName);
    await umbracoUi.dataType.clickCreateFolderButton();

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
  });

  test('can rename a data type folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongDataTypeName = 'Wrong Folder';
    await umbracoApi.dataType.ensureNameNotExists(wrongDataTypeName);
    await umbracoApi.dataType.createFolder(wrongDataTypeName);
    expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeName)).toBeTruthy();
    
    // Act
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.clickActionsMenuForDataType(wrongDataTypeName);
    await umbracoUi.dataType.clickRenameFolderThreeDotsButton();
    await umbracoUi.dataType.enterFolderName(dataTypeFolderName);
    await umbracoUi.dataType.clickUpdateFolderButton();

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
    expect(await umbracoApi.dataType.doesNameExist(wrongDataTypeName)).toBeFalsy();
  });

  test('can delete a data type folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
    await umbracoApi.dataType.createFolder(dataTypeFolderName);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
    
    // Act
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.deleteDataTypeFolder(dataTypeFolderName);

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeFalsy();
  });

  test('can create a data type in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
    let dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
    
    // Act
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeFolderName);
    await umbracoUi.dataType.clickCreateThreeDotsButton();
    await umbracoUi.dataType.clickNewDataTypeButton();
    await umbracoUi.dataType.enterDataTypeName(dataTypeName);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
    const dataTypeChildren = await umbracoApi.dataType.getChildren(dataTypeFolderId);
    expect(dataTypeChildren[0].name).toBe(dataTypeName); 
    expect(dataTypeChildren[0].isFolder).toBeFalsy(); 
  });

  test('can create a folder in a folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const childFolderName = 'Child Folder';
    await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
    let dataTypeFolderId = await umbracoApi.dataType.createFolder(dataTypeFolderName);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeFolderName)).toBeTruthy();
    
    // Act
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeFolderName);
    await umbracoUi.dataType.clickCreateThreeDotsButton();
    await umbracoUi.dataType.clickNewDataTypeFolderButton();
    await umbracoUi.dataType.enterFolderName(childFolderName);
    await umbracoUi.dataType.clickCreateFolderButton();

    // Assert
    expect(await umbracoApi.dataType.doesNameExist(childFolderName)).toBeTruthy();
    const dataTypeChildren = await umbracoApi.dataType.getChildren(dataTypeFolderId);
    expect(dataTypeChildren[0].name).toBe(childFolderName); 
    expect(dataTypeChildren[0].isFolder).toBeTruthy(); 
  });

  test('cannot delete a non-empty data type folder', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dataType.ensureNameNotExists(dataTypeFolderName);
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
