import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Richtext editor';
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

test('can enable ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedDataTypeValues = {
    "alias": "ignoreUserStartNodes",
    "value": true
  };
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.clickIgnoreUserStartNodesSlider();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

// TODO: Remove skip when the front-end is ready. Currently it is impossible to uncheck all the toolbar options.
test.skip('can enable all toolbar options', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const toolbarValues: string[] = ["undo", "redo", "cut", "copy"];
  const expectedDataTypeValues = [
    {
      "alias": "toolbar",
      "value": toolbarValues
    }
  ];

  // Remove all existing values
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeData.values = [];
  await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.pickTheToolbarOptionByValue(toolbarValues);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toEqual(expectedDataTypeValues);
});

// TODO: Remove skip when the front-end is ready
test.skip('can add stylesheet', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const stylesheetName = 'StylesheetForDataType.css';
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  const stylesheetPath = await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);

  const expectedDataTypeValues = {
    "alias": "stylesheets",
    "value": [stylesheetPath]
  };

  await umbracoUi.dataType.goToDataType(dataTypeName);
  
  // Act
  await umbracoUi.dataType.addStylesheet(stylesheetName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);

  // Clean
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
});

test('can add dimensions', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const width = 100;
  const height = 10;
  const expectedDataTypeValues = {
    "alias": "dimensions",
    "value": {
      "width": width,
      "height": height
    }
  };
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.enterDimensionsValue(width.toString(), height.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can add maximum size for inserted images', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const maxImageSize = 300;
  const expectedDataTypeValues = {
    "alias": "maxImageSize",
    "value": maxImageSize
  };
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.enterMaximumSizeForImages(maxImageSize.toString());
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can select overlay size', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const overlaySizeValue = 'large';
  const expectedDataTypeValues = {
    "alias": "overlaySize",
    "value": overlaySizeValue
  };
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.chooseOverlaySizeByValue(overlaySizeValue);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can enable hide label', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedDataTypeValues = {
    "alias": "hideLabel",
    "value": true
  };
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.clickHideLabelSlider();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can add image upload folder', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaFolderName = 'TestMediaFolder';
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
  const mediaFolderId = await umbracoApi.media.createDefaultMediaFolder(mediaFolderName);
  const expectedDataTypeValues = {
    "alias": "mediaParentId",
    "value": mediaFolderId
  };
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.addImageUploadFolder(mediaFolderName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
});

test('can enable inline editing mode', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mode = 'Inline';
  const expectedDataTypeValues = {
    "alias": "mode",
    "value": mode
  };
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.clickInlineRadioButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can add an available block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeName = 'TestElementType';
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const expectedDataTypeValues = {
    alias: "blocks",
    value: [
      {
        contentElementTypeKey: elementTypeId,
      }
    ]
  };
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.addAvailableBlocks(elementTypeName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);

  // Clean
  await umbracoApi.documentType.delete(elementTypeId);
});
