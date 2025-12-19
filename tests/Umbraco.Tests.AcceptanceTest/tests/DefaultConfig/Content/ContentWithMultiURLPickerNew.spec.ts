import {ConstantHelper, test, AliasHelper} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Multi URL Picker';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const link = 'https://docs.umbraco.com';
const linkTitle = 'Umbraco Documentation';
const anchorValue = '#section1';
const querystringValue = '?id=123';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.describe('Source Tab Selection Tests', () => {

  test('cannot add link when no source tab is selected', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();

    // Assert
    await expect(umbracoUi.content.addButton).toBeDisabled();
  });

  test('can add link when Content tab is selected with valid content', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    // Create a document to link
    const documentTypeForLinkedDocumentName = 'TestDocumentType';
    const documentTypeForLinkedDocumentId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeForLinkedDocumentName);
    const linkedDocumentName = 'LinkedDocument';
    const linkedDocumentId = await umbracoApi.document.createDefaultDocument(linkedDocumentName, documentTypeForLinkedDocumentId);
    await umbracoApi.document.publish(linkedDocumentId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateActionMenuOption();
    await umbracoUi.content.chooseDocumentType(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickDocumentLinkButton();
    await umbracoUi.content.selectLinkByName(linkedDocumentName);
    await umbracoUi.content.clickChooseModalButton();

    // Assert
    await expect(umbracoUi.content.addButton).toBeEnabled();

    // Clean
    await umbracoApi.documentType.ensureNameNotExists(documentTypeForLinkedDocumentName);
    await umbracoApi.document.ensureNameNotExists(linkedDocumentName);
  });

  test('can add link when Media tab is selected with valid media', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    // Create a media to pick
    const mediaFileName = 'TestMediaFileForContent';
    await umbracoApi.media.ensureNameNotExists(mediaFileName);
    await umbracoApi.media.createDefaultMediaWithImage(mediaFileName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickMediaLinkButton();
    await umbracoUi.content.selectMediaWithName(mediaFileName);
    await umbracoUi.content.clickChooseModalButton();

    // Assert
    await expect(umbracoUi.content.addButton).toBeEnabled();

    // Clean
    await umbracoApi.media.ensureNameNotExists(mediaFileName);
  });

  test('can see add button state update correctly when switching between tabs', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act & Assert
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();

    // Initially Add button should be disabled (no tab selected)
    await expect(umbracoUi.content.addButton).toBeDisabled();

    // Click Manual tab and enter URL - Add button should be enabled
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink(link);
    await expect(umbracoUi.content.addButton).toBeEnabled();

    // Click Content tab (without selecting content) - Add button should be disabled
    await umbracoUi.content.clickDocumentLinkButton();
    await expect(umbracoUi.content.addButton).toBeDisabled();
  });
});

test.describe('Manual Tab Validation Tests', () => {

  test('cannot create content with empty manual url and anchor', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink('');
    await umbracoUi.content.enterAnchorOrQuerystring('');
    await umbracoUi.content.clickAddButton();

    // Assert
    await umbracoUi.content.isTextWithMessageVisible(ConstantHelper.validationMessages.emptyLinkPicker);
  });

  test('can see validation error disappear when url is entered', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    // Leave both fields empty first to trigger validation
    await umbracoUi.content.enterLink('');
    await umbracoUi.content.enterAnchorOrQuerystring('');
    await umbracoUi.content.clickAddButton();
    // Verify error appears
    await umbracoUi.content.isTextWithMessageVisible(ConstantHelper.validationMessages.emptyLinkPicker);
    // Enter URL to clear validation
    await umbracoUi.content.enterLink(link);

    // Assert
    await umbracoUi.content.isTextWithMessageVisible(ConstantHelper.validationMessages.emptyLinkPicker, false);
  });

  test('can see validation error disappear when anchor is entered', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    // Leave both fields empty first to trigger validation
    await umbracoUi.content.enterLink('');
    await umbracoUi.content.enterAnchorOrQuerystring('');
    await umbracoUi.content.clickAddButton();
    // Verify error appears
    await umbracoUi.content.isTextWithMessageVisible(ConstantHelper.validationMessages.emptyLinkPicker);
    // Enter Anchor to clear validation
    await umbracoUi.content.enterAnchorOrQuerystring(anchorValue);

    // Assert
    await umbracoUi.content.isTextWithMessageVisible(ConstantHelper.validationMessages.emptyLinkPicker, false);
  });

  test('can create content with manual url only', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink(link);
    // Leave Anchor empty
    await umbracoUi.content.clickAddButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessStateVisibleForSaveButton();
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
    expect(contentData.values[0].value.length).toBe(1);
    expect(contentData.values[0].value[0].type).toEqual('external');
    expect(contentData.values[0].value[0].url).toEqual(link);
  });

  test('can create content with manual anchor only', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    // Leave URL empty
    await umbracoUi.content.enterAnchorOrQuerystring(anchorValue);
    await umbracoUi.content.clickAddButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessStateVisibleForSaveButton();
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
    expect(contentData.values[0].value.length).toBe(1);
    expect(contentData.values[0].value[0].queryString).toEqual(anchorValue);
  });

  test('can create content with manual url and anchor', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink(link);
    await umbracoUi.content.enterAnchorOrQuerystring(querystringValue);
    await umbracoUi.content.clickAddButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessStateVisibleForSaveButton();
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
    expect(contentData.values[0].value.length).toBe(1);
    expect(contentData.values[0].value[0].type).toEqual('external');
    expect(contentData.values[0].value[0].url).toEqual(link);
    expect(contentData.values[0].value[0].queryString).toEqual(querystringValue);
  });
});

