import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom Block List';
const elementTypeName = 'BlockListElement';
const propertyInBlock = 'Textstring';
const groupName = 'testGroup';
let elementTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  const textStringData = await umbracoApi.dataType.getByName(propertyInBlock);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, propertyInBlock, textStringData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

test('can create content with an empty block list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(customDataTypeName, elementTypeId);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values).toEqual([]);
});

test('can publish content with an empty block list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(customDataTypeName, elementTypeId);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationsHaveCount(2);  
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values).toEqual([]);
});

test('can add a block element in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'This is block test';
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(customDataTypeName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(inputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toEqual(inputText);
  const blockListValue = contentData.values.find(item => item.editorAlias === "Umbraco.BlockList")?.value;
  expect(blockListValue).toBeTruthy();
});

test('can edit block element in the content', async ({umbracoApi, umbracoUi}) => {
  const updatedText = 'This updated block test';
  await umbracoApi.document.createDefaultDocumentWithABlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickEditBlockListBlockButton();
  await umbracoUi.content.enterTextstring(updatedText);
  await umbracoUi.content.clickUpdateButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toEqual(updatedText);
});

test('can delete block element in the content', async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.createDefaultDocumentWithABlockListEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickDeleteBlockListBlockButton();
  await umbracoUi.content.clickConfirmToDeleteButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  const blockGridValue = contentData.values.find(item => item.value);
  expect(blockGridValue).toBeFalsy();
});

test('cannot add number of block element greater than the maximum amount', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'This is block test';
  const customDataTypeId = await umbracoApi.dataType.createBlockListWithABlockAndMinAndMaxAmount(customDataTypeName, elementTypeId, 0, 1);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(inputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(inputText);
  await umbracoUi.content.clickCreateModalButton();

  // Assert
  await umbracoUi.content.doesFormValidationMessageContainText('Maximum');
  await umbracoUi.content.doesFormValidationMessageContainText('too many');
});

test('can set the label of block element in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockLabel = 'Test Block Label';
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithLabel(customDataTypeName, elementTypeId, blockLabel);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.isErrorNotificationVisible(false);
  await umbracoUi.content.doesBlockElementHaveName(blockLabel);
});

// TODO: Remove skip when front-end is ready. Currently, it is impossible to create content with blocklist that has a setting model
test.skip('can add settings model for the block in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const contentBlockInputText = 'This is textstring';
  const settingBlockInputText = 'This is textarea';
  const settingModelName = 'Test Setting Model';
  const textAreaDataTypeName = 'Textarea';
  const textAreaData = await umbracoApi.dataType.getByName(textAreaDataTypeName);
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(settingModelName, groupName, textAreaDataTypeName, textAreaData.id);
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithContentAndSettingsElementType(customDataTypeName, elementTypeId, settingsElementTypeId, true);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.enterTextstring(contentBlockInputText);
  await umbracoUi.content.clickAddBlockSettingsTabButton();
  await umbracoUi.content.enterTextArea(settingBlockInputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toEqual(contentBlockInputText);
  expect(contentData.values[0].value.settingsData[0].values[0].value).toEqual(settingBlockInputText);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(settingModelName);
});

test.skip('can move blocks in the content', async ({umbracoApi, umbracoUi}) => {
  // TODO: Implement it later
});

test('can create content with a block list with the inline editing mode enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingMode(customDataTypeName, elementTypeId);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
});

test('can add a block element with inline editing mode enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'This is block test';
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithInlineEditingModeAndABlock(customDataTypeName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.clickInlineBlockCaretButtonForName(elementTypeName);
  await umbracoUi.content.enterTextstring(inputText);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  //await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.isErrorNotificationVisible(false);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toEqual(inputText);
  const blockListValue = contentData.values.find(item => item.editorAlias === "Umbraco.BlockList")?.value;
  expect(blockListValue).toBeTruthy();
});