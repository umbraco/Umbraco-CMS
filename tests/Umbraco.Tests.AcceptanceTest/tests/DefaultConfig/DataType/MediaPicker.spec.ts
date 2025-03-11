import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypes = ['Media Picker', 'Multiple Media Picker', 'Image Media Picker', 'Multiple Image Media Picker'];
for (const dataTypeName of dataTypes) {
  test.describe(`${dataTypeName} tests`, () => {
    let dataTypeDefaultData = null;
    let dataTypeData = null;

    test.beforeEach(async ({umbracoUi, umbracoApi}) => {
      await umbracoUi.goToBackOffice();
      await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
      dataTypeDefaultData = await umbracoApi.dataType.getByName(dataTypeName);
    });

    test.afterEach(async ({umbracoApi}) => {
      if (dataTypeDefaultData !== null) {
        await umbracoApi.dataType.update(dataTypeDefaultData.id, dataTypeDefaultData);
      }
    });

    test('can update pick multiple items', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const expectedDataTypeValues = {
        "alias": "multiple",
        "value": dataTypeName === 'Media Picker' || dataTypeName === 'Image Media Picker' ? true : false,
      };

      // Act
      await umbracoUi.dataType.goToDataType(dataTypeName);
      await umbracoUi.dataType.clickPickMultipleItemsToggle();
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('can update amount', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const lowValue = 5;
      const highValue = 1000;
      const expectedDataTypeValues = {
        "alias": "validationLimit",
        "value": {
          "min": lowValue,
          "max": highValue
        }
      };

      // Act
      await umbracoUi.dataType.goToDataType(dataTypeName);
      await umbracoUi.dataType.enterAmountValue(lowValue.toString(), highValue.toString());
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('can update enable focal point', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const expectedDataTypeValues = {
        "alias": "enableLocalFocalPoint",
        "value": true
      };

      // Act
      await umbracoUi.dataType.goToDataType(dataTypeName);
      await umbracoUi.dataType.clickEnableFocalPointToggle();
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('can add image crop', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const cropData = ['Test Label', 'Test Alias', 100, 50];
      const expectedDataTypeValues = {
        "alias": "crops",
        "value": [
          {
            "label": cropData[0],
            "alias": cropData[1],
            "width": cropData[2],
            "height": cropData[3]
          }
        ]
      };

      // Act
      await umbracoUi.dataType.goToDataType(dataTypeName);
      await umbracoUi.waitForTimeout(500);
      await umbracoUi.dataType.enterCropValues(
        cropData[0].toString(),
        cropData[1].toString(),
        cropData[2].toString(),
        cropData[3].toString()
      );
      await umbracoUi.waitForTimeout(500);
      await umbracoUi.dataType.clickAddCropButton();
      await umbracoUi.dataType.clickSaveButton();
      await umbracoUi.waitForTimeout(500);

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('can update ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const expectedDataTypeValues = {
        "alias": "ignoreUserStartNodes",
        "value": true
      };

      // Act
      await umbracoUi.dataType.goToDataType(dataTypeName);
      await umbracoUi.dataType.clickIgnoreUserStartNodesToggle();
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('can add accepted types', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const mediaTypeName = 'Audio';
      const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
      const imageTypeData = await umbracoApi.mediaType.getByName('Image');
      const expectedFilterValue =
        dataTypeName === "Image Media Picker" ||
        dataTypeName === "Multiple Image Media Picker"
          ? imageTypeData.id + "," + mediaTypeData.id
          : mediaTypeData.id;
      const expectedDataTypeValues = {
        "alias": "filter",
        "value": expectedFilterValue
      };

      // Act
      await umbracoUi.dataType.goToDataType(dataTypeName);
      await umbracoUi.dataType.addAcceptedType(mediaTypeName);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
    });

    test('can remove accepted types', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const mediaTypeName = 'Audio';
      const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
      const removedDataTypeValues = [{
        "alias": "filter",
        "value": mediaTypeData.id
      }];
      const expectedDataTypeValues = [];

      // Remove all existing options and add an option to remove
      dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      dataTypeData.values = removedDataTypeValues;
      await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);

      // Act
      await umbracoUi.dataType.goToDataType(dataTypeName);
      await umbracoUi.dataType.removeAcceptedType(mediaTypeName);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      expect(dataTypeData.values).toEqual(expectedDataTypeValues);
    });

    test('can add start node', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      // Create media
      const mediaName = 'TestStartNode';
      await umbracoApi.media.ensureNameNotExists(mediaName);
      const mediaId = await umbracoApi.media.createDefaultMediaWithArticle(mediaName);
      expect(await umbracoApi.media.doesNameExist(mediaName)).toBeTruthy();

      const expectedDataTypeValues = {
        "alias": "startNodeId",
        "value": mediaId
      };

      // Act
      await umbracoUi.dataType.goToDataType(dataTypeName);
      await umbracoUi.dataType.clickChooseStartNodeButton();
      await umbracoUi.dataType.addMediaStartNode(mediaName);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);

      // Clean
      await umbracoApi.media.ensureNameNotExists(mediaName);
    });

    test('can remove start node', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      // Create media
      const mediaName = 'TestStartNode';
      await umbracoApi.media.ensureNameNotExists(mediaName);
      const mediaId = await umbracoApi.media.createDefaultMediaWithArticle(mediaName);
      expect(await umbracoApi.media.doesNameExist(mediaName)).toBeTruthy();

      const removedDataTypeValues = [{
        "alias": "startNodeId",
        "value": mediaId
      }];

      // Remove all existing values and add a start node to remove
      dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      dataTypeData.values = removedDataTypeValues;
      await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);

      // Act
      await umbracoUi.dataType.goToDataType(dataTypeName);
      await umbracoUi.dataType.removeMediaStartNode(mediaName);
      await umbracoUi.dataType.clickSaveButton();

      // Assert
      dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      expect(dataTypeData.values).toEqual([]);

      // Clean
      await umbracoApi.media.ensureNameNotExists(mediaName);
    });
  });
}
