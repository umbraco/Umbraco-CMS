import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Content Picker';
const customDataTypeName = 'Custom Content Picker';
const editorAlias = 'Umbraco.ContentPicker';
const editorUiAlias = 'Umb.PropertyEditorUi.DocumentPicker';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can show open button', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultContentPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickShowOpenButtonToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'showOpenButton', true)).toBeTruthy();
});

test('can ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.dataType.createDefaultContentPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickIgnoreUserStartNodesToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'ignoreUserStartNodes', true)).toBeTruthy();
});

test('can add start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create content
  const documentTypeName = 'TestDocumentType';
  const contentName = 'TestStartNode';
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  expect(await umbracoApi.document.doesExist(contentId)).toBeTruthy();
  // Create data type
  await umbracoApi.dataType.createDefaultContentPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.clickChooseButton();
  await umbracoUi.dataType.addContentStartNode(contentName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.dataType.doesDataTypeHaveValue(customDataTypeName, 'startNodeId', contentId)).toBeTruthy();

  // Clean
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can remove start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create content
  const documentTypeName = 'TestDocumentType';
  const contentName = 'TestStartNode';
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  expect(await umbracoApi.document.doesExist(contentId)).toBeTruthy();
  // Create data type
  await umbracoApi.dataType.createContentPickerDataTypeWithStartNode(customDataTypeName, contentId);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.removeContentStartNode(contentName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  await umbracoUi.dataType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const customDataTypeData = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(customDataTypeData.values).toEqual([]);

  // Clean
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('the default configuration is correct', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Assert
  await umbracoUi.dataType.doesSettingHaveValue(ConstantHelper.contentPickerSettings);
  await umbracoUi.dataType.doesSettingItemsHaveCount(ConstantHelper.contentPickerSettings);
  await umbracoUi.dataType.doesPropertyEditorHaveAlias(editorAlias);
  await umbracoUi.dataType.doesPropertyEditorHaveUiAlias(editorUiAlias);
  const dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeDefaultData.editorAlias).toBe(editorAlias);
  expect(dataTypeDefaultData.editorUiAlias).toBe(editorUiAlias);
  expect(dataTypeDefaultData.values).toEqual([]);
});
