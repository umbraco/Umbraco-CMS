import { ConstantHelper, test, AliasHelper } from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Multi URL Picker';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const link = 'https://docs.umbraco.com';
const linkTitle = 'Umbraco Documentation';
const manualPropertyName = 'Manual';
const anchorOrQuerystringPropertyName = 'Anchor or querystring';
const anchorValue = '#anchor';
const querystringValue = '?param=value';
let dataTypeData: any;
let documentTypeId: any;

test.beforeEach(async ({umbracoApi}) => {
  dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content with the document link', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
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
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].type).toEqual('document');
  expect(contentData.values[0].value[0].icon).toEqual('icon-document');
  expect(contentData.values[0].value[0].target).toBeNull();
  expect(contentData.values[0].value[0].unique).toEqual(linkedDocumentId);
  expect(contentData.values[0].value[0].name).toEqual(linkedDocumentName);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeForLinkedDocumentName);
  await umbracoApi.document.ensureNameNotExists(linkedDocumentName);
});

test('can publish content with the document link', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  // Create a document to link
  const documentTypeForLinkedDocumentName = 'TestDocumentType';
  const documentTypeForLinkedDocumentId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeForLinkedDocumentName);
  const linkedDocumentName = 'ContentToPick';
  const linkedDocumentId = await umbracoApi.document.createDefaultDocument(linkedDocumentName, documentTypeForLinkedDocumentId);
  await umbracoApi.document.publish(linkedDocumentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickDocumentLinkButton();
  await umbracoUi.content.selectLinkByName(linkedDocumentName);
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short); // Wait for the document link to be selected
  await umbracoUi.content.clickButtonWithName('Choose');
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].type).toEqual('document');
  expect(contentData.values[0].value[0].icon).toEqual('icon-document');
  expect(contentData.values[0].value[0].target).toBeNull();
  expect(contentData.values[0].value[0].unique).toEqual(linkedDocumentId);
  expect(contentData.values[0].value[0].name).toEqual(linkedDocumentName);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeForLinkedDocumentName);
  await umbracoApi.document.ensureNameNotExists(linkedDocumentName);
});

test('can create content with the manual link with title', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(link);
  await umbracoUi.content.enterLinkTitle(linkTitle);
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].type).toEqual('external');
  expect(contentData.values[0].value[0].icon).toEqual('icon-link');
  expect(contentData.values[0].value[0].name).toEqual(linkTitle);
  expect(contentData.values[0].value[0].url).toEqual(link);
});

test('can create content with the media link', async ({umbracoApi, umbracoUi}) => {
  // Arrange
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
  await umbracoUi.content.selectMediaWithName(mediaFileName, true);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].type).toEqual('media');
  expect(contentData.values[0].value[0].icon).toEqual('icon-picture');
  expect(contentData.values[0].value[0].unique).toEqual(mediaFileId);
  expect(contentData.values[0].value[0].name).toEqual(mediaFileName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
});

test('can add multiple links in the content', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  // Create a media to pick
  const mediaFileName = 'TestMediaFileForContent';
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
  const mediaFileId = await umbracoApi.media.createDefaultMediaWithImage(mediaFileName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  // Add media link
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickMediaLinkButton();
  await umbracoUi.content.selectMediaWithName(mediaFileName, true);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.waitForTimeout(500); // Wait for the media link to be added
  // Add external link
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(link);
  await umbracoUi.content.enterLinkTitle(linkTitle);
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(2);
  // Verify the information of the first URL picker
  expect(contentData.values[0].value[0].type).toEqual('media');
  expect(contentData.values[0].value[0].icon).toEqual('icon-picture');
  expect(contentData.values[0].value[0].unique).toEqual(mediaFileId);
  expect(contentData.values[0].value[0].name).toEqual(mediaFileName);
  // Verify the information of the second URL picker
  expect(contentData.values[0].value[1].type).toEqual('external');
  expect(contentData.values[0].value[1].icon).toEqual('icon-link');
  expect(contentData.values[0].value[1].name).toEqual(linkTitle);
  expect(contentData.values[0].value[1].url).toEqual(link);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
});

test('can remove the URL picker in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDocumentWithExternalLinkURLPicker(contentName, documentTypeId, link, linkTitle);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.removeUrlPickerByName(linkTitle);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values).toEqual([]);
});

test('can edit the URL picker in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedLinkTitle = 'Updated Umbraco Documentation';
  await umbracoApi.document.createDocumentWithExternalLinkURLPicker(contentName, documentTypeId, link, linkTitle);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickLinkWithName(linkTitle);
  await umbracoUi.content.enterLinkTitle(updatedLinkTitle);
  await umbracoUi.content.clickUpdateButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].type).toEqual('external');
  expect(contentData.values[0].value[0].icon).toEqual('icon-link');
  expect(contentData.values[0].value[0].name).toEqual(updatedLinkTitle);
  expect(contentData.values[0].value[0].url).toEqual(link);
});

