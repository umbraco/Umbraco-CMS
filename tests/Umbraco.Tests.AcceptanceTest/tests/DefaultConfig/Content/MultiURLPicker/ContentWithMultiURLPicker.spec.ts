import { ConstantHelper, test, AliasHelper } from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Multi URL Picker';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const link = 'https://docs.umbraco.com';
const linkTitle = 'Umbraco Documentation';
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
  await umbracoApi.document.doesVariantHaveState(contentData, expectedState);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName)).length).toBe(1);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].type).toEqual('document');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].icon).toEqual('icon-document');
  expect(umbracoApi.document.getOnlyPropertyValue(contentData)[0].target).toBeNull();
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].unique).toEqual(linkedDocumentId);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].name).toEqual(linkedDocumentName);

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
  await umbracoUi.content.clickButtonWithName('Choose');
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  await umbracoApi.document.doesVariantHaveState(contentData, expectedState);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName)).length).toBe(1);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].type).toEqual('document');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].icon).toEqual('icon-document');
  expect(umbracoApi.document.getOnlyPropertyValue(contentData)[0].target).toBeNull();
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].unique).toEqual(linkedDocumentId);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].name).toEqual(linkedDocumentName);

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
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName)).length).toBe(1);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].type).toEqual('external');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].icon).toEqual('icon-link');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].name).toEqual(linkTitle);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].url).toEqual(link);
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
  await umbracoUi.content.selectMediaWithName(mediaFileName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName)).length).toBe(1);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].type).toEqual('media');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].icon).toEqual('icon-picture');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].unique).toEqual(mediaFileId);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].name).toEqual(mediaFileName);

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
  await umbracoUi.content.selectMediaWithName(mediaFileName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickLinkPickerAddButton();
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
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName)).length).toBe(2);
  // Verify the information of the first URL picker
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].type).toEqual('media');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].icon).toEqual('icon-picture');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].unique).toEqual(mediaFileId);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].name).toEqual(mediaFileName);
  // Verify the information of the second URL picker
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[1].type).toEqual('external');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[1].icon).toEqual('icon-link');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[1].name).toEqual(linkTitle);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[1].url).toEqual(link);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
});

test('can remove the URL picker in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDocumentWithExternalLinkURLPicker(contentName, documentTypeId, dataTypeName, link, linkTitle);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.removeUrlPickerByName(linkTitle);
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  await umbracoApi.document.doesHaveValueCount(contentData, 0);
});

test('can edit the URL picker in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedLinkTitle = 'Updated Umbraco Documentation';
  await umbracoApi.document.createDocumentWithExternalLinkURLPicker(contentName, documentTypeId, dataTypeName, link, linkTitle);
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
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName)).length).toBe(1);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].type).toEqual('external');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].icon).toEqual('icon-link');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].name).toEqual(updatedLinkTitle);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].url).toEqual(link);
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
  await umbracoApi.document.doesVariantHaveState(contentData, expectedState);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName)).length).toBe(1);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].type).toEqual('document');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].icon).toEqual('icon-document');
  expect(umbracoApi.document.getOnlyPropertyValue(contentData)[0].target).toBeNull();
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].unique).toEqual(linkedDocumentId);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].name).toEqual(linkedDocumentName);

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
  await umbracoApi.document.doesHaveValueCount(contentData, 0);
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
  await umbracoUi.content.clickManualLinkRemoveButton();
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
  expect(umbracoApi.document.getOnlyPropertyValue(contentData)[0].name).toEqual(customTitle);
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
  expect(umbracoApi.document.getOnlyPropertyValue(contentData)[0].target).toEqual('_blank');
});

test('cannot select a media folder as a media link', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const mediaFolderName = 'TestMediaFolder';
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
  await umbracoApi.media.createDefaultMediaFolder(mediaFolderName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickMediaLinkButton();

  // Assert
  await umbracoUi.content.isSelectCheckboxVisibleForMediaName(mediaFolderName, false);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
});

test('can select a media file inside a folder as a media link', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const mediaFolderName = 'TestMediaFolder';
  const mediaFileName = 'TestMediaFileInFolder';
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
  const mediaFolderId = await umbracoApi.media.createDefaultMediaFolder(mediaFolderName);
  const mediaFileId = await umbracoApi.media.createDefaultMediaWithImageAndParentId(mediaFileName, mediaFolderId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickMediaLinkButton();
  await umbracoUi.content.clickMediaWithName(mediaFolderName);
  await umbracoUi.content.selectMediaWithName(mediaFileName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickLinkPickerAddButton();
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName)).length).toBe(1);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].type).toEqual('media');
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].unique).toEqual(mediaFileId);
  expect(umbracoApi.document.getPropertyValue(contentData, AliasHelper.toAlias(dataTypeName))[0].name).toEqual(mediaFileName);

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaFolderName);
  await umbracoApi.media.ensureNameNotExists(mediaFileName);
});
