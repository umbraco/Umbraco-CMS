import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Content
const contentName = 'TestContent';
const contentGroupName = 'TestContentGroup';
// DocumentType
const documentTypeName = 'TestDocumentTypeForContent';
// DataType
const dataTypeName = 'Textstring';
// BlockType
const blockGridName = 'TestBlockGridForContent';
const blockListName = 'TestBlockListForContent';
// ElementType
const elementGroupName = 'TestElementGroupForContent';
const elementTypeName1 = 'TestElementTypeForContent';
const elementTypeName2 = 'Hero Banner';
// Setting Model
const settingModelName = 'Test Setting Model';
const groupName = 'Test Group';
// Block Custom View
const blockGridCustomViewLocator = 'block-grid-custom-view';
const blockCustomViewLocator = 'block-custom-view';
// Property Editor
const propertyEditorName = 'Heading';
const propertyEditorSettingName = 'Theme';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridName);
  await umbracoApi.dataType.ensureNameNotExists(blockListName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName1);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName2);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('block custom view appears in a specific Block Type (BlockGrid)', async ({umbracoApi, umbracoUi}) => {
	// Arrange 
	const textStringDataType = await umbracoApi.dataType.getByName(dataTypeName);
	const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName1, elementGroupName, dataTypeName, textStringDataType.id);
	const blockGridId = await umbracoApi.dataType.createBlockGridWithABlock(blockGridName, elementTypeId);
	const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridName, blockGridId, contentGroupName);
	await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

	// Act
	await umbracoUi.goToBackOffice();
	await umbracoUi.content.goToSection(ConstantHelper.sections.content);
	await umbracoUi.content.goToContentWithName(contentName);
	await umbracoUi.content.clickAddBlockElementButton();
	await umbracoUi.content.clickBlockElementWithName(elementTypeName1);
	await umbracoUi.content.clickCreateModalButton();
	
	// Assert
	await umbracoUi.content.isBlockCustomViewVisible(blockGridCustomViewLocator);
	await umbracoUi.content.expectOnlyOneBlockElementVisible(false);
});

test('block custom view does not appear in a specific Block Type (BlockList)', async ({umbracoApi, umbracoUi}) => {
	// Arrange 
	const textStringDataType = await umbracoApi.dataType.getByName(dataTypeName);
	const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName1, elementGroupName, dataTypeName, textStringDataType.id);
	const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
	const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, contentGroupName);
	await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

	// Act
	await umbracoUi.goToBackOffice();
	await umbracoUi.content.goToSection(ConstantHelper.sections.content);
	await umbracoUi.content.goToContentWithName(contentName);
	await umbracoUi.content.clickAddBlockElementButton();
	await umbracoUi.content.clickBlockElementWithName(elementTypeName1);
	await umbracoUi.content.clickCreateModalButton();
	
	// Assert
	await umbracoUi.content.isBlockCustomViewVisible(blockGridCustomViewLocator, false);
	await umbracoUi.content.expectOnlyOneBlockElementVisible();
});

test('block custom view appears in a specific Block Element', async ({umbracoApi, umbracoUi}) => {
	// Arrange 
	const textStringDataType = await umbracoApi.dataType.getByName(dataTypeName);
	const elementTypeId1 = await umbracoApi.documentType.createDefaultElementType(elementTypeName1, elementGroupName, dataTypeName, textStringDataType.id);
	const elementTypeId2 = await umbracoApi.documentType.createDefaultElementType(elementTypeName2, elementGroupName, dataTypeName, textStringDataType.id);
	const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithTwoBlocks(blockListName, elementTypeId1, elementTypeId2);
	const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, contentGroupName);
	await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

	// Act
	await umbracoUi.goToBackOffice();
	await umbracoUi.content.goToSection(ConstantHelper.sections.content);
	await umbracoUi.content.goToContentWithName(contentName);
	await umbracoUi.content.clickAddBlockElementButton();
	await umbracoUi.content.clickBlockCardWithName(elementTypeName2);
	await umbracoUi.content.clickCreateModalButton();
	await umbracoUi.content.clickAddBlockElementButton();
	await umbracoUi.content.clickBlockCardWithName(elementTypeName1);
	await umbracoUi.content.clickCreateModalButton();
	
	// Assert
	await umbracoUi.content.isBlockCustomViewVisible(blockCustomViewLocator);
	await umbracoUi.content.expectOnlyOneBlockElementVisible();
});

test('block custom view can display Content and Setting', async ({umbracoApi, umbracoUi}) => {
	// Arrange
	const contentValue = 'This is block test';
	const settingValue = 'This is setting test';
	const valueText = `Heading and Theme: ${contentValue} - ${settingValue}`;
	const textStringDataType = await umbracoApi.dataType.getByName(dataTypeName);
	const textAreaDataType = await umbracoApi.dataType.getByName('Textarea');
	const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName2, elementGroupName, propertyEditorName, textStringDataType.id);
	const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(settingModelName, groupName, propertyEditorSettingName, textAreaDataType.id);
	const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithContentAndSettingsElementType(blockListName, elementTypeId, settingsElementTypeId);
	const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, contentGroupName);
	await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

	// Act
	await umbracoUi.goToBackOffice();
	await umbracoUi.content.goToSection(ConstantHelper.sections.content);
	await umbracoUi.content.goToContentWithName(contentName);
	await umbracoUi.content.clickAddBlockElementButton();
	await umbracoUi.content.clickBlockCardWithName(elementTypeName2);
	await umbracoUi.content.enterTextstring(contentValue);
	await umbracoUi.content.clickAddBlockSettingsTabButton();
	await umbracoUi.content.enterTextArea(settingValue);
	await umbracoUi.content.clickCreateModalButton();
	await umbracoUi.waitForTimeout(5000);
	
	// Assert
	await umbracoUi.content.isBlockCustomViewVisible(blockCustomViewLocator);
	await umbracoUi.content.verifyBlockCustomViewDisplaysCorrectValues(blockCustomViewLocator, valueText);
});