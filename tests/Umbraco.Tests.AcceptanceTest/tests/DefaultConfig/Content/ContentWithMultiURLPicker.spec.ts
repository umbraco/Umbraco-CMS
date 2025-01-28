import { ConstantHelper, test, AliasHelper } from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const dataTypeName = 'Multi URL Picker';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const link = 'https://docs.umbraco.com';
const linkTitle = 'Umbraco Documentation';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content with the document link', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickDocumentLinkButton();
  await umbracoUi.content.selectLinkByName(linkedDocumentName);
  await umbracoUi.content.clickButtonWithName('Choose');
  await umbracoUi.content.clickAddButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
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
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
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
  await umbracoUi.content.clickAddButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
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

test('can create content with the external link', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.clickAddButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
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
  await umbracoUi.content.isSuccessNotificationVisible();
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

test('can add multiple links in the content', async ({umbracoApi, umbracoUi}) => {
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
  // Add media link
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickMediaLinkButton();
  await umbracoUi.content.selectMediaWithName(mediaFileName);
  await umbracoUi.content.clickChooseModalButton();
  await umbracoUi.content.clickAddButton();
  // Add external link
  await umbracoUi.content.clickAddMultiURLPickerButton();
  await umbracoUi.content.clickManualLinkButton();
  await umbracoUi.content.enterLink(link);
  await umbracoUi.content.enterLinkTitle(linkTitle);
  await umbracoUi.content.clickAddButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
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
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDocumentWithExternalLinkURLPicker(contentName, documentTypeId, link, linkTitle);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.removeUrlPickerByName(linkTitle);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values).toEqual([]);
});

test('can edit the URL picker in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedLinkTitle = 'Updated Umbraco Documentation';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  await umbracoApi.document.createDocumentWithExternalLinkURLPicker(contentName, documentTypeId, link, linkTitle);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickLinkWithName(linkTitle);
  await umbracoUi.content.enterLinkTitle(updatedLinkTitle);
  await umbracoUi.content.clickUpdateButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(dataTypeName));
  expect(contentData.values[0].value.length).toBe(1);
  expect(contentData.values[0].value[0].type).toEqual('external');
  expect(contentData.values[0].value[0].icon).toEqual('icon-link');
  expect(contentData.values[0].value[0].name).toEqual(updatedLinkTitle);
  expect(contentData.values[0].value[0].url).toEqual(link);
});
