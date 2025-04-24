import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const blockGridEditorName = 'TestBlockGridEditor';
const elementTypeName = 'BlockGridElement';
const dataTypeName = 'Textstring';
const groupName = 'testGroup';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockGridEditorName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSettingsTreeItem('Data Types');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.dataType.ensureNameNotExists(blockGridEditorName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can update grid columns for areas for a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const gridColumns = 6;
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.enterGridColumnsForArea(gridColumns);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaGridColumns(blockGridEditorName, contentElementTypeId, gridColumns)).toBeTruthy();
});

test('can add an area for a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithABlock(blockGridEditorName, contentElementTypeId);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.addAreaButton();
  await umbracoUi.dataType.clickAreaSubmitButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithAlias(blockGridEditorName, contentElementTypeId)).toBeTruthy();
});

// TODO: There are currently issues when trying to select the locator.
test.skip('can resize an area for a block', async ({umbracoApi, umbracoUi}) => {
// Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
});

test('can update alias an area for a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const newAlias = 'NewAlias';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.goToAreaByAlias(areaAlias);
  await umbracoUi.dataType.enterAreaAlias(newAlias);
  await umbracoUi.dataType.clickAreaSubmitButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithAlias(blockGridEditorName, contentElementTypeId, newAlias)).toBeTruthy();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaCount(blockGridEditorName, contentElementTypeId, 1)).toBeTruthy();
});

test('can remove an area for a block', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithAlias(blockGridEditorName, contentElementTypeId, areaAlias)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.clickRemoveAreaByAlias(areaAlias);
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithAlias(blockGridEditorName, contentElementTypeId, areaAlias)).toBeFalsy();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaCount(blockGridEditorName, contentElementTypeId, 0)).toBeTruthy();
});

test('can add multiple areas for a block', async ({umbracoApi, umbracoUi}) => {
// Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaCount(blockGridEditorName, contentElementTypeId, 1)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.addAreaButton();
  await umbracoUi.dataType.clickAreaSubmitButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithAlias(blockGridEditorName, contentElementTypeId)).toBeTruthy();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithAlias(blockGridEditorName, contentElementTypeId, areaAlias)).toBeTruthy();
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaCount(blockGridEditorName, contentElementTypeId, 2)).toBeTruthy();
});

test('can add create button label for an area in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const createButtonLabel = 'CreateButtonLabel';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.goToAreaByAlias(areaAlias);
  await umbracoUi.dataType.enterCreateButtonLabelInArea(createButtonLabel);
  await umbracoUi.dataType.clickAreaSubmitButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithCreateButtonLabel(blockGridEditorName, contentElementTypeId, areaAlias, createButtonLabel)).toBeTruthy();
});

test('can remove create button label for an area in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const createButtonLabel = 'CreateButtonLabel';
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias, createButtonLabel);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.goToAreaByAlias(areaAlias);
  await umbracoUi.dataType.enterCreateButtonLabelInArea('');
  await umbracoUi.dataType.clickAreaSubmitButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithCreateButtonLabel(blockGridEditorName, contentElementTypeId, areaAlias, '')).toBeTruthy();
});

test('can add min allowed for an area in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const minAllowed = 3;
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.goToAreaByAlias(areaAlias);
  await umbracoUi.dataType.enterMinAllowedInArea(minAllowed);
  await umbracoUi.dataType.clickAreaSubmitButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithMinAllowed(blockGridEditorName, contentElementTypeId, areaAlias, minAllowed)).toBeTruthy();
});

test('can remove min allowed for an area in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const minAllowed = 6;
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias, null, undefined, undefined, minAllowed);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithMinAllowed(blockGridEditorName, contentElementTypeId, areaAlias, minAllowed)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.goToAreaByAlias(areaAlias);
  await umbracoUi.dataType.enterMinAllowedInArea(undefined);
  await umbracoUi.dataType.clickAreaSubmitButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithMinAllowed(blockGridEditorName, contentElementTypeId, areaAlias, minAllowed)).toBeFalsy();
});

//TODO: Frontend issue. when value is inserted to the min or max, it is set as a string instead of number
test.skip('can add add max allowed for an area in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const maxAllowed = 7;
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.goToAreaByAlias(areaAlias);
  await umbracoUi.dataType.enterMaxAllowedInArea(maxAllowed);
  await umbracoUi.dataType.clickAreaSubmitButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithMaxAllowed(blockGridEditorName, contentElementTypeId, areaAlias, maxAllowed)).toBeTruthy();
});

test('can remove max allowed for an area in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const areaAlias = 'TestArea';
  const maxAllowed = 7;
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias, null, undefined, undefined, undefined, maxAllowed);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithMaxAllowed(blockGridEditorName, contentElementTypeId, areaAlias, maxAllowed)).toBeTruthy();

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.goToAreaByAlias(areaAlias);
  await umbracoUi.dataType.enterMaxAllowedInArea(undefined);
  await umbracoUi.dataType.clickAreaSubmitButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
  expect(await umbracoApi.dataType.doesBlockEditorBlockContainAreaWithMaxAllowed(blockGridEditorName, contentElementTypeId, areaAlias, maxAllowed)).toBeFalsy();
});

// TODO: Remove skip when the front-end is ready. Currently there is no frontend validation for min and max values
test.skip('min can not be more than max an area in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const areaAlias = 'TestArea';
  const minAllowed = 6;
  const maxAllowed = 7;
  const newMinAllowed = 8;
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias, null, undefined, undefined, minAllowed, maxAllowed);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.goToAreaByAlias(areaAlias);
  await umbracoUi.dataType.enterMinAllowedInArea(newMinAllowed);
  await umbracoUi.dataType.clickAreaSubmitButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
});

test('can add specified allowance for an area in a block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const areaAlias = 'TestArea';
  const textStringData = await umbracoApi.dataType.getByName(dataTypeName);
  const contentElementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, dataTypeName, textStringData.id);
  await umbracoApi.dataType.createBlockGridWithAnAreaInABlock(blockGridEditorName, contentElementTypeId, areaAlias);

  // Act
  await umbracoUi.dataType.goToDataType(blockGridEditorName);
  await umbracoUi.dataType.goToBlockWithName(elementTypeName);
  await umbracoUi.dataType.goToBlockAreasTab();
  await umbracoUi.dataType.goToAreaByAlias(areaAlias);
  await umbracoUi.dataType.clickAddSpecifiedAllowanceButton();
  await umbracoUi.dataType.clickAreaSubmitButton();
  await umbracoUi.dataType.clickSubmitButton();
  await umbracoUi.dataType.clickSaveButton();

  // Assert
  //await umbracoUi.dataType.isSuccessNotificationVisible();
  await umbracoUi.dataType.isErrorNotificationVisible(false);
});

// TODO: It is currently not possible to add a specified allowance
test.skip('can update specified allowance for an area in a block', async ({umbracoApi, umbracoUi}) => {

});

// TODO: It is currently not possible to add a specified allowance
test.skip('can remove specified allowance for an area in a block', async ({umbracoApi, umbracoUi}) => {

});

// TODO: It is currently not possible to add a specified allowance
test.skip('can add multiple specified allowances for an area in a block', async ({umbracoApi, umbracoUi}) => {

});

// TODO: It is currently not possible to add a specified allowance
test.skip('can add specified allowance with min and max for an area in a block', async ({umbracoApi, umbracoUi}) => {
});

// TODO: It is currently not possible to add a specified allowance
test.skip('can remove min and max from specified allowance for an area in a block', async ({umbracoApi, umbracoUi}) => {

});
