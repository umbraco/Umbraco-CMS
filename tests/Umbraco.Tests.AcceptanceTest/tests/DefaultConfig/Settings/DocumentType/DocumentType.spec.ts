import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
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

  // DESIGN
  test('can create a empty document type', async ({umbracoApi, umbracoUi}) => {
    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.clickActionsMenuForName('Document Types');
    await umbracoUi.documentType.clickCreateThreeDotsButton();
    await umbracoUi.documentType.clickNewDocumentTypeButton();
    await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  });

  test('can rename a empty document type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongName = 'NotADocumentTypeName';
    await umbracoApi.documentType.ensureNameNotExists(wrongName);
    await umbracoApi.documentType.createDefaultDocumentType(wrongName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(wrongName);
    await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  });

  test('can add a property to a empty document type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickAddGroupButton();
    await umbracoUi.documentType.addPropertyEditor(dataTypeName);
    await umbracoUi.documentType.enterGroupName(groupName);
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
    const newDataTypeName = 'Media Picker';
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
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

  test('can update group name to a document type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const newGroupName = 'UpdatedGroupName';
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.enterGroupName(newGroupName);
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    expect(documentTypeData.containers[0].name).toBe(newGroupName);
  });

  // There a currently frontend issues, when you try to add a property editor you have to click the button twice to focus.
  test.skip('can create a document type with a property in a tab', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickAddTabButton();
    await umbracoUi.documentType.enterTabName(tabName);
    await umbracoUi.documentType.addPropertyEditor(dataTypeName);
  });

  // There a currently frontend issues, when you try to add a property editor you have to click the button twice to focus.
  test.skip('can create a document type with properties in a group and a tab', async ({page, umbracoApi, umbracoUi}) => {
  });

  // There a currently frontend issues, when you try to add a property editor to the second group. the editor ends up in the first group
  test.skip('can create a document type with multiple groups', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const secondDataTypeName = 'Media Picker';
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickAddGroupButton();
    await umbracoUi.documentType.addPropertyEditor(secondDataTypeName, 1);
    await umbracoUi.documentType.enterGroupName('TesterGroup', 1);
    await umbracoUi.documentType.clickSaveButton();
  });

  // There a currently frontend issues, when you try to add a property editor you have to click the button twice to focus.
  test.skip('can create a document type with multiple tabs', async ({page, umbracoApi, umbracoUi}) => {

  });

  test('can create a document type with a composition', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const compositionDocumentTypeName = 'CompositionDocumentType';
    await umbracoApi.documentType.ensureNameNotExists(compositionDocumentTypeName);
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const compositionDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(compositionDocumentTypeName, dataTypeName, dataTypeData.id, groupName);
    await umbracoApi.documentType.createDefaultDocumentType(documentTypeName)

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickCompositionsButton();
    await umbracoUi.documentType.clickButtonWithName(compositionDocumentTypeName)
    // This is needed
    await umbracoUi.waitForTimeout(200);
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

  // Not possible
  test.skip('can reorder a group in a document type', async ({page, umbracoApi, umbracoUi}) => {
  });

  test('can reorder a property in a document type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const dataTypeNameTwo = "Second Color Picker";
    await umbracoApi.documentType.createDocumentTypeWithTwoPropertyEditors(documentTypeName, dataTypeName, dataTypeData.id, dataTypeNameTwo, dataTypeData.id);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickReorderButton();
    // Drag and Drop
    const dragFromLocator = page.getByText(dataTypeNameTwo);
    const dragToLocator = page.getByText(dataTypeName);
    await umbracoUi.documentType.dragAndDrop(dragFromLocator, dragToLocator, 0, 0, 5);
    await umbracoUi.documentType.clickIAmDoneReorderingButton();
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    expect(documentTypeData.properties[0].name).toBe(dataTypeNameTwo);
  });

  test('can add a description to a property in a document type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const descriptionText = 'This is a property';
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickEditorSettingsButton();
    await umbracoUi.documentType.enterDescription(descriptionText);
    await umbracoUi.documentType.clickUpdateButton();
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    expect(umbracoUi.documentType.doesDescriptionHaveValue(descriptionText)).toBeTruthy();
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    expect(documentTypeData.properties[0].description).toBe(descriptionText);
  });

  // Mandatory is not set when saved
  test.skip('can set is mandatory for a property in a document type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickEditorSettingsButton();
    await umbracoUi.documentType.clickMandatorySlider();
    await umbracoUi.documentType.clickUpdateButton();
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    expect(documentTypeData.properties[0].mandatory).toBeTruthy();
  });

  test('can enable validation for a property in a document type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const regex = '^[a-zA-Z0-9]*$';
    const regexMessage = 'Only letters and numbers are allowed';
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickEditorSettingsButton();
    await umbracoUi.documentType.selectValidationOption('')
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

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
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

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
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

  // Structure
  test('can add allow as root to a document type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickStructureTab();
    await umbracoUi.documentType.clickAllowAsRootButton();
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    expect(documentTypeData.allowedAsRoot).toBeTruthy();
  });

  // The choose button is not present.
  test.skip('can add an allowed child node to a document type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickStructureTab();
    await umbracoUi.documentType.clickChooseButton()
    await umbracoUi.documentType.clickButtonWithName(documentTypeName);
    await umbracoUi.documentType.clickFilterChooseButton();
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    expect(documentTypeData.allowedDocumentTypes[0].documentType.id).toBe(documentTypeData.id);
  });

  // The label when trying to delete an allowed child node is not correct
  test.skip('can remove an allowed child node from a document type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const childDocumentTypeName = 'ChildDocumentType';
    await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
    const childDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeName);
    await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    // Is needed
    await umbracoUi.waitForTimeout(200);
    await page.getByRole('tab', {name: 'Structure'}).click({force: true});
    // Assert

    // Clean
    await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  });

  // When saving the collection is not saved
  test.skip('can display children for a document type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await page.getByRole('tab', {name: 'Structure'}).click({force: true});
    await page.getByText('Display children in a Collection view').click();
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    console.log(documentTypeData);
  });


  // Settings

  test('can add allow vary by culture for a document type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
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

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
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

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickDocumentTypeSettingsTab();
    await umbracoUi.documentType.clickTextButtonWithName('Element type');
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.isSuccessNotificationVisible();
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    expect(documentTypeData.isElement).toBeTruthy();

  });

  // The cleanup is not updated upon save
  test.skip('can disable history cleanup for a document type', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
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

  // Templates
  test('can add an allowed template to a document type', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
    const templateName = 'TestTemplate';
    await umbracoApi.template.ensureNameNotExists(templateName);
    const templateId = await umbracoApi.template.createDefaultTemplate(templateName)

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
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

    // Act
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);
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
