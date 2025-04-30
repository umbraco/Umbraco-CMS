import {ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

const mediaTypeName = 'TestMediaType';
const dataTypeName = 'Upload File';
const groupName = 'TestGroup';
const tabName = 'TestTab';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
});

test('can create a media type with a property', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickAddGroupButton();
  await umbracoUi.mediaType.addPropertyEditor(dataTypeName);
  await umbracoUi.mediaType.enterGroupName(groupName);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  // Checks if the correct property was added to the media type
  expect(mediaTypeData.properties[0].dataType.id).toBe(dataType.id);
});

test('can update a property in a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const newDataTypeName = 'Image Media Picker';
  await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.updatePropertyEditor(newDataTypeName);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  const dataType = await umbracoApi.dataType.getByName(newDataTypeName);
  // Checks if the correct property was added to the media type
  expect(mediaTypeData.properties[0].dataType.id).toBe(dataType.id);
});

test('can update group name in a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const updatedGroupName = 'UpdatedGroupName';
  await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id, groupName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.enterGroupName(updatedGroupName);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.containers[0].name).toBe(updatedGroupName);
});

test('can delete a property in a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id, groupName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.deletePropertyEditorWithName(dataTypeName);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.properties.length).toBe(0);
});

test('can add a description to property in a media type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const descriptionText = 'Test Description';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id, groupName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickEditorSettingsButton();
  await umbracoUi.mediaType.enterPropertyEditorDescription(descriptionText);
  await umbracoUi.mediaType.clickSubmitButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  await expect(umbracoUi.mediaType.enterDescriptionTxt).toBeVisible();
  expect(umbracoUi.mediaType.doesDescriptionHaveValue(descriptionText)).toBeTruthy();
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.properties[0].description).toBe(descriptionText);
});

test('can set a property as mandatory in a media type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickEditorSettingsButton();
  await umbracoUi.mediaType.clickMandatoryToggle();
  await umbracoUi.mediaType.clickSubmitButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.properties[0].validation.mandatory).toBeTruthy();
});

test('can set up validation for a property in a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const regex = '^[a-zA-Z0-9]*$';
  const regexMessage = 'Only letters and numbers are allowed';
  await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickEditorSettingsButton();
  await umbracoUi.mediaType.selectValidationOption('');
  await umbracoUi.mediaType.enterRegEx(regex);
  await umbracoUi.mediaType.enterRegExMessage(regexMessage);
  await umbracoUi.mediaType.clickSubmitButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.properties[0].validation.regEx).toBe(regex);
  expect(mediaTypeData.properties[0].validation.regExMessage).toBe(regexMessage);
});

test('can set appearance as label on top for property in a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickEditorSettingsButton();
  await umbracoUi.mediaType.clickLabelAboveButton();
  await umbracoUi.mediaType.clickSubmitButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.properties[0].appearance.labelOnTop).toBeTruthy();
});

test('can delete a group in a media type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id, groupName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.deleteGroup(groupName);
  await umbracoUi.mediaType.clickConfirmToDeleteButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.containers.length).toBe(0);
  expect(mediaTypeData.properties.length).toBe(0);
});

test('can create a media type with a property in a tab', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickAddTabButton();
  await umbracoUi.mediaType.enterTabName(tabName);
  // TODO: We need to wait to make sure the Tab name has been entered, otherwise the test can be flaky and the property editor is not added to the correctly
  await umbracoUi.waitForTimeout(500);
  await umbracoUi.mediaType.addPropertyEditor(dataTypeName);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  // Checks if the media type has the correct tab and property
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(await umbracoApi.mediaType.doesTabContainerCorrectPropertyEditor(mediaTypeName, tabName, mediaTypeData.properties[0].dataType.id)).toBeTruthy();
});

test('can create a media type with multiple groups', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondDataTypeName = 'Image Media Picker';
  await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id, groupName);
  const secondDataType = await umbracoApi.dataType.getByName(secondDataTypeName);
  const secondGroupName = 'TesterGroup';

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickAddGroupButton();
  await umbracoUi.mediaType.enterGroupName(secondGroupName, 1);
  await umbracoUi.mediaType.addPropertyEditor(secondDataTypeName, 1);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
  expect(await umbracoApi.mediaType.doesGroupContainCorrectPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id, groupName)).toBeTruthy();
  expect(await umbracoApi.mediaType.doesGroupContainCorrectPropertyEditor(mediaTypeName, secondDataTypeName, secondDataType.id, secondGroupName)).toBeTruthy();
});

test('can create a media type with multiple tabs', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondDataTypeName = 'Image Media Picker';
  const secondGroupName = 'TesterGroup';
  const secondTabName = 'SecondTab';
  await umbracoApi.mediaType.createMediaTypeWithPropertyEditorInTab(mediaTypeName, dataTypeName, dataTypeData.id, tabName, groupName);
  const secondDataType = await umbracoApi.dataType.getByName(secondDataTypeName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickAddTabButton();
  await umbracoUi.mediaType.enterTabName(secondTabName);
  await umbracoUi.mediaType.clickAddGroupButton();
  await umbracoUi.mediaType.enterGroupName(secondGroupName);
  await umbracoUi.mediaType.addPropertyEditor(secondDataTypeName, 1);
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
  expect(await umbracoApi.mediaType.doesTabContainCorrectPropertyEditorInGroup(mediaTypeName, dataTypeName, dataTypeData.id, tabName, groupName)).toBeTruthy();
  expect(await umbracoApi.mediaType.doesTabContainCorrectPropertyEditorInGroup(mediaTypeName, secondDataTypeName, secondDataType.id, secondTabName, secondGroupName)).toBeTruthy();
});

test('can delete a tab from a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.mediaType.createMediaTypeWithPropertyEditorInTab(mediaTypeName, dataTypeName, dataTypeData.id, tabName, groupName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.mediaType.clickRemoveTabWithName(tabName);
  await umbracoUi.mediaType.clickConfirmToDeleteButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
});

test('can create a media type with a composition', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const compositionMediaTypeName = 'CompositionMediaType';
  await umbracoApi.mediaType.ensureNameNotExists(compositionMediaTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const compositionMediaTypeId = await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(compositionMediaTypeName, dataTypeName, dataTypeData.id, groupName);
  await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

  // Act
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);
  await umbracoUi.waitForTimeout(500);
  await umbracoUi.mediaType.clickCompositionsButton();
  await umbracoUi.mediaType.clickButtonWithName(compositionMediaTypeName);
  await umbracoUi.mediaType.clickSubmitButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  expect(umbracoUi.mediaType.doesGroupHaveValue(groupName)).toBeTruthy();
  // Checks if the composition in the media type is correct
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.compositions[0].mediaType.id).toBe(compositionMediaTypeId);

  // Clean
  await umbracoApi.mediaType.ensureNameNotExists(compositionMediaTypeName);
});

test('can reorder groups in a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondGroupName = 'SecondGroup';
  await umbracoApi.mediaType.createMediaTypeWithTwoGroups(mediaTypeName, dataTypeName, dataTypeData.id, groupName, secondGroupName);
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);

  // Act
  await umbracoUi.mediaType.clickReorderButton();
  const groupValues = await umbracoUi.mediaType.reorderTwoGroups();
  const firstGroupValue = groupValues.firstGroupValue;
  const secondGroupValue = groupValues.secondGroupValue;
  await umbracoUi.mediaType.clickIAmDoneReorderingButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  // Since we swapped sorting order, the firstGroupValue should have sortOrder 1 and the secondGroupValue should have sortOrder 0
  expect(await umbracoApi.mediaType.doesMediaTypeGroupNameContainCorrectSortOrder(mediaTypeName, secondGroupValue, 0)).toBeTruthy();
  expect(await umbracoApi.mediaType.doesMediaTypeGroupNameContainCorrectSortOrder(mediaTypeName, firstGroupValue, 1)).toBeTruthy();
});

test('can reorder properties in a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const dataTypeNameTwo = "Upload Second File";
  await umbracoApi.mediaType.createMediaTypeWithTwoPropertyEditors(mediaTypeName, dataTypeName, dataTypeData.id, dataTypeNameTwo, dataTypeData.id);
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);

  // Act
  await umbracoUi.mediaType.clickReorderButton();
  // Drag and Drop
  const dragFromLocator = umbracoUi.mediaType.getTextLocatorWithName(dataTypeNameTwo);
  const dragToLocator = umbracoUi.mediaType.getTextLocatorWithName(dataTypeName);
  await umbracoUi.mediaType.dragAndDrop(dragFromLocator, dragToLocator);
  await umbracoUi.mediaType.clickIAmDoneReorderingButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  expect(mediaTypeData.properties[0].name).toBe(dataTypeNameTwo);
});

// TODO: Remove skip when the frontend is ready. Currently it is impossible to reorder tab by drag and drop
test.skip('can reorder tabs in a media type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondTabName = 'SecondTab';
  await umbracoApi.mediaType.createMediaTypeWithTwoTabs(mediaTypeName, dataTypeName, dataTypeData.id, tabName, secondTabName);
  await umbracoUi.mediaType.goToMediaType(mediaTypeName);

  // Act
  const dragToLocator = umbracoUi.mediaType.getTabLocatorWithName(tabName);
  const dragFromLocator = umbracoUi.mediaType.getTabLocatorWithName(secondTabName);
  await umbracoUi.mediaType.clickReorderButton();
  await umbracoUi.mediaType.dragAndDrop(dragFromLocator, dragToLocator, 0, 0, 10);
  await umbracoUi.mediaType.clickIAmDoneReorderingButton();
  await umbracoUi.mediaType.clickSaveButton();

  // Assert
  //await umbracoUi.mediaType.isSuccessNotificationVisible();
  await umbracoUi.mediaType.isErrorNotificationVisible(false);
  expect(await umbracoApi.mediaType.doesMediaTypeTabNameContainCorrectSortOrder(mediaTypeName, secondTabName, 0)).toBeTruthy();
  expect(await umbracoApi.mediaType.doesMediaTypeTabNameContainCorrectSortOrder(mediaTypeName, tabName, 1)).toBeTruthy();
});
