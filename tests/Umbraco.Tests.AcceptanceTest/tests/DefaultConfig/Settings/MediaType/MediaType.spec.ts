import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

test.describe('Media Type tests', () => {
  const mediaTypeName = 'TestMediaType';
  const dataTypeName = 'Upload File';
  const groupName = 'TestGroup';
  const tabName = 'TestTab';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName)
    await umbracoUi.goToBackOffice();
    await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName)
  });

  // Design
  // Name and alias is removed when saving
  test.skip('can create an empty media type', async ({page, umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.mediaType.clickActionsMenuForName('Media Types');
    await umbracoUi.mediaType.clickCreateThreeDotsButton();
    await umbracoUi.mediaType.clickNewMediaTypeButton();
    await umbracoUi.mediaType.enterMediaTypeName(mediaTypeName);
    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
  });

  test('can create a media type with a single property', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

    // Act
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    await umbracoUi.mediaType.clickAddGroupButton();
    await umbracoUi.mediaType.addPropertyEditor(dataTypeName);
    await umbracoUi.mediaType.enterMediaTypeGroupName(groupName);
    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
    const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
    const dataType = await umbracoApi.dataType.getByName(dataTypeName);
    // Checks if the correct property was added to the media type
    expect(mediaTypeData.properties[0].dataType.id).toBe(dataType.id);
  });

  test('can rename a media type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongName = 'NotAMediaTypeName';
    await umbracoApi.mediaType.ensureNameNotExists(wrongName);
    await umbracoApi.mediaType.createDefaultMediaType(wrongName);

    // Act
    await umbracoUi.mediaType.goToMediaType(wrongName);
    await umbracoUi.mediaType.enterMediaTypeName(mediaTypeName);
    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeTruthy();
  });

  test('can update alias for a media type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const updatedAlias = 'TestMediaAlias';
    await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

    // Act
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    // Unlocks the alias field
    await page.locator('#name #alias-lock').click();
    // Updates the alias
    await page.locator('#name').getByLabel('alias').clear();
    await page.locator('#name').getByLabel('alias').fill(updatedAlias);

    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
    expect(mediaTypeData.alias).toBe(updatedAlias);
  });

  test('can update a property in a media type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const newDataTypeName = 'Media Picker';
    await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    await umbracoUi.mediaType.updatePropertyEditor(newDataTypeName);
    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
    const dataType = await umbracoApi.dataType.getByName(newDataTypeName);
    // Checks if the correct property was added to the media type
    expect(mediaTypeData.properties[0].dataType.id).toBe(dataType.id);
  });

  test('can update group name in a media type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const updatedGroupName = 'UpdatedGroupName';
    await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id, groupName);

    // Act
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    await umbracoUi.mediaType.enterMediaTypeGroupName(updatedGroupName);
    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
    expect(mediaTypeData.containers[0].name).toBe(updatedGroupName);
  });

  test('can delete a media type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

    // Act
    await umbracoUi.mediaType.clickRootFolderCaretButton();
    await umbracoUi.mediaType.clickActionsMenuForName(mediaTypeName);
    await umbracoUi.mediaType.clickDeleteButton();
    await umbracoUi.mediaType.clickConfirmToDeleteButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    expect(await umbracoApi.mediaType.doesNameExist(mediaTypeName)).toBeFalsy();
  });

  test('can delete a property in a media type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id, groupName);

    // Act
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    const containerName = page.locator('[container-name="TestGroup"]');
    await containerName.hover();
    await containerName.getByLabel('Delete').click({force: true});
    await umbracoUi.mediaType.clickDeleteButton();
    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
    expect(mediaTypeData.properties.length).toBe(0);
  });

  // There is no button for deleting a group in the UI
  test.skip('can delete a group in a media type', async ({page, umbracoApi, umbracoUi}) => {
  });

  // There a currently frontend issues, when you try to add a property editor you have to click the button twice to focus.
  test.skip('can create a media type with a property in a tab', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

    // Act
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    await umbracoUi.mediaType.clickAddTabButton();
    await umbracoUi.mediaType.enterTabName(tabName);
    await umbracoUi.mediaType.addPropertyEditor(dataTypeName);
  });

  // There a currently frontend issues, when you try to add a property editor you have to click the button twice to focus.
  test.skip('can create a media type with properties in a group and a tab', async ({page, umbracoApi, umbracoUi}) => {
  });

  // There a currently frontend issues, when you try to add a property editor to the second group. the editor ends up in the first group
  test.skip('can create a media type with multiple groups', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const secondDataTypeName = 'Media Picker';
    await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(mediaTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    await umbracoUi.mediaType.clickAddGroupButton();
    await umbracoUi.mediaType.addPropertyEditor(secondDataTypeName, 1);
    await umbracoUi.mediaType.enterGroupName('TesterGroup', 1);
    await umbracoUi.mediaType.clickSaveButton();
  });

  // There a currently frontend issues, when you try to add a property editor you have to click the button twice to focus.
  test.skip('can create a media type with multiple tabs', async ({page, umbracoApi, umbracoUi}) => {

  });

  // Nothing happens when you press the composition button
  test.skip('can create a media type with a composition', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const compositionMediaTypeName = 'CompositionMediaType';
    await umbracoApi.mediaType.ensureNameNotExists(compositionMediaTypeName);
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const compositionMediaTypeId = await umbracoApi.mediaType.createMediaTypeWithPropertyEditor(compositionMediaTypeName, dataTypeName, dataTypeData.id, groupName);
    await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

    // Act
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    await umbracoUi.mediaType.clickCompositionsButton();
    await umbracoUi.mediaType.clickButtonWithName(compositionMediaTypeName)
    // This is needed
    await umbracoUi.mediaType.clickSubmitButton();
    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    expect(umbracoUi.mediaType.doesGroupHaveValue(groupName)).toBeTruthy();
    // Checks if the composition in the media type is correct
    const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
    expect(mediaTypeData.compositions[0].mediaType.id).toBe(compositionMediaTypeId);

    // Clean
    await umbracoApi.mediaType.ensureNameNotExists(compositionMediaTypeName);
  });

  // Not possible
  test.skip('can reorder a group in a media type', async ({page, umbracoApi, umbracoUi}) => {
  });

  // It's not possible to reorder
  test.skip('can reorder a property in a media type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const dataTypeNameTwo = "Upload Second File";
    await umbracoApi.mediaType.createMediaTypeWithTwoPropertyEditors(mediaTypeName, dataTypeName, dataTypeData.id, dataTypeNameTwo, dataTypeData.id);

    // Act
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);

    await umbracoUi.mediaType.clickReorderButton();
    // Drag and Drop
    const dragFromLocator = page.getByText(dataTypeNameTwo);
    const dragToLocator = page.getByText(dataTypeName);
    await umbracoUi.mediaType.dragAndDrop(dragFromLocator, dragToLocator, 0, 0, 5);
    await umbracoUi.mediaType.clickIAmDoneReorderingButton();
    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
    expect(mediaTypeData.properties[0].name).toBe(dataTypeNameTwo);
  });

// Structure
  test('can create a media type with allow as root enabled', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

    // Act
    await umbracoUi.mediaType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.mediaType.goToMediaType(mediaTypeName);
    await umbracoUi.mediaType.clickStructureTab();
    await umbracoUi.mediaType.clickAllowAsRootButton();
    await umbracoUi.mediaType.clickSaveButton();

    // Assert
    await umbracoUi.mediaType.isSuccessNotificationVisible();
    const documentTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
    expect(documentTypeData.allowedAsRoot).toBeTruthy();
  });

  // It is currently not possible to add a childNodeType to a mediaType
  test.skip('can create a media type with an allowed child node type', async ({page, umbracoApi, umbracoUi}) => {
  });

  // It is currently not possible to add a childNodeType to a mediaType
  test.skip('can create a media type with multiple allowed child nodes types', async ({page, umbracoApi, umbracoUi}) => {
  });

  // It is currently not possible enable display children in a collection view
  test.skip('can create a media type with display children in a collection view enabled', async ({page, umbracoApi, umbracoUi}) => {
  });
});
