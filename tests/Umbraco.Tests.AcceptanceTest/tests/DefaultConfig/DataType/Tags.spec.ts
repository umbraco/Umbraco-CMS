import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Tags';
let dataTypeDefaultData = null;
let dataTypeData = null;

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
  await umbracoUi.dataType.goToDataType(dataTypeName);
  dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  if (dataTypeDefaultData !== null) {
    await umbracoApi.dataType.update(dataTypeDefaultData.id, dataTypeDefaultData);   
  }   
});

test('can update define a tag group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const tagGroup = 'testTagGroup';
  const expectedDataTypeValues = {
    "alias": "group",
    "value": tagGroup
  };

  // Act
  await umbracoUi.dataType.enterDefineTagGroupValue(tagGroup);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can select storage type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const storageType = 'Csv';
  const expectedDataTypeValues = {
    "alias": "storageType",
    "value": storageType
  };

  // Act
  await umbracoUi.dataType.selectStorageTypeOption(storageType);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});