test('can create content with the link to an unpublished document', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  // Create a document to link
  const documentTypeForLinkedDocumentName = 'TestDocumentType';
  const documentTypeForLinkedDocumentId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeForLinkedDocumentName);
  const linkedDocumentName = 'LinkedDocument';
  const linkedDocumentId = await umbracoApi.document.createDefaultDocument(linkedDocumentName, documentTypeForLinkedDocumentId);
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
  await umbracoUi.content.clickButtonWithName('Choose');
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeCreated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].type).toEqual('document');
  expect(contentData.values[0].value[0].icon).toEqual('icon-document');
  expect(contentData.values[0].value[0].target).toBeNull();
  expect(contentData.values[0].value[0].unique).toEqual(linkedDocumentId);
  expect(contentData.values[0].value[0].name).toEqual(linkedDocumentName);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeForLinkedDocumentName);
  await umbracoApi.document.ensureNameNotExists(linkedDocumentName);
});

test('cannot add link when no source tab is selected', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();

  // Assert
  await umbracoUi.content.isLinkPickerAddButtonDisabled();
});

test('can close dialog without saving link', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(link);
  await umbracoUi.content.enterLinkTitle(linkTitle);
  await umbracoUi.content.clickLinkPickerCloseButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  // Verify no link was added since dialog was closed without clicking Add
  expect(contentData.values).toEqual([]);
});

test('can reset manual URL using remove button', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(link);
  // Click remove button to reset URL
  await umbracoUi.content.clickRemoveExactButton();
  await umbracoUi.content.clickConfirmToResetButton();

  // Assert
  await umbracoUi.content.isLinkPickerAddButtonDisabled();
});

test('can create content with special characters in title field', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customTitle = 'Custom Link Title with Special Characters !@#$%^&*()';
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(link);
  await umbracoUi.content.enterLinkTitle(customTitle);
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0].name).toEqual(customTitle);
});

test('can create content with target toggle enabled to open link in new window', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(link);
  await umbracoUi.content.enterLinkTitle(linkTitle);
  await umbracoUi.content.clickLinkPickerTargetToggle();
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value[0].target).toEqual('_blank');
});

test.describe('manual tab validation tests', () => {
  test('cannot create content with empty manual url and anchor', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink('');
    await umbracoUi.content.enterAnchorOrQuerystring('');
    await umbracoUi.content.clickLinkPickerAddButton();

    // Assert
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
  });

  test('can see validation message disappear when url is entered', async ({umbracoApi, umbracoUi}) => {
    // Arrange
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
    await umbracoUi.content.clickLinkPickerAddButton();
    // Verify validation messages appear
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
    // Enter URL to clear validation
    await umbracoUi.content.enterLink(link);

    // Assert
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker, false);
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker, false);
  });

  test('can see validation message disappear when anchor is entered', async ({umbracoApi, umbracoUi}) => {
    // Arrange
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
    await umbracoUi.content.clickLinkPickerAddButton();
    // Verify validation messages appear
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
    // Enter Anchor to clear validation
    await umbracoUi.content.enterAnchorOrQuerystring(anchorValue);

    // Assert
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker, false);
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker, false);
  });

  test('can create content with manual url only', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink(link);
    // Leave Anchor empty
    await umbracoUi.content.clickLinkPickerAddButton();
    await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

    // Assert
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
    expect(contentData.values[0].value.length).toBe(1);
    expect(contentData.values[0].value[0].type).toEqual('external');
    expect(contentData.values[0].value[0].url).toEqual(link);
  });

  test('can create content with manual anchor only', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    // Leave URL empty
    await umbracoUi.content.enterAnchorOrQuerystring(anchorValue);
    await umbracoUi.content.clickLinkPickerAddButton();
    await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

    // Assert
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
    expect(contentData.values[0].value.length).toBe(1);
    expect(contentData.values[0].value[0].queryString).toEqual(anchorValue);
  });

  test('can create content with manual url and anchor', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickAddMultiURLPickerButton();
    await umbracoUi.content.clickManualLinkButton();
    await umbracoUi.content.enterLink(link);
    await umbracoUi.content.enterAnchorOrQuerystring(querystringValue);
    await umbracoUi.content.clickLinkPickerAddButton();
    await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

    // Assert
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
    expect(contentData.values[0].value.length).toBe(1);
    expect(contentData.values[0].value[0].type).toEqual('external');
    expect(contentData.values[0].value[0].url).toEqual(link);
    expect(contentData.values[0].value[0].queryString).toEqual(querystringValue);
  });

  test('cannot update content with empty manual url and anchor', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.document.createDocumentWithExternalLinkURLPicker(contentName, documentTypeId, link, linkTitle);
    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickLinkWithName(linkTitle);
    await umbracoUi.content.enterLink('');
    await umbracoUi.content.enterAnchorOrQuerystring('');
    await umbracoUi.content.clickUpdateButton();

    // Assert
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(manualPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
    await umbracoUi.content.doesPropertyWithNameContainValidationMessage(anchorOrQuerystringPropertyName, ConstantHelper.validationMessages.emptyManualLinkPicker);
  });
});
