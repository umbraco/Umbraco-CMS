import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Member Type tests', () => {
  const memberTypeName = 'TestMemberType';
  const dataTypeName = 'Upload File';
  const groupName = 'TestGroup';
  const tabName = 'TestTab';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.memberType.goToSection(ConstantHelper.sections.settings);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  });

  test('can create an empty member type', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.memberType.clickActionsMenuAtRoot();
    await umbracoUi.memberType.clickCreateThreeDotsButton();
    await umbracoUi.memberType.enterMemberTypeName(memberTypeName);
    await umbracoUi.memberType.clickSaveButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
    expect(await umbracoApi.memberType.doesNameExist(memberTypeName)).toBeTruthy();
  });

  test('can create a member type with a single property', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.memberType.createDefaultMemberType(memberTypeName);

    // Act
    await umbracoUi.memberType.goToMemberType(memberTypeName);
    await umbracoUi.memberType.clickAddGroupButton();
    await umbracoUi.memberType.addPropertyEditor(dataTypeName);
    await umbracoUi.memberType.enterGroupName(groupName);
    await umbracoUi.memberType.clickSaveButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
    expect(await umbracoApi.memberType.doesNameExist(memberTypeName)).toBeTruthy();
    const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
    const dataType = await umbracoApi.dataType.getByName(dataTypeName);
    expect(memberTypeData.properties[0].dataType.id).toBe(dataType.id);
  });

  test('can rename a member type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongName = 'WrongMemberTypeName';
    await umbracoApi.memberType.ensureNameNotExists(wrongName);
    await umbracoApi.memberType.createDefaultMemberType(wrongName);

    // Act
    await umbracoUi.memberType.goToMemberType(wrongName);
    await umbracoUi.memberType.enterMemberTypeName(memberTypeName);
    await umbracoUi.memberType.clickSaveButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
    expect(await umbracoApi.memberType.doesNameExist(memberTypeName)).toBeTruthy();
  });

  test('can update a property in a member type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const newDataTypeName = 'Media Picker';
    await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.memberType.goToMemberType(memberTypeName);
    await umbracoUi.memberType.updatePropertyEditor(newDataTypeName);
    await umbracoUi.memberType.clickSaveButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
    const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
    const dataType = await umbracoApi.dataType.getByName(newDataTypeName);
    expect(memberTypeData.properties[0].dataType.id).toBe(dataType.id);
  });

  test('can update group name in a member type',async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const updatedGroupName = 'UpdatedGroupName';
    await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id, groupName);

    // Act
    await umbracoUi.memberType.goToMemberType(memberTypeName);
    await umbracoUi.memberType.enterGroupName(updatedGroupName);
    await umbracoUi.memberType.clickSaveButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
    const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
    expect(memberTypeData.containers[0].name).toBe(updatedGroupName);
  });

  test('can delete a member type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.memberType.createDefaultMemberType(memberTypeName);

    // Act
    await umbracoUi.memberType.clickRootFolderCaretButton();
    await umbracoUi.memberType.clickActionsMenuForName(memberTypeName);
    await umbracoUi.memberType.clickDeleteButton();
    await umbracoUi.memberType.clickConfirmToDeleteButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
    expect(await umbracoApi.memberType.doesNameExist(memberTypeName)).toBeFalsy();
  });

  test('can delete a property in a member type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id, groupName);

    // Act
    await umbracoUi.memberType.goToMemberType(memberTypeName);
    await umbracoUi.memberType.deletePropertyEditor(dataTypeName);
    await umbracoUi.memberType.clickConfirmToDeleteButton();
    await umbracoUi.memberType.clickSaveButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
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
    await umbracoUi.memberType.enterDescription(descriptionText);
    await umbracoUi.memberType.clickSaveButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
    await umbracoUi.memberType.doesDescriptionHaveValue(descriptionText);
    const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
    expect(memberTypeData.properties[0].description).toBe(descriptionText);
  });

  // TODO: remove .skip when the frontend is ready. Currently, it is not saved when enabling the mandatory slider .
  test.skip('can set a property as mandatory in a member type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id, groupName);

    // Act
    await umbracoUi.memberType.goToMemberType(memberTypeName);
    await umbracoUi.memberType.clickEditorSettingsButton();
    await umbracoUi.memberType.clickMandatorySlider();
    await umbracoUi.memberType.clickUpdateButton();
    await umbracoUi.memberType.clickSaveButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
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
    await umbracoUi.memberType.selectValidationOption('');
    await umbracoUi.memberType.enterRegEx(regex);
    await umbracoUi.memberType.enterRegExMessage(regexMessage);
    await umbracoUi.memberType.clickUpdateButton();
    await umbracoUi.memberType.clickSaveButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
    const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
    expect(memberTypeData.properties[0].validation.regEx).toBe(regex);
    expect(memberTypeData.properties[0].validation.regExMessage).toBe(regexMessage);
  });

  test('can set appearance as label on top for property in a member type', async ({umbracoApi, umbracoUi}) => {
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.memberType.goToMemberType(memberTypeName);
    await umbracoUi.memberType.clickEditorSettingsButton();
    await umbracoUi.memberType.clickLabelOnTopButton();
    await umbracoUi.memberType.clickUpdateButton();
    await umbracoUi.memberType.clickSaveButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
    const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
    expect(memberTypeData.properties[0].appearance.labelOnTop).toBeTruthy();
  });

  
  test.skip('can delete a group in a member type', async ({umbracoApi, umbracoUi}) => {
    // TODO: Implement this test when the front-end is ready. Currently, there is no button for deleting a group.
  });

  test.skip('can create a member type with a property in a tab', async ({umbracoApi, umbracoUi}) => {
    // TODO: Implement this test when the front-end is ready. Currently, it is impossible to adding a tab
  });

  test.skip('can create a member type with properties in a group and a tab', async ({umbracoApi, umbracoUi}) => {
    // TODO: Implement this test when the front-end is ready.
    // There a currently frontend issues, when you try to add a property editor in a tab, you have to click the button twice to focus.
  });

  // TODO: Remove .skip when the front-end is ready. There a currently frontend issues, when you try to add a property editor to the second group. the editor ends up in the first group
  test.skip('can create a member type with multiple groups', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const secondDataTypeName = 'Media Picker';
    await umbracoApi.memberType.createMemberTypeWithPropertyEditor(memberTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.memberType.goToMemberType(memberTypeName);
    await umbracoUi.memberType.clickAddGroupButton();
    await umbracoUi.memberType.addPropertyEditor(secondDataTypeName, 1);
    await umbracoUi.memberType.enterGroupName('TesterGroup', 1);
    await umbracoUi.memberType.clickSaveButton();
  });

  test.skip('can create a member type with multiple tabs', async ({umbracoApi, umbracoUi}) => {
    // TODO: Implement this test when the front-end is ready.
  });

  test.skip('can create a member type with a composition', async ({umbracoApi, umbracoUi}) => {
    // TODO: Implement this test when the front-end is ready. Currently, nothing happens when you press the composition button
  });

  test.skip('can reorder a group in a member type', async ({umbracoApi, umbracoUi}) => {
    // TODO: Implement this test when the front-end is ready. Currently, it is impossible to create two groups
  });

  test.skip('can reorder a property in a member type', async ({umbracoApi, umbracoUi}) => {
    // TODO: Implement this test when the front-end is ready. Currently, it is impossible to reorder
  });

  test('can add an icon to a member type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const iconName = 'icon-bug';
    await umbracoApi.memberType.createDefaultMemberType(memberTypeName);

    // Act
    await umbracoUi.memberType.goToMemberType(memberTypeName);
    await umbracoUi.memberType.updateIcon(iconName);
    await umbracoUi.memberType.clickSubmitButton();
    await umbracoUi.memberType.clickSaveButton();

    // Assert
    await umbracoUi.memberType.isSuccessNotificationVisible();
    const memberTypeData = await umbracoApi.memberType.getByName(memberTypeName);
    expect(memberTypeData.icon).toBe(iconName);
  });
});