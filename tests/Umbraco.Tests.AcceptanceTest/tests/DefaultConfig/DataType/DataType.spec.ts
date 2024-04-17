import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Data Types basic functionalities tests', () => {
  const dataTypeName = 'TestDataType';
  const editorAlias = 'Umbraco.DateTime';

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  });

  test('can create a data type @smoke', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.dataType.clickActionsMenuAtRoot();
    await umbracoUi.dataType.clickCreateButton();
    await umbracoUi.dataType.clickNewDataTypeThreeDotsButton();
    await umbracoUi.dataType.enterDataTypeName(dataTypeName);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
  });

  test('can update a data type name @smoke', async ({umbracoApi, umbracoUi}) => {
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

  test.skip('can delete a data type @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.dataType.create(dataTypeName, editorAlias, []);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();

    // Act
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.deleteDataType(dataTypeName);

    // Assert
    await umbracoUi.dataType.isSuccessNotificationVisible();
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeFalsy();
  });

  test.skip('can change Property Editor in a data type @smoke', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const updatedEditorName = 'Text Area';
    const updatedEditorAlias = 'Umbraco.TextArea';
    const updatedEditorUiAlias = 'Umb.PropertyEditorUi.TextArea';

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
});
