import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Content Picker';
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

test('can show open button', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedDataTypeValues = {
    "alias": "showOpenButton",
    "value": true
  };
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.clickShowOpenButtonToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can ignore user start nodes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedDataTypeValues = {
    "alias": "ignoreUserStartNodes",
    "value": true
  };
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.clickIgnoreUserStartNodesToggle();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);
});

test('can add start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create content
  const documentTypeName = 'TestDocumentType';
  const contentName = 'TestStartNode';
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  expect(await umbracoApi.document.doesExist(contentId)).toBeTruthy();

  const expectedDataTypeValues = {
    "alias": "startNodeId",
    "value": contentId
  };
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.clickChooseButton();
  await umbracoUi.dataType.addContentStartNode(contentName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toContainEqual(expectedDataTypeValues);

  // Clean
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can remove start node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create content
  const documentTypeName = 'TestDocumentType';
  const contentName = 'TestStartNode';
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  expect(await umbracoApi.document.doesExist(contentId)).toBeTruthy();

  const removedDataTypeValues = [{
    "alias": "startNodeId",
    "value": contentId
  }];

  // Remove all existing values and add a start node to remove
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeData.values = removedDataTypeValues;
  await umbracoApi.dataType.update(dataTypeData.id, dataTypeData);
  await umbracoUi.dataType.goToDataType(dataTypeName);

  // Act
  await umbracoUi.dataType.removeContentStartNode(contentName);
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  expect(dataTypeData.values).toEqual([]);

  // Clean
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});