test.describe('General Functionality Tests', () => {

  test('can close dialog without saving link', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink(link);
    await umbracoUi.content.enterLinkTitle(linkTitle);
    await umbracoUi.content.clickCloseButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessStateVisibleForSaveButton();
    const contentData = await umbracoApi.document.getByName(contentName);
    // Verify no link was added since dialog was closed without clicking Add
    expect(contentData.values).toEqual([]);
  });

  test('can clear url field using remove button', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink(link);
    // Click remove button to clear URL
    await umbracoUi.content.clickRemoveLinkButton();

    // Assert - Add button should be disabled after URL is cleared
    await expect(umbracoUi.content.addButton).toBeDisabled();
  });

  test('can create content with special characters in title field', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const customTitle = 'Custom Link Title with Special Characters !@#$%^&*()';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink(link);
    await umbracoUi.content.enterLinkTitle(customTitle);
    await umbracoUi.content.clickAddButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessStateVisibleForSaveButton();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value[0].name).toEqual(customTitle);
  });

  test('can create content with target toggle enabled to open link in new window', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink(link);
    await umbracoUi.content.enterLinkTitle(linkTitle);
    await umbracoUi.content.clickTargetToggle();
    await umbracoUi.content.clickAddButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessStateVisibleForSaveButton();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value[0].target).toEqual('_blank');
  });

  test('can create content with target toggle off by default', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink(link);
    // Do not toggle target
    await umbracoUi.content.clickAddButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessStateVisibleForSaveButton();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value[0].target).toBeNull();
  });
});

