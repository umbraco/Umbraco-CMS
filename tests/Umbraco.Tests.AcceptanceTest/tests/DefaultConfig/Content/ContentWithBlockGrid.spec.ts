import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const customDataTypeName = 'Custom Block Grid';
const elementTypeName = 'BlockGridElement';
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

test('can create content with an empty block grid', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithPermissions(customDataTypeName, elementTypeId, true, true);
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
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values).toEqual([]);
});

test('can publish content with an empty block grid', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithPermissions(customDataTypeName, elementTypeId, true, true);
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
  await umbracoUi.content.doesSuccessNotificationsHaveCount(2);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values).toEqual([]);
});

test('can add a block element in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'This is block test';
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithPermissions(customDataTypeName, elementTypeId, true, true);
  const documentId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(inputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toEqual(inputText);
  const blockGridValue = contentData.values.find(item => item.editorAlias === "Umbraco.BlockGrid")?.value;
  expect(blockGridValue).toBeTruthy();
});

test.skip('can edit block element in the content', async ({umbracoApi, umbracoUi}) => {
});

test.skip('can delete block element in the content', async ({umbracoApi, umbracoUi}) => {

});

test('cannot add block element if allow in root is disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithPermissions(customDataTypeName, elementTypeId, false, false);
  const documentId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);

  // Assert
  await umbracoUi.content.isAddBlockElementButtonVisible(false);
});

test('cannot add number of block element greater than the maxiumm amount', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndMinAndMaxAmount(customDataTypeName, elementTypeId, 0, 0, true);
  const documentId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.clickCreateModalButton();

  // Assert
  await umbracoUi.content.doesFormValidationMessageContainText('Maximum');
  await umbracoUi.content.doesFormValidationMessageContainText('too many');
});

test('can set the label of create button in root', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const createButtonLabel = 'Test Create Button Label';
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndCreateButtonLabel(customDataTypeName, elementTypeId, createButtonLabel);
  const documentId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);

  // Assert
  await umbracoUi.content.isAddBlockElementButtonWithLabelVisible(createButtonLabel);
});

test('can set the label of block element in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockLabel = 'Test Block Label';
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithLabel(customDataTypeName, elementTypeId, blockLabel);
  const documentId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesBlockListBlockNameHaveName(blockLabel);
});

test('can set the number of columns for the layout in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const gridColumns = 6;
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndGridColumns(customDataTypeName, elementTypeId, gridColumns);
  const documentId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const contentData = await umbracoApi.document.getByName(contentName);
  const layoutValue = contentData.values[0]?.value.layout["Umbraco.BlockGrid"];
  expect(layoutValue[0].columnSpan).toBe(gridColumns);
});

test('can add settings model for the block in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const contentBlockInputText = 'This is textstring';
  const settingBlockInputText = 'This is textarea';
  const settingModelName = 'Test Setting Model';
  const textAreaDataTypeName = 'Textarea';
  const textAreaData = await umbracoApi.dataType.getByName(textAreaDataTypeName);
  const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(settingModelName, groupName, textAreaDataTypeName, textAreaData.id);
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithContentAndSettingsElementType(customDataTypeName, elementTypeId, settingsElementTypeId, true);
  const documentId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(contentBlockInputText);
  await umbracoUi.content.clickAddBlockSettingsTabButton();
  await umbracoUi.content.enterTextArea(settingBlockInputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toEqual(contentBlockInputText);
  expect(contentData.values[0].value.settingsData[0].values[0].value).toEqual(settingBlockInputText);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(settingModelName);
});