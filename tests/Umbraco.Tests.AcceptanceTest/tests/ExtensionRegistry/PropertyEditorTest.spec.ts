import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Content
const contentName = 'TestContent';
// DocumentType
const documentTypeName = 'TestDocumentTypeForContent';
// DataType
const dataTypeName = 'CustomTextBox';
const editorUiAlias = 'Custom.TextEditor';
const editorAlias = 'Umbraco.TextBox';
// Property Editor
const customPropertyEditorName = 'Custom Text Editor';

// Test values
const testValue = 'This is a test value for the custom property editor';

test.afterEach(async ({umbracoApi}) => {
	await umbracoApi.document.ensureNameNotExists(contentName);
	await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test('custom property editor appears in Property Editor Picker', async ({umbracoApi, umbracoUi}) =>{
    //Arrange
    await umbracoUi.goToBackOffice();
    await umbracoUi.dataType.goToSection(ConstantHelper.sections.settings);

    await umbracoUi.dataType.clickActionsMenuAtRoot();
    await umbracoUi.dataType.clickCreateActionMenuOption();
	await umbracoUi.dataType.clickDataTypeButton();
    await umbracoUi.dataType.enterDataTypeName(dataTypeName);
    await umbracoUi.dataType.clickSelectAPropertyEditorButton();
	await umbracoUi.dataType.selectAPropertyEditor(customPropertyEditorName);
    await umbracoUi.dataType.clickSaveButton();

    // Assert
    await umbracoUi.dataType.waitForDataTypeToBeCreated();
    await umbracoUi.dataType.isDataTypeTreeItemVisible(dataTypeName);
    expect(await umbracoApi.dataType.doesNameExist(dataTypeName)).toBeTruthy();
});

test('custom property editor appears in Document type when configuring a property', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
    await umbracoApi.dataType.create(
		dataTypeName, 
		editorAlias, 
		editorUiAlias,
		[]
	); 
    await umbracoUi.goToBackOffice();
    await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

    //Act
    await umbracoUi.documentType.goToDocumentType(documentTypeName);
    await umbracoUi.documentType.clickAddGroupButton();
    await umbracoUi.documentType.addPropertyEditor(dataTypeName);
    await umbracoUi.documentType.enterGroupName('Content');
    await umbracoUi.documentType.clickSaveButton();

    // Assert
    await umbracoUi.documentType.waitForDocumentTypeToBeCreated();
    expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
    const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
    const dataType = await umbracoApi.dataType.getByName(dataTypeName);
    // Checks if the correct property was added to the document type
    expect(documentTypeData.properties[0].dataType.id).toBe(dataType.id);
});

test('user can write and read value from custom property editor', async({umbracoApi, umbracoUi}) => {
  // Arrange - Create Data Type with custom property editor
	const dataTypeId = await umbracoApi.dataType.create(
		dataTypeName, 
		editorAlias, 
		editorUiAlias,
		[{
			alias: 'maxChars', value: '100',
		}]
	);
    if (!dataTypeId) throw new Error('Data type id is undefined');

    // Create Document Type with custom property editor
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
    if (!documentTypeId) throw new Error('Document type id is undefined');

	// Create Content with custom property editor
	const documentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, 'Test content', dataTypeName);
	if (!documentId) throw new Error('Document id is undefined');

    await umbracoUi.goToBackOffice();
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);

    // Act - Navigate to content and write value
    await umbracoUi.content.goToContentWithName(contentName);

	await umbracoUi.content.enterPropertyValue(dataTypeName, testValue1);
    await umbracoUi.content.clickSaveButton();

	// Read value back from property editor using the API
	const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].value).toEqual(testValue1);
});