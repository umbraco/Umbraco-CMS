import {ConstantHelper, NotificationConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {beforeEach} from "node:test";

// Document Types
const documentTypeName = 'DocumentTypeName';
let documentTypeId = null;
const documentTypeGroupName = 'DocumentGroup';

// Block Lists
const blockListName = 'BlockListName';
let blockListId = null;

// Element Types
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

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.language.ensureIsoCodeNotExists('da');
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName)
  textStringDataTypeId = textStringDataType.id
  await umbracoApi.language.createDanishLanguage();

});

test.afterEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.language.ensureIsoCodeNotExists('da');
});

test('invariant document with invariant block list with invariant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(blockName, elementGroupName, textStringDataTypeName, textStringDataTypeId)
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId)
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName)
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId)
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton()
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

test('invariant document with invariant block list with variant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, false)
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId)
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName)
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId)
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton()
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

test('invariant document with invariant block list with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, true, true)
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId)
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName)
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId)
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton()
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

test('invariant document with invariant block list with invariant block with an variant textString', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(blockName, elementGroupName, textStringName, textStringDataTypeId, false, true)
  blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId)
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, documentTypeGroupName)
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId)
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockElementButton()
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

test('variant document with variant block list with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {

});

test('variant document with invariant block list with variant block with an invariant textString', async ({umbracoApi, umbracoUi}) => {

});

test('variant document with invariant block list with variant block with an variant textString', async ({umbracoApi, umbracoUi}) => {

});

