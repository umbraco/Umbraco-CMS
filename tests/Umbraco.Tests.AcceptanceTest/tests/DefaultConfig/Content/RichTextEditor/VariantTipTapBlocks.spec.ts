import {ConstantHelper, NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";

// Document Type
const documentTypeName = 'DocumentTypeName';
let documentTypeId = null;
const documentTypeGroupName = 'DocumentGroup';

// Tip Tap
const tipTapName = 'TipTapTest';
let tipTapId = null;

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

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringDataTypeId = textStringDataType.id;
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  await umbracoApi.documentType.ensureNameNotExists(blockName);
  await umbracoApi.dataType.ensureNameNotExists(tipTapName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('invariant document type with invariant tiptap RTE with invariant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockName, elementGroupName, textStringName, textStringDataTypeId);
  tipTapId = await umbracoApi.dataType.createTipTapDataTypeWithABlock(tipTapName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapName, tipTapId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.isErrorNotificationVisible(false);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  await umbracoUi.reloadPage();

  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});

test('can not create unsupported invariant document type with invariant tiptap RTE with variant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, false);
  tipTapId = await umbracoApi.dataType.createTipTapDataTypeWithABlock(tipTapName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapName, tipTapId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.unsupportInvariantContentItemWithVariantBlocks);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible();
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
});

test('can not create unsupported invariant document type with invariant tiptap RTE with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, true);
  tipTapId = await umbracoApi.dataType.createTipTapDataTypeWithABlock(tipTapName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapName, tipTapId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.unsupportInvariantContentItemWithVariantBlocks);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible();
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
});

test('variant document type with variant tiptap RTE with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, true);
  tipTapId = await umbracoApi.dataType.createTipTapDataTypeWithABlock(tipTapName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapName, tipTapId, documentTypeGroupName, true);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  await umbracoUi.reloadPage();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});

test('variant document type with invariant tiptap RTE with variant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, false);
  tipTapId = await umbracoApi.dataType.createTipTapDataTypeWithABlock(tipTapName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapName, tipTapId, documentTypeGroupName, true, false);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  await umbracoUi.reloadPage();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});

test('variant document type with invariant tiptap RTE with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, true);
  tipTapId = await umbracoApi.dataType.createTipTapDataTypeWithABlock(tipTapName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, tipTapName, tipTapId, documentTypeGroupName, true, false);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInsertBlockButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  await umbracoUi.reloadPage();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});
