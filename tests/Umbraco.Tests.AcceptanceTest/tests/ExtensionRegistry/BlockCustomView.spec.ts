import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// Content
const contentName = 'TestContent';
const contentGroupName = 'TestContentGroup';
// DocumentType
const documentTypeName = 'TestDocumentTypeForContent';
// DataType
const dataTypeName = 'Textstring';
const textAreaDataTypeName = 'Textarea';
// BlockType
const blockGridName = 'TestBlockGridForContent';
const blockListName = 'TestBlockListForContent';
// ElementType
const elementGroupName = 'TestElementGroupForContent';
const firstElementTypeName = 'TestElementTypeForContent';
const secondElementTypeName = 'Element Type For Custom Block View';
// Setting Model
const settingModelName = 'Test Setting Model';
const groupName = 'Test Group';
// Block Custom View
const blockGridCustomViewLocator = 'block-grid-custom-view';
const blockCustomViewLocator = 'block-custom-view';
// Property Editor
const propertyEditorName = 'Heading';
const propertyEditorSettingName = 'Theme';

test.afterEach(async ({ umbracoApi }) => {
	await umbracoApi.document.ensureNameNotExists(contentName);
	await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
	await umbracoApi.dataType.ensureNameNotExists(blockGridName);
	await umbracoApi.dataType.ensureNameNotExists(blockListName);
	await umbracoApi.documentType.ensureNameNotExists(firstElementTypeName);
	await umbracoApi.documentType.ensureNameNotExists(secondElementTypeName);
	await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('block custom view appears in a specific block type', async ({umbracoApi, umbracoUi}) => {
	// Arrange 
	const textStringDataType = await umbracoApi.dataType.getByName(dataTypeName);
	const elementTypeId = await umbracoApi.documentType.createDefaultElementType(firstElementTypeName, elementGroupName, dataTypeName, textStringDataType.id);
	const blockGridId = await umbracoApi.dataType.createBlockGridWithABlock(blockGridName, elementTypeId);
	const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridName, blockGridId, contentGroupName);
	await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

	// Act
	await umbracoUi.goToBackOffice();
	await umbracoUi.content.goToSection(ConstantHelper.sections.content);
	await umbracoUi.content.goToContentWithName(contentName);
	await umbracoUi.content.clickAddBlockElementButton();
	await umbracoUi.content.clickBlockElementWithName(firstElementTypeName);
	await umbracoUi.content.clickCreateModalButton();

	// Assert
	await umbracoUi.content.isBlockCustomViewVisible(blockGridCustomViewLocator);
	await umbracoUi.content.isSingleBlockElementVisible(false);
});

test('block custom view does not appear in block list editor when configured for block grid only', async ({umbracoApi, umbracoUi}) => {
	// Arrange 
	const textStringDataType = await umbracoApi.dataType.getByName(dataTypeName);
	const elementTypeId = await umbracoApi.documentType.createDefaultElementType(firstElementTypeName, elementGroupName, dataTypeName, textStringDataType.id);
	const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId);
	const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, contentGroupName);
	await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

	// Act
	await umbracoUi.goToBackOffice();
	await umbracoUi.content.goToSection(ConstantHelper.sections.content);
	await umbracoUi.content.goToContentWithName(contentName);
	await umbracoUi.content.clickAddBlockElementButton();
	await umbracoUi.content.clickBlockElementWithName(firstElementTypeName);
	await umbracoUi.content.clickCreateModalButton();

	// Assert
	await umbracoUi.content.isBlockCustomViewVisible(blockGridCustomViewLocator, false);
	await umbracoUi.content.isSingleBlockElementVisible();
});

test('block custom view applies to correct content type', async ({umbracoApi, umbracoUi}) => {
	// Arrange 
	const textStringDataType = await umbracoApi.dataType.getByName(dataTypeName);
	const firstElementTypeId = await umbracoApi.documentType.createDefaultElementType(firstElementTypeName, elementGroupName, dataTypeName, textStringDataType.id);
	const secondElementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementTypeName, elementGroupName, dataTypeName, textStringDataType.id);
	const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithTwoBlocks(blockListName, firstElementTypeId, secondElementTypeId);
	const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, contentGroupName);
	await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

	// Act
	await umbracoUi.goToBackOffice();
	await umbracoUi.content.goToSection(ConstantHelper.sections.content);
	await umbracoUi.content.goToContentWithName(contentName);
	await umbracoUi.content.clickAddBlockElementButton();
	await umbracoUi.content.clickBlockCardWithName(secondElementTypeName);
	await umbracoUi.content.clickCreateModalButton();
	await umbracoUi.content.clickAddBlockElementButton();
	await umbracoUi.content.clickBlockCardWithName(firstElementTypeName);
	await umbracoUi.content.clickCreateModalButton();

	// Assert
	await umbracoUi.content.isBlockCustomViewVisible(blockCustomViewLocator);
	await umbracoUi.content.isSingleBlockElementVisible();
});

