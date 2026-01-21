import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Element Picker';
const customDataTypeName = 'Custom Element Picker';
const editorAlias = 'Umbraco.ElementPicker';
const editorUiAlias = 'Umb.PropertyEditorUi.ElementPicker';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can create an element picker data type', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuForName('Data Types');
  await umbracoUi.dataType.clickCreateActionMenuOption();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(customDataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor(dataTypeName);
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeCreated();

  // Assert
  await umbracoUi.dataType.isDataTypeTreeItemVisible(customDataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(customDataTypeName)).toBeTruthy();
  const dataTypeData = await umbracoApi.dataType.getByName(customDataTypeName);
  expect(dataTypeData.editorAlias).toBe(editorAlias);
  expect(dataTypeData.editorUiAlias).toBe(editorUiAlias);
   expect(dataTypeData.values).toEqual([]);
});

test('can set minimum amount', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minAmount = 2;
  await umbracoApi.dataType.createDefaultElementPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterAmountLowValue(minAmount.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesElementPickerHaveMinAndMaxAmount(customDataTypeName, minAmount)).toBeTruthy();
});

test('can set maximum amount', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxAmount = 5;
  await umbracoApi.dataType.createDefaultElementPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterAmountHighValue(maxAmount.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesElementPickerHaveMinAndMaxAmount(customDataTypeName, undefined, maxAmount)).toBeTruthy();
});

test('can set minimum and maximum amount', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const minAmount = 1;
  const maxAmount = 10;
  await umbracoApi.dataType.createDefaultElementPickerDataType(customDataTypeName);
  await umbracoUi.dataType.goToDataType(customDataTypeName);

  // Act
  await umbracoUi.dataType.enterAmountValue(minAmount.toString(), maxAmount.toString());
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.dataType.doesElementPickerHaveMinAndMaxAmount(customDataTypeName, minAmount, maxAmount)).toBeTruthy();
});
