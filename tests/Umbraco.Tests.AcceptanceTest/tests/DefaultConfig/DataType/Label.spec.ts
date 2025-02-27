import { test } from "@umbraco/playwright-testhelpers";
import { expect } from "@playwright/test";

const labelTypes = ['Label (bigint)', 'Label (datetime)', 'Label (decimal)', 'Label (integer)', 'Label (string)', 'Label (time)'];
for (const labelType of labelTypes) {
  test.describe(`${labelType} tests`, () => {
    let dataTypeDefaultData = null;
    let dataTypeData = null;

    test.beforeEach(async ({ umbracoUi, umbracoApi }) => {
      await umbracoUi.goToBackOffice();
      await umbracoUi.dataType.goToSettingsTreeItem("Data Types");
      dataTypeDefaultData = await umbracoApi.dataType.getByName(labelType);
      await umbracoUi.dataType.goToDataType(labelType);
    });

    test.afterEach(async ({ umbracoApi }) => {
      if (dataTypeDefaultData !== null) {
        await umbracoApi.dataType.update(dataTypeDefaultData.id,dataTypeDefaultData);
      }
    });

    test('can change value type', async ({ umbracoApi, umbracoUi }) => {
      // Arrange
      const expectedDataTypeValues = [
        {
          "alias": "umbracoDataValueType",
          "value": "TEXT",
        }
      ];

      // Act
      await umbracoUi.dataType.changeValueType("Long String");
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(labelType);
      expect(dataTypeData.values).toEqual(expectedDataTypeValues);
    });
  });
}
