import {ConstantHelper, test} from "@umbraco/acceptance-test-helpers";
import {expect} from "@playwright/test";

const memberTypeName = 'TestMemberType';
const dataTypeName = 'Approved Color';
const groupName = 'TestGroup';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberType.goToSection(ConstantHelper.sections.settings);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
});

test('can create a member type with a property', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.memberType.createDefaultMemberType(memberTypeName);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.memberType.clickAddGroupButton();
  await umbracoUi.memberType.addPropertyEditor(dataTypeName);
  await umbracoUi.memberType.enterGroupName(groupName);
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.memberType.doesNameExist(memberTypeName)).toBeTruthy();
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  expect(memberTypeData.properties[0].dataType.id).toBe(dataType.id);
});

test('can update a property in a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const newDataTypeName = 'Image Media Picker';
  await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.memberType.updatePropertyEditor(newDataTypeName);
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  const dataType = await umbracoApi.dataType.getByName(newDataTypeName);
  expect(memberTypeData.properties[0].dataType.id).toBe(dataType.id);
});

test('can update group name in a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const updatedGroupName = 'UpdatedGroupName';
  await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id, groupName);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.memberType.enterGroupName(updatedGroupName);
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.containers[0].name).toBe(updatedGroupName);
});

test('can delete a property in a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id, groupName);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.memberType.deletePropertyEditorWithName(dataTypeName);
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.properties.length).toBe(0);
});

test('can add a description to property in a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const descriptionText = 'Test Description';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id, groupName);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.memberType.clickEditorSettingsButton();
  await umbracoUi.memberType.enterPropertyEditorDescription(descriptionText);
  await umbracoUi.memberType.clickSubmitButton();
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  await expect(umbracoUi.memberType.enterDescriptionTxt).toBeVisible();
  await umbracoUi.memberType.doesDescriptionHaveValue(descriptionText);
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.properties[0].description).toBe(descriptionText);
});

test('can set a property as mandatory in a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.memberType.clickEditorSettingsButton();
  await umbracoUi.memberType.clickMandatoryToggle();
  await umbracoUi.memberType.clickSubmitButton();
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.properties[0].validation.mandatory).toBeTruthy();
});

test('can set up validation for a property in a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const regex = '^[a-zA-Z0-9]*$';
  const regexMessage = 'Only letters and numbers are allowed';
  await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.memberType.clickEditorSettingsButton();
  await umbracoUi.memberType.selectValidationOption('.+');
  await umbracoUi.memberType.enterRegEx(regex);
  await umbracoUi.memberType.enterRegExMessage(regexMessage);
  await umbracoUi.memberType.clickSubmitButton();
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.properties[0].validation.regEx).toBe(regex);
  expect(memberTypeData.properties[0].validation.regExMessage).toBe(regexMessage);
});

test('can set appearance as label on top for property in a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.memberType.clickEditorSettingsButton();
  await umbracoUi.memberType.clickLabelAboveButton();
  await umbracoUi.memberType.clickSubmitButton();
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.properties[0].appearance.labelOnTop).toBeTruthy();
});

test('can delete a group in a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id, groupName);

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.memberType.deleteGroup(groupName);
  await umbracoUi.memberType.clickConfirmToDeleteButton();
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.containers.length).toBe(0);
  expect(memberTypeData.properties.length).toBe(0);
});

test('can create a member type with multiple groups', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondDataTypeName = 'Image Media Picker';
  await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id, groupName);
  const secondGroupName = 'TesterGroup';

  // Act
  await umbracoUi.memberType.goToMemberType(memberTypeName);
  await umbracoUi.memberType.clickAddGroupButton();
  await umbracoUi.memberType.enterGroupName(secondGroupName, 1);
  await umbracoUi.memberType.addPropertyEditor(secondDataTypeName, 2);
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  expect(await umbracoApi.memberType.doesNameExist(memberTypeName)).toBeTruthy();
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.properties.length).toBe(2);
  expect(memberTypeData.containers.length).toBe(2);
});

test('can reorder properties in a member type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const dataTypeNameTwo = 'Second Approved Color';
  await umbracoApi.memberType.createMemberTypeWithTwoPropertyEditors(memberTypeName, dataTypeName, dataTypeData.id, dataTypeNameTwo, dataTypeData.id);
  await umbracoUi.memberType.goToMemberType(memberTypeName);

  // Act
  await umbracoUi.memberType.clickReorderButton();
  const dragFromLocator = umbracoUi.memberType.getTextLocatorWithName(dataTypeNameTwo);
  const dragToLocator = umbracoUi.memberType.getTextLocatorWithName(dataTypeName);
  await umbracoUi.memberType.dragAndDrop(dragFromLocator, dragToLocator);
  await umbracoUi.memberType.clickIAmDoneReorderingButton();
  await umbracoUi.memberType.clickSaveButtonAndWaitForMemberTypeToBeUpdated();

  // Assert
  const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
  expect(memberTypeData.properties[0].name).toBe(dataTypeNameTwo);
});
