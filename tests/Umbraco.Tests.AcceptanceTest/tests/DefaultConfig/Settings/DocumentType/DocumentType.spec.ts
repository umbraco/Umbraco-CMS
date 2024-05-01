import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

test.describe('Document Type tests', () => {
  const documentTypeName = 'TestDocumentType';
  const dataTypeName = 'Approved Color';
  const groupName = 'TestGroup';
  const tabName = 'TestTab';

  test.beforeEach(async ({umbracoUi, umbracoApi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoUi.goToBackOffice();
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  test('can create a document type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

    // Act
    await umbracoUi.documentType.clickActionsMenuAtRoot();
    await page.pause();
    await umbracoUi.documentType.clickCreateButton();
    await umbracoUi.documentType.clickCreateDocumentTypeButton();
    await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
    await umbracoUi.documentType.reloadTree('Document Types')
    await umbracoUi.documentType.isDocumentTreeItemVisible(documentTypeName);
  });

  test('can create a document type with a template', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoApi.template.ensureNameNotExists(documentTypeName);

    // Act
    await umbracoUi.documentType.clickActionsMenuAtRoot();
    await umbracoUi.documentType.clickCreateButton();
    await umbracoUi.documentType.clickCreateDocumentTypeWithTemplateButton();
    await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
    // Checks if the documentType contains the template
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    const templateData = await umbracoApi.template.getByName(documentTypeName);
    expect(documentTypeData.allowedTemplates[0].id).toEqual(templateData.id);

    // Clean
    await umbracoApi.template.ensureNameNotExists(documentTypeName);
  });

  test('can create a element type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

    // Act
    await umbracoUi.documentType.clickActionsMenuAtRoot();
    await umbracoUi.documentType.clickCreateButton();
    await umbracoUi.documentType.clickCreateElementTypeButton();
    await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
    // Checks if the isElement is true
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    expect(documentTypeData.isElement).toBeTruthy();
  });

  test.describe('Design tab', () => {
    test('can rename a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const wrongName = 'NotADocumentTypeName';
      await umbracoApi.documentType.ensureNameNotExists(wrongName);
      await umbracoApi.documentType.createDefaultDocumentType(wrongName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(wrongName);
      await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
      await umbracoUi.documentType.isDocumentTreeItemVisible(wrongName, false);
      await umbracoUi.documentType.isDocumentTreeItemVisible(documentTypeName);
    });

    test('can update the alias for a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const oldAlias = AliasHelper.toAlias(documentTypeName);
      const newAlias = 'newDocumentTypeAlias';
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      const documentTypeDataOld = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeDataOld.alias).toBe(oldAlias);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.enterAliasName(newAlias);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      await umbracoUi.documentType.isDocumentTreeItemVisible(documentTypeName, true);
      const documentTypeDataNew = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeDataNew.alias).toBe(newAlias);
    });

    test('can add an icon for a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const bugIcon = 'icon-bug';
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.updateIcon(bugIcon);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.icon).toBe(bugIcon);
      await umbracoUi.documentType.isDocumentTreeItemVisible(documentTypeName, true);
    });

    test('can add a property to a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickAddGroupButton();
      await umbracoUi.documentType.addPropertyEditor(dataTypeName);
      await umbracoUi.documentType.enterDocumentTypeGroupName(groupName);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      const dataType = await umbracoApi.dataType.getByName(dataTypeName);
      // Checks if the correct property was added to the document type
      expect(documentTypeData.properties[0].dataType.id).toBe(dataType.id);
    });

    test('can update a property in a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      const newDataTypeName = 'Image Media Picker';
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.updatePropertyEditor(newDataTypeName);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      const dataType = await umbracoApi.dataType.getByName(newDataTypeName);
      // Checks if the correct property was added to the document type
      expect(documentTypeData.properties[0].dataType.id).toBe(dataType.id);
    });

    test('can update group name in a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      const newGroupName = 'UpdatedGroupName';
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.enterDocumentTypeGroupName(newGroupName);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.containers[0].name).toBe(newGroupName);
    });

    // TODO: It is currently not possible to delete a group
    test.skip('can delete a group in a document type', async ({page, umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
      const groupId = await umbracoApi.documentType.getContainerIdWithName(documentTypeName, groupName);
      console.log(groupId);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      console.log(documentTypeData);
    });

    // TODO: Currently I am getting an error If I delete a tab that contains children. The children are not cleaned up when deleting the tab.
    test.skip('can delete a tab in a document type', async ({page, umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTab(documentTypeName, dataTypeName, dataTypeData.id, tabName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await page.locator('[label="' + tabName + '"] [label="Remove"]').click();
      await umbracoUi.documentType.clickConfirmToDeleteButton();
      await umbracoUi.documentType.clickSaveButton();

      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
    });

    test('can delete a property editor in a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.deletePropertyEditorInDocumentTypeWithName(dataTypeName);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.properties.length).toBe(0);
    });

    test('can create a document type with a property in a tab', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickAddTabButton();
      await umbracoUi.documentType.enterTabName(tabName);
      await umbracoUi.documentType.clickAddGroupButton();
      await umbracoUi.documentType.addPropertyEditor(dataTypeName);
      await umbracoUi.documentType.enterDocumentTypeGroupName(groupName);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(await umbracoApi.documentType.doesTabContainCorrectPropertyEditorInGroup(documentTypeName, dataTypeName, documentTypeData.properties[0].dataType.id, tabName, groupName)).toBeTruthy();
    });

    test('can create a document type with multiple groups', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      const secondDataTypeName = 'Image Media Picker';
      const secondGroupName = 'TesterGroup';
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName);
      const secondDataType = await umbracoApi.dataType.getByName(secondDataTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickAddGroupButton();
      await umbracoUi.documentType.enterDocumentTypeGroupName(secondGroupName, 1);
      await umbracoUi.documentType.addPropertyEditor(secondDataTypeName, 1);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
      expect(await umbracoApi.documentType.doesGroupContainCorrectPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName)).toBeTruthy();
      expect(await umbracoApi.documentType.doesGroupContainCorrectPropertyEditor(documentTypeName, secondDataTypeName, secondDataType.id, secondGroupName)).toBeTruthy();
    });

    test('can create a document type with multiple tabs', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      const secondDataTypeName = 'Image Media Picker';
      const secondGroupName = 'TesterGroup';
      const secondTabName = 'SecondTab';
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditorInTab(documentTypeName, dataTypeName, dataTypeData.id, tabName, groupName);
      const secondDataType = await umbracoApi.dataType.getByName(secondDataTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickAddTabButton();
      await umbracoUi.documentType.enterTabName(secondTabName);
      await umbracoUi.documentType.clickAddGroupButton();
      await umbracoUi.documentType.enterDocumentTypeGroupName(secondGroupName);
      await umbracoUi.documentType.addPropertyEditor(secondDataTypeName);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
      expect(await umbracoApi.documentType.doesTabContainCorrectPropertyEditorInGroup(documentTypeName, dataTypeName, dataTypeData.id, tabName, groupName)).toBeTruthy();
      expect(await umbracoApi.documentType.doesTabContainCorrectPropertyEditorInGroup(documentTypeName, secondDataTypeName, secondDataType.id, secondTabName, secondGroupName)).toBeTruthy();
    });

    test('can create a document type with a composition', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const compositionDocumentTypeName = 'CompositionDocumentType';
      await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickCompositionsButton();
      await umbracoUi.documentType.clickButtonWithName(compositionDocumentTypeName);
      await umbracoUi.documentType.clickSubmitButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(umbracoUi.documentType.doesGroupHaveValue(groupName)).toBeTruthy();
      // Checks if the composition in the document type is correct
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.compositions[0].documentType.id).toBe(compositionDocumentTypeId);

      // Clean
      await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
    });

    test('can remove a composition form a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const compositionDocumentTypeName = 'CompositionDocumentType';
      await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
      await umbracoApi.documentType.createDocumentTypeWithAComposition(documentTypeName, compositionDocumentTypeId);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickCompositionsButton();
      await umbracoUi.documentType.clickButtonWithName(compositionDocumentTypeName);
      await umbracoUi.documentType.clickSubmitButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoUi.documentType.doesGroupHaveValue(groupName)).toBeFalsy();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.compositions).toEqual([]);

      // Clean
      await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
    });

    test('can reorder groups in a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      const secondGroupName = 'SecondGroup';
      await umbracoApi.documentType.createDocumentTypeWithTwoGroups(documentTypeName, dataTypeName, dataTypeData.id, groupName, secondGroupName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
      await umbracoUi.documentType.goToDocumentType(documentTypeName);

      // Act
      const groupValues = await umbracoUi.documentType.reorderTwoGroupsInADocumentType();
      const firstGroupValue = groupValues.firstGroupValue;
      const secondGroupValue = groupValues.secondGroupValue;
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      // Since we swapped sorting order, the firstGroupValue should have sortOrder 1 and the secondGroupValue should have sortOrder 0
      expect(await umbracoApi.documentType.doesDocumentTypeGroupNameContainCorrectSortOrder(documentTypeName, secondGroupValue, 0)).toBeTruthy();
      expect(await umbracoApi.documentType.doesDocumentTypeGroupNameContainCorrectSortOrder(documentTypeName, firstGroupValue, 1)).toBeTruthy();
    });

    // TODO: Unskip when it works. Sometimes the properties are not dragged correctly.
    test.skip('can reorder properties in a document type', async ({page, umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      const dataTypeNameTwo = "Second Color Picker";
      await umbracoApi.documentType.createDocumentTypeWithTwoPropertyEditors(documentTypeName, dataTypeName, dataTypeData.id, dataTypeNameTwo, dataTypeData.id);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickReorderButton();
      // Drag and Drop
      await page.waitForTimeout(5000);

      const dragFromLocator = page.getByText(dataTypeNameTwo);
      const dragToLocator = page.getByText(dataTypeName);
      await umbracoUi.documentType.dragAndDrop(dragFromLocator, dragToLocator, 0, 0, 5);
      await umbracoUi.documentType.clickIAmDoneReorderingButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.properties[0].name).toBe(dataTypeNameTwo);
      expect(documentTypeData.properties[1].name).toBe(dataTypeName);
    });

    // TODO: Unskip when the frontend does not give the secondTab -1 as the sortOrder
    test.skip('can reorder tabs in a document type', async ({page, umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      const secondTabName = 'SecondTab';
      await umbracoApi.documentType.createDocumentTypeWithTwoTabs(documentTypeName, dataTypeName, dataTypeData.id, tabName, secondTabName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
      await umbracoUi.documentType.goToDocumentType(documentTypeName);

      // Act
      const dragToLocator = page.getByRole('tab', {name: tabName});
      const dragFromLocator = page.getByRole('tab', {name: secondTabName});
      await umbracoUi.documentType.clickReorderButton();
      await umbracoUi.documentType.dragAndDrop(dragFromLocator, dragToLocator, 0, 0, 10);
      await umbracoUi.documentType.clickIAmDoneReorderingButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      expect(await umbracoApi.documentType.doesDocumentTypeTabNameContainCorrectSortOrder(documentTypeName, secondTabName, 0)).toBeTruthy();
      expect(await umbracoApi.documentType.doesDocumentTypeTabNameContainCorrectSortOrder(documentTypeName, tabName, 1)).toBeTruthy();
    });

    test('can add a description to a property in a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      const descriptionText = 'This is a property';
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickEditorSettingsButton();
      await umbracoUi.documentType.enterPropertyEditorDescription(descriptionText);
      await umbracoUi.documentType.clickUpdateButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      // We need to reload the page, because the description is not updated until you refresh the page.
      await umbracoUi.reloadPage();
      await expect(umbracoUi.documentType.enterDescriptionTxt).toBeVisible();
      expect(umbracoUi.documentType.doesDescriptionHaveValue(descriptionText)).toBeTruthy();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.properties[0].description).toBe(descriptionText);
    });

    test('can set is mandatory for a property in a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickEditorSettingsButton();
      await umbracoUi.documentType.clickMandatorySlider();
      await umbracoUi.documentType.clickUpdateButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.properties[0].validation.mandatory).toBeTruthy();
    });

    test('can enable validation for a property in a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      const regex = '^[a-zA-Z0-9]*$';
      const regexMessage = 'Only letters and numbers are allowed';
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickEditorSettingsButton();
      await umbracoUi.documentType.selectValidationOption('');
      await umbracoUi.documentType.enterRegEx(regex);
      await umbracoUi.documentType.enterRegExMessage(regexMessage);
      await umbracoUi.documentType.clickUpdateButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.properties[0].validation.regEx).toBe(regex);
      expect(documentTypeData.properties[0].validation.regExMessage).toBe(regexMessage);
    });

    test('can allow vary by culture for a property in a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, groupName, true);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickEditorSettingsButton();
      await umbracoUi.documentType.clickVaryByCultureSlider();
      await umbracoUi.documentType.clickUpdateButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.properties[0].variesByCulture).toBeTruthy();
    });

    test('can set appearance to label on top for a property in a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
      await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickEditorSettingsButton();
      await umbracoUi.documentType.clickLabelOnTopButton();
      await umbracoUi.documentType.clickUpdateButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.properties[0].appearance.labelOnTop).toBeTruthy();
    });
  });

  test.describe('Structure tab', () => {
    test('can add allow as root to a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickStructureTab();
      await umbracoUi.documentType.clickAllowAtRootButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.allowedAsRoot).toBeTruthy();
    });

    test('can add an allowed child node to a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickStructureTab();
      await umbracoUi.documentType.clickChooseButton();
      await umbracoUi.documentType.clickButtonWithName(documentTypeName);
      await umbracoUi.documentType.clickAllowedChildNodesButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.allowedDocumentTypes[0].documentType.id).toBe(documentTypeData.id);
    });

    test('can remove an allowed child node from a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const childDocumentTypeName = 'ChildDocumentType';
      await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
      const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
      await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickStructureTab();
      await umbracoUi.documentType.clickTrashIconButtonForName(childDocumentTypeName);
      await umbracoUi.documentType.clickConfirmRemoveButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.allowedDocumentTypes.length).toBe(0);

      // Clean
      await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
    });

    test('can configure a collection for a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      const collectionDataTypeName = 'TestCollection';
      await umbracoApi.dataType.ensureNameNotExists(collectionDataTypeName);
      const collectionDataTypeId = await umbracoApi.dataType.create(collectionDataTypeName, 'Umbraco.ListView', [], null, 'Umb.PropertyEditorUi.CollectionView');
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickStructureTab();
      await umbracoUi.documentType.clickConfigureAsACollectionButton();
      await umbracoUi.documentType.clickTextButtonWithName(collectionDataTypeName);
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.collection.id).toEqual(collectionDataTypeId);

      // Clean
      await umbracoApi.dataType.ensureNameNotExists(collectionDataTypeName);
    });
  });

  test.describe('Settings tab', () => {

    test('can add allow vary by culture for a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickDocumentTypeSettingsTab();
      await umbracoUi.documentType.clickVaryByCultureButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.variesByCulture).toBeTruthy();
    });

    test('can add allow segmentation for a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickDocumentTypeSettingsTab();
      await umbracoUi.documentType.clickVaryBySegmentsButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.variesBySegment).toBeTruthy();
    });

    test('can set is an element type for a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickDocumentTypeSettingsTab();
      await umbracoUi.documentType.clickTextButtonWithName('Element type');
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.isElement).toBeTruthy();

    });

    // TODO: Unskip. Currently The cleanup is not updated upon save
    test.skip('can disable history cleanup for a document type', async ({page, umbracoApi, umbracoUi}) => {
      // Arrange
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      // Is needed
      await umbracoUi.waitForTimeout(200);
      await page.locator('umb-body-layout').getByRole('tab', {name: 'Settings'}).click({force: true});
      await page.getByText('Auto cleanup').click();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.cleanup.preventCleanup).toBeTruthy();
    });
  });

  test.describe('Templates tab', () => {
    test('can add an allowed template to a document type', async ({umbracoApi, umbracoUi}) => {
      // Arrange
      await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
      const templateName = 'TestTemplate';
      await umbracoApi.template.ensureNameNotExists(templateName);
      const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await umbracoUi.documentType.clickDocumentTypeTemplatesTab();
      await umbracoUi.documentType.clickAddButton();
      await umbracoUi.documentType.clickLabelWithName(templateName);
      await umbracoUi.documentType.clickChooseButton();
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.allowedTemplates[0].id).toBe(templateId);
    });

    // When removing a template, the defaultTemplateId is set to "" which is not correct
    test.skip('can remove an allowed template from a document type', async ({page, umbracoApi, umbracoUi}) => {
      // Arrange
      const templateName = 'TestTemplate';
      await umbracoApi.template.ensureNameNotExists(templateName);
      const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
      await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(documentTypeName, templateId);
      await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

      // Act
      await umbracoUi.documentType.goToDocumentType(documentTypeName);
      await page.getByRole('tab', {name: 'Templates'}).click({force: true});
      await page.getByLabel('Remove ' + templateName).click({force: true});
      await umbracoUi.documentType.clickSaveButton();

      // Assert
      await umbracoUi.documentType.isSuccessNotificationVisible();
      const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
      expect(documentTypeData.allowedTemplates).toHaveLength(0);
    });
  });
});
