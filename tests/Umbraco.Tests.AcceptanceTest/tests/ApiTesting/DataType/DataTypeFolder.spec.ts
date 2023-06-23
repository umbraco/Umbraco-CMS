import {test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

test.describe('DataTypeFolder tests', () => {
  const dataTypeFolderName = "TestTypeFolder";

  test.beforeEach(async ({page, umbracoApi}) => {
    await umbracoApi.dataType.ensureDataTypeNameNotExistsAtRoot(dataTypeFolderName);
  });

  test.afterEach(async ({page, umbracoApi}) => {
    await umbracoApi.dataType.ensureDataTypeNameNotExistsAtRoot(dataTypeFolderName);
  });

  test('can create a dataType folder', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.dataType.createDataTypeFolder(dataTypeFolderName);

    // Assert
    await expect(umbracoApi.dataType.doesDataTypeWithNameExistAtRoot(dataTypeFolderName)).toBeTruthy();
  });

  test('can update a dataType folder', async ({page, umbracoApi, umbracoUi}) => {
    const oldDataTypeFolderName = 'Oldie';

    await umbracoApi.dataType.createDataTypeFolder(oldDataTypeFolderName);

    const dataTypeFolder = await umbracoApi.dataType.getDataTypeFolderByName(oldDataTypeFolderName);

    // Updates the dataType folder
    dataTypeFolder.name = dataTypeFolderName;
    await umbracoApi.dataType.updateDataTypeFolderById(dataTypeFolder.id, dataTypeFolder);

    // Assert
    await umbracoApi.dataType.doesDataTypeWithNameExistAtRoot(dataTypeFolderName);
    // Checks if the dataType folder was updated
    const newDataTypeFolderName = await umbracoApi.dataType.getDataTypeFolderById(dataTypeFolder.id);
    await expect(newDataTypeFolderName.name == dataTypeFolderName).toBeTruthy();
  });

  test('can delete a dataType folder', async ({page, umbracoApi, umbracoUi}) => {
    await umbracoApi.dataType.createDataTypeFolder(dataTypeFolderName);

    await expect(umbracoApi.dataType.doesDataTypeWithNameExistAtRoot(dataTypeFolderName)).toBeTruthy();

    await umbracoApi.dataType.deleteDataTypeFolderByName(dataTypeFolderName);

    // Assert
    await expect(await umbracoApi.dataType.doesDataTypeWithNameExistAtRoot(dataTypeFolderName)).toBeFalsy();
  });
});
