import { ConstantHelper, NotificationConstantHelper, test } from "@umbraco/playwright-testhelpers";
import { expect } from "@playwright/test";

const uploadTypes = [
  {type: 'Upload Article', fileExtensions: ['pdf', 'docx', 'doc']},
  {type: 'Upload Audio', fileExtensions: ['mp3', 'weba', 'oga', 'opus']},
  {type: 'Upload File', fileExtensions: []},
  {type: 'Upload Vector Graphics', fileExtensions: ['svg']},
  {type: 'Upload Video', fileExtensions: ['mp4', 'webm', 'ogv']}
];
const customDataTypeName = 'Custom Upload Field';
const editorAlias = 'Umbraco.UploadField';
const editorUiAlias = 'Umb.PropertyEditorUi.UploadField';

test.beforeEach(async ({ umbracoUi }) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can add accepted file extension', async ({ umbracoApi, umbracoUi }) => {
  // Arrange
  const fileExtensionValue = 'zip';
  await umbracoApi.dataType.createUploadDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickAddAcceptedFileExtensionsButton();
  await umbracoUi.dataType.enterAcceptedFileExtensions(fileExtensionValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'fileExtensions', [fileExtensionValue])).toBeTruthy();
});

test('can remove accepted file extension', async ({ umbracoApi, umbracoUi }) => {
  // Arrange
  const removedFileExtensionValue = "bat";
  await umbracoApi.dataType.createUploadDataType(customDataTypeName, [removedFileExtensionValue]);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeAcceptedFileExtensionsByValue(removedFileExtensionValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  const customDataTypeData = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(customDataTypeData.values).toEqual([]);
});

for (const uploadType of uploadTypes) {
  test(`the default configuration of ${uploadType.type} is correct`, async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.dataType.goToDataType(uploadType.type);

    // Assert
    await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.uploadSettings);
    await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.uploadSettings);
    await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
    await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
    if (uploadType.fileExtensions.length > 0) {
      expect(await umbracoApi.dataType.doesDataTypeHaveValue(uploadType.type, 'fileExtensions', uploadType.fileExtensions)).toBeTruthy();
    } else {
      const dataTypeDefaultData = await umbracoApi.dataType.getByName(uploadType.type);
      expect(dataTypeDefaultData.values).toEqual([]);
    }
  });
}
