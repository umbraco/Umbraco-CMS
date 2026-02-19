import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'EntityPickerWithCollection';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSection(ConstantHelper.sections.settings);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('can create an entity picker data type with the collection data source', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.dataType.clickActionsMenuForName('Data Types');
  await umbracoUi.dataType.clickCreateActionMenuOption();
  await umbracoUi.dataType.clickDataTypeButton();
  await umbracoUi.dataType.enterDataTypeName(dataTypeName);
  await umbracoUi.dataType.clickSelectAPropertyEditorButton();
  await umbracoUi.dataType.selectAPropertyEditor('Entity Data Picker');
  await umbracoUi.dataType.clickChooseDataSourceButton();
  await umbracoUi.dataType.clickButtonWithName('My Picker Collection Data Source');
  await umbracoUi.dataType.clickChooseModalButton();
  await umbracoUi.dataType.clickSaveButtonAndWaitForDataTypeToBeCreated();

  // Assert
  await umbracoUi.dataType.isDataTypeTreeItemVisible(dataTypeName);
  expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
});
