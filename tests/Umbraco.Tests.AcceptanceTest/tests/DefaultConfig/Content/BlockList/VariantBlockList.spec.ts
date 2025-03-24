import {ConstantHelper, NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";

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

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringDataTypeId = textStringDataType.id;
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(blockName);
  await umbracoApi.dataType.ensureNameNotExists(blockListName);
});

test('invariant document type with invariant block list with invariant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockName, elementGroupName, textStringName, textStringDataTypeId);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  await umbracoUi.reloadPage();
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});

test('invariant document type with invariant block list with variant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, false);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  await umbracoUi.reloadPage();
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});

// Remove fixme when this test works. Currently the textstring value is is not saved when saving / publishing the document
test.fixme('invariant document type with invariant block list with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, true);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName)
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  await umbracoUi.reloadPage();
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});

test('variant document type with variant block list with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, true);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName, true);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  await umbracoUi.reloadPage();
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});

test('variant document type with invariant block list with variant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, false);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName, true, false);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  await umbracoUi.reloadPage();
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});

test('variant document type with invariant block list with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, true);
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName, true, false);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton()
  await umbracoUi.content.clickBlockElementWithName(blockName);
  await umbracoUi.content.enterTextstring(textStringText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);

  await umbracoUi.reloadPage();
  await umbracoUi.content.goToBlockListBlockWithName(documentTypeGroupName, blockListName, blockName);
  await umbracoUi.content.doesPropertyContainValue(textStringName, textStringText);
});
