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
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.waitForContentToBeCreated();
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
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe(expectedState);
  expect(contentData.values).toEqual([]);
});

test('can add a block element in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'This is block test';
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithPermissions(customDataTypeName, elementTypeId, true, true);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
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
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toEqual(inputText);
  const blockGridValue = contentData.values.find(item => item.editorAlias === "Umbraco.BlockGrid")?.value;
  expect(blockGridValue).toBeTruthy();
});

test('can edit block element in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedText = 'This updated block test';
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickEditBlockGridBlockButton();
  await umbracoUi.content.enterTextstring(updatedText);
  await umbracoUi.content.clickUpdateButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toEqual(updatedText);
});

test('can delete block element in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditor(contentName, elementTypeId, documentTypeName, customDataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickDeleteBlockGridBlockButton();
  await umbracoUi.content.clickConfirmToDeleteButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  const contentData = await umbracoApi.document.getByName(contentName);
  const blockGridValue = contentData.values.find(item => item.value);
  expect(blockGridValue).toBeFalsy();
});

test('cannot add block element if allow in root is disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithPermissions(customDataTypeName, elementTypeId, false, false);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);

  // Assert
  await umbracoUi.content.isAddBlockElementButtonVisible(false);
});

test('cannot add number of block element greater than the maximum amount', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndMinAndMaxAmount(customDataTypeName, elementTypeId, 0, 0);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.waitForTimeout(500);
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.clickCreateModalButton();

  // Assert
  await umbracoUi.content.doesFormValidationMessageContainText('Maximum 0 entries, you have entered 1 too many.');
});

test('can set the label of create button in root', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const createButtonLabel = 'Test Create Button Label';
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndCreateButtonLabel(customDataTypeName, elementTypeId, createButtonLabel);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);

  // Assert
  await umbracoUi.content.isAddBlockElementButtonWithLabelVisible(customDataTypeName, createButtonLabel);
});

test('can set the label of block element in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockLabel = 'Test Block Label';
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithLabel(customDataTypeName, elementTypeId, blockLabel);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  await umbracoUi.content.doesBlockElementHaveName(blockLabel);
});

test('can set the number of columns for the layout in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const gridColumns = 6;
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockAndGridColumns(customDataTypeName, elementTypeId, gridColumns);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
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
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
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
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
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

test('can create content with a block grid with the inline editing mode enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockWithInlineEditingMode(customDataTypeName, elementTypeId);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateActionMenuOption();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(contentName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible(false);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
});

test('can add a block element with inline editing mode enabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'This is block test';
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlockWithInlineEditingMode(customDataTypeName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(inputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value).toEqual(inputText);
  const blockGridValue = contentData.values.find(item => item.editorAlias === "Umbraco.BlockGrid")?.value;
  expect(blockGridValue).toBeTruthy();
  await umbracoUi.content.doesPropertyContainValue(propertyInBlock, inputText);
});

test('can add an invariant block element with an invariant RTE Tiptap in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'This is block test';
  const customRTEDataTypeName = 'TestRTETiptap';
  const customElementTypeName = 'BlockGridWithRTEElement';
  const customRTEDataTypeId = await umbracoApi.dataType.createDefaultTiptapDataType(customRTEDataTypeName);
  const customElementTypeId = await umbracoApi.documentType.createDefaultElementType(customElementTypeName, groupName, customRTEDataTypeName, customRTEDataTypeId);
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithPermissions(customDataTypeName, customElementTypeId, true, true);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(customElementTypeName);
  await umbracoUi.content.enterRTETipTapEditor(inputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButtonForContent();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value.markup).toContain(inputText);
  const blockGridValue = contentData.values.find(item => item.editorAlias === "Umbraco.BlockGrid")?.value;
  expect(blockGridValue).toBeTruthy();

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customRTEDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(customElementTypeName);
});

test('can add a variant block element with variant RTE Tiptap in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'This is block test';
  const customRTEDataTypeName = 'TestRTETiptap';
  const customElementTypeName = 'BlockGridWithRTEElement';
  await umbracoApi.language.createDanishLanguage();
  const customRTEDataTypeId = await umbracoApi.dataType.createDefaultTiptapDataType(customRTEDataTypeName);
  const customElementTypeId = await umbracoApi.documentType.createDefaultElementType(customElementTypeName, groupName, customRTEDataTypeName, customRTEDataTypeId);
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithPermissions(customDataTypeName, customElementTypeId, true, true);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId, 'testGroup', true);
  await umbracoApi.document.createDefaultDocumentWithCulture(contentName, documentTypeId, 'en-US');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(customElementTypeName);
  await umbracoUi.content.enterRTETipTapEditor(inputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value.markup).toContain(inputText);
  const blockGridValue = contentData.values.find(item => item.editorAlias === "Umbraco.BlockGrid")?.value;
  expect(blockGridValue).toBeTruthy();

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customRTEDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(customElementTypeName);
  await umbracoApi.language.ensureNameNotExists('Danish');
});

test('can add a variant block element with invariant RTE Tiptap in the content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const inputText = 'This is block test';
  const customRTEDataTypeName = 'TestRTETiptap';
  const customElementTypeName = 'BlockGridWithRTEElement';
  await umbracoApi.language.createDanishLanguage();
  const customRTEDataTypeId = await umbracoApi.dataType.createDefaultTiptapDataType(customRTEDataTypeName);
  const customElementTypeId = await umbracoApi.documentType.createDefaultElementType(customElementTypeName, groupName, customRTEDataTypeName, customRTEDataTypeId);
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithPermissions(customDataTypeName, customElementTypeId, true, true);
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
  await umbracoApi.document.createDefaultDocumentWithCulture(contentName, documentTypeId, 'en-US');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickTextButtonWithName(customElementTypeName);
  await umbracoUi.content.enterRTETipTapEditor(inputText);
  await umbracoUi.content.clickCreateModalButton();
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveButton();
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].value.contentData[0].values[0].value.markup).toContain(inputText);
  const blockGridValue = contentData.values.find(item => item.editorAlias === "Umbraco.BlockGrid")?.value;
  expect(blockGridValue).toBeTruthy();

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(customRTEDataTypeName);
  await umbracoApi.documentType.ensureNameNotExists(customElementTypeName);
  await umbracoApi.language.ensureNameNotExists('Danish');
});