test.describe('Content and Media Tab Tests', () => {

  test('can create content with document link', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedState = 'Draft';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    // Create a document to link
    const documentTypeForLinkedDocumentName = 'TestDocumentType';
    const documentTypeForLinkedDocumentId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeForLinkedDocumentName);
    const linkedDocumentName = 'LinkedDocument';
    const linkedDocumentId = await umbracoApi.document.createDefaultDocument(linkedDocumentName, documentTypeForLinkedDocumentId);
    await umbracoApi.document.publish(linkedDocumentId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateActionMenuOption();
    await umbracoUi.content.chooseDocumentType(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickDocumentLinkButton();
    await umbracoUi.content.selectLinkByName(linkedDocumentName);
    await umbracoUi.content.clickChooseModalButton();
    await umbracoUi.content.clickAddButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.waitForContentToBeCreated();
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.variants[0].state).toBe(expectedState);
    expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
    expect(contentData.values[0].value.length).toBe(1);
    expect(contentData.values[0].value[0].type).toEqual('document');
    expect(contentData.values[0].value[0].unique).toEqual(linkedDocumentId);

    // Clean
    await umbracoApi.documentType.ensureNameNotExists(documentTypeForLinkedDocumentName);
    await umbracoApi.document.ensureNameNotExists(linkedDocumentName);
  });

  test('can create content with media link', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    // Create a media to pick
    const mediaFileName = 'TestMediaFileForContent';
    await umbracoApi.media.ensureNameNotExists(mediaFileName);
    const mediaFileId = await umbracoApi.media.createDefaultMediaWithImage(mediaFileName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickMediaLinkButton();
    await umbracoUi.content.selectMediaWithName(mediaFileName);
    await umbracoUi.content.clickChooseModalButton();
    await umbracoUi.content.clickAddButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessStateVisibleForSaveButton();
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
    expect(contentData.values[0].value.length).toBe(1);
    expect(contentData.values[0].value[0].type).toEqual('media');
    expect(contentData.values[0].value[0].unique).toEqual(mediaFileId);

    // Clean
    await umbracoApi.media.ensureNameNotExists(mediaFileName);
  });

  test('can create content with document link and anchor', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    // Create a document to link
    const documentTypeForLinkedDocumentName = 'TestDocumentType';
    const documentTypeForLinkedDocumentId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeForLinkedDocumentName);
    const linkedDocumentName = 'LinkedDocument';
    const linkedDocumentId = await umbracoApi.document.createDefaultDocument(linkedDocumentName, documentTypeForLinkedDocumentId);
    await umbracoApi.document.publish(linkedDocumentId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateActionMenuOption();
    await umbracoUi.content.chooseDocumentType(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickDocumentLinkButton();
    await umbracoUi.content.selectLinkByName(linkedDocumentName);
    await umbracoUi.content.clickChooseModalButton();
    await umbracoUi.content.enterAnchorOrQuerystring(anchorValue);
    await umbracoUi.content.clickAddButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.waitForContentToBeCreated();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value[0].type).toEqual('document');
    expect(contentData.values[0].value[0].unique).toEqual(linkedDocumentId);
    expect(contentData.values[0].value[0].queryString).toEqual(anchorValue);

    // Clean
    await umbracoApi.documentType.ensureNameNotExists(documentTypeForLinkedDocumentName);
    await umbracoApi.document.ensureNameNotExists(linkedDocumentName);
  });

  test('can create content with document link and title', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const customTitle = 'Custom Document Link Title';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    // Create a document to link
    const documentTypeForLinkedDocumentName = 'TestDocumentType';
    const documentTypeForLinkedDocumentId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeForLinkedDocumentName);
    const linkedDocumentName = 'LinkedDocument';
    const linkedDocumentId = await umbracoApi.document.createDefaultDocument(linkedDocumentName, documentTypeForLinkedDocumentId);
    await umbracoApi.document.publish(linkedDocumentId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateActionMenuOption();
    await umbracoUi.content.chooseDocumentType(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickDocumentLinkButton();
    await umbracoUi.content.selectLinkByName(linkedDocumentName);
    await umbracoUi.content.clickChooseModalButton();
    await umbracoUi.content.enterLinkTitle(customTitle);
    await umbracoUi.content.clickAddButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.waitForContentToBeCreated();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value[0].type).toEqual('document');
    expect(contentData.values[0].value[0].unique).toEqual(linkedDocumentId);
    expect(contentData.values[0].value[0].name).toEqual(customTitle);

    // Clean
    await umbracoApi.documentType.ensureNameNotExists(documentTypeForLinkedDocumentName);
    await umbracoApi.document.ensureNameNotExists(linkedDocumentName);
  });

  test('can create content with media link and target enabled', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    // Create a media to pick
    const mediaFileName = 'TestMediaFileForContent';
    await umbracoApi.media.ensureNameNotExists(mediaFileName);
    await umbracoApi.media.createDefaultMediaWithImage(mediaFileName);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickMediaLinkButton();
    await umbracoUi.content.selectMediaWithName(mediaFileName);
    await umbracoUi.content.clickChooseModalButton();
    await umbracoUi.content.clickTargetToggle();
    await umbracoUi.content.clickAddButton();
    await umbracoUi.content.clickSaveButton();

    // Assert
    await umbracoUi.content.isSuccessStateVisibleForSaveButton();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value[0].type).toEqual('media');
    expect(contentData.values[0].value[0].target).toEqual('_blank');

    // Clean
    await umbracoApi.media.ensureNameNotExists(mediaFileName);
  });
});
