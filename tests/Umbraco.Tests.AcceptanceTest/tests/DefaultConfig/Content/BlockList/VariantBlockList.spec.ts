import {ConstantHelper, NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

// Document Type
const documentTypeName = 'DocumentTypeName';
let documentTypeId = null;
const documentTypeGroupName = 'DocumentGroup';

// Block List
const blockListName = 'BlockListName';
let blockListId = null;

// Element Type
const blockName = 'BlockName';
let elementTypeId = null;
const elementGroupName = 'ElementGroup';

// Text String
const textStringName = 'TextStringName';
let textStringDataTypeId = null;
const textStringDataTypeName = 'Textstring';
const textStringText = 'ThisIsATextString';

// Content Name
const contentName = 'ContentName';
let contentId = null;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringDataTypeId = textStringDataType.id;
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  await umbracoApi.documentType.ensureNameNotExists(blockName);
  await umbracoApi.dataType.ensureNameNotExists(blockListName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('invariant document type with invariant block list with invariant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockName, elementGroupName, textStringName, textStringDataTypeId);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeTruthy();
  await umbracoUi.reloadPage();
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});

test('can not create unsupported invariant document type with invariant block list with variant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, false);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.unsupportInvariantContentItemWithVariantBlocks);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isFailedStateButtonVisible();
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeFalsy();
});

test('can not create unsupported invariant document type with invariant block list with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, true);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.unsupportInvariantContentItemWithVariantBlocks);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isFailedStateButtonVisible();
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeFalsy();
});

test('variant document type with variant block list with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, true);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName, true);
  contentId = await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateBlockModalButtonAndWaitForModalToClose();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeTruthy();
  await umbracoUi.reloadPage();
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});

test('variant document type with invariant block list with variant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, false);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName, true, false);
  contentId = await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateBlockModalButtonAndWaitForModalToClose();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeTruthy();
  await umbracoUi.reloadPage();
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});

test('variant document type with invariant block list with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, true);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName, true, false);
  contentId = await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton()
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateBlockModalButtonAndWaitForModalToClose();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  expect(await umbracoApi.document.isDocumentPublished(contentId)).toBeTruthy();
  await umbracoUi.reloadPage();
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});
