import { test } from "@umbraco/playwright-testhelpers";
import { expect } from "@playwright/test";

const uploadTypes = ['Upload Article', 'Upload Audio', 'Upload File', 'Upload Vector Graphics', 'Upload Video'];
for (const uploadType of uploadTypes) {
  test.describe(`${uploadType} tests`, () => {
    let dataTypeDefaultData = null;
    let dataTypeData = null;

    test.beforeEach(async ({ umbracoUi, umbracoApi }) => {
      await umbracoUi.goToBackOffice();
      await umbracoUi.dataType.goToSettingsTreeItem("Data Types");
      dataTypeDefaultData = await umbracoApi.dataType.getByName(uploadType);
    });

    test.afterEach(async ({ umbracoApi }) => {
      if (dataTypeDefaultData !== null) {
        await umbracoApi.dataType.update(dataTypeDefaultData.id, dataTypeDefaultData);
      }
    });

    //TODO: Remove skip when the frontend is ready
    test.skip("can add accepted file extension", async ({ umbracoApi, umbracoUi }) => {
      // Arrange
      const fileExtensionValue = 'zip';
      const expectedDataTypeValues = [
        {
          alias: "fileExtensions",
          value: [
            {
              "value": fileExtensionValue
            },
          ],
        },
      ];
      // Remove all existing accepted file extensions
      dataTypeData = await umbracoApi.dataType.getByName(uploadType);
      dataTypeData.values = [];
      await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
      await umbracoUi.dataType.goToDataType(uploadType);

      // Act
      await umbracoUi.dataType.clickAddAcceptedFileExtensionsButton();
      await umbracoUi.dataType.enterAcceptedFileExtensions(fileExtensionValue);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(uploadType);
      expect(dataTypeData.values).toEqual(expectedDataTypeValues);
    });

    //TODO: Remove skip when the frontend is ready
    test.skip("can remove accepted file extension", async ({ umbracoApi, umbracoUi }) => {
      // Arrange
      const removedFileExtensionValue = "bat";
      const removedFileExtensionsValues = [
        {
          alias: "fileExtensions",
          value: [
            {
              "value": removedFileExtensionValue
            },
          ],
        },
      ];
      // Remove all existing accepted file extensions and add an file extension to remove
      dataTypeData = await umbracoApi.dataType.getByName(uploadType);
      dataTypeData.values = removedFileExtensionsValues;
      await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
      await umbracoUi.dataType.goToDataType(uploadType);

      // Act
      await umbracoUi.dataType.removeAcceptedFileExtensionsByValue(removedFileExtensionValue);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(uploadType);
      expect(dataTypeData.values).toEqual([]);
    });
  });
}