test('block custom view can display values from the content and settings parts', async ({umbracoApi, umbracoUi}) => {
	// Arrange
	const contentValue = 'This is block test';
	const settingValue = 'This is setting test';
	const valueText = `Heading and Theme: ${contentValue} - ${settingValue}`;
	const textStringDataType = await umbracoApi.dataType.getByName(dataTypeName);
	const textAreaDataType = await umbracoApi.dataType.getByName(textAreaDataTypeName);
	const elementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementTypeName, elementGroupName, propertyEditorName, textStringDataType.id);
	const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(settingModelName, groupName, propertyEditorSettingName, textAreaDataType.id);
	const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithContentAndSettingsElementType(blockListName, elementTypeId, settingsElementTypeId);
	const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, contentGroupName);
	await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

	// Act
	await umbracoUi.goToBackOffice();
	await umbracoUi.content.goToSection(ConstantHelper.sections.content);
	await umbracoUi.content.goToContentWithName(contentName);
	await umbracoUi.content.clickAddBlockElementButton();
	await umbracoUi.content.clickBlockCardWithName(secondElementTypeName);
	await umbracoUi.content.enterTextstring(contentValue);
	await umbracoUi.content.clickAddBlockSettingsTabButton();
	await umbracoUi.content.enterTextArea(settingValue);
	await umbracoUi.content.clickCreateModalButton();

	// Assert
	await umbracoUi.content.isBlockCustomViewVisible(blockCustomViewLocator);
	await umbracoUi.content.doesBlockCustomViewHaveValue(blockCustomViewLocator, valueText);
});

test('block custom view can display values from the content and settings parts after update', async ({umbracoApi, umbracoUi}) => {
	// Arrange
	const contentValue = 'This is block test';
	const settingValue = 'This is setting test';
	const updatedContentValue = 'This is updated block test';
	const updatedSettingValue = 'This is updated setting test';
	const updatedValueText = `Heading and Theme: ${updatedContentValue} - ${updatedSettingValue}`;
	const textStringDataType = await umbracoApi.dataType.getByName(dataTypeName);
	const textAreaDataType = await umbracoApi.dataType.getByName(textAreaDataTypeName);
	const elementTypeId = await umbracoApi.documentType.createDefaultElementType(secondElementTypeName, elementGroupName, propertyEditorName, textStringDataType.id);
	const settingsElementTypeId = await umbracoApi.documentType.createDefaultElementType(settingModelName, groupName, propertyEditorSettingName, textAreaDataType.id);
	const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithContentAndSettingsElementType(blockListName, elementTypeId, settingsElementTypeId);
	const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockListName, blockListId, contentGroupName);
	await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);

	// Act
	await umbracoUi.goToBackOffice();
	await umbracoUi.content.goToSection(ConstantHelper.sections.content);
	await umbracoUi.content.goToContentWithName(contentName);
	await umbracoUi.content.clickAddBlockElementButton();
	await umbracoUi.content.clickBlockCardWithName(secondElementTypeName);
	await umbracoUi.content.enterTextstring(contentValue);
	await umbracoUi.content.clickAddBlockSettingsTabButton();
	await umbracoUi.content.enterTextArea(settingValue);
	await umbracoUi.content.clickCreateModalButton();
	await umbracoUi.content.clickEditBlockListBlockButton();
	await umbracoUi.content.enterTextstring(updatedContentValue);
	await umbracoUi.content.clickAddBlockSettingsTabButton();
	await umbracoUi.content.enterTextArea(updatedSettingValue);
	await umbracoUi.content.clickUpdateButton();

	// Assert
	await umbracoUi.content.isBlockCustomViewVisible(blockCustomViewLocator);
	await umbracoUi.content.doesBlockCustomViewHaveValue(blockCustomViewLocator, updatedValueText);
});