import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const documentTypeName = 'TestDocumentType';
const contentName = 'TestContent';
const secondContentName = "ContentNumberTwoTest"
const elementGroupName = 'ElementGroup';
const groupName = 'TestGroup';
const secondGroupName = 'SecondGroupName';
const elementTypeName = 'TestElement';
const elementDataTypeName = 'Richtext editor';
const elementDataTypeUiAlias = 'Umbraco.RichText';
const elementPropertyName = 'TipTapProperty'

const blockGridDataTypeName = 'TestBlockGridEditor';
const secondBlockGridPropertyName = 'TesterBlockGridProperty';
const blockPropertyValue = 'This is a test';
const secondBlockPropertyValue = 'Yet Another test';
let tipTapDataTypeId = '';
let elementTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
  const tipTapDataType = await umbracoApi.dataType.getByName(elementDataTypeName);
  tipTapDataTypeId = tipTapDataType.id;
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, elementGroupName, elementPropertyName, tipTapDataTypeId);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
});

test('can copy a single block', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithValue(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockGridBlockButton(groupName, blockGridDataTypeName, elementTypeName);

  // Assert
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockGridDataTypeName);
  await umbracoUi.content.doesClipboardHaveCopiedBlockWithName(contentName, blockGridDataTypeName, elementTypeName);
});

test('can copy and paste a single block into the same document and group', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithValue(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockGridBlockButton(groupName, blockGridDataTypeName, elementTypeName);
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockGridDataTypeName);
  await umbracoUi.content.selectClipboardEntryWithName(contentName, blockGridDataTypeName, elementTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  // Original block
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied block
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
});

test('can copy and paste a single block into the same document but different group', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithValueAndTwoGroups(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName, secondBlockGridPropertyName, secondGroupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockGridBlockButton(groupName, blockGridDataTypeName, elementTypeName);
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(secondGroupName, secondBlockGridPropertyName);
  await umbracoUi.content.selectClipboardEntryWithName(contentName, blockGridDataTypeName, elementTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  // Original block
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied block
  await umbracoUi.content.goToBlockGridBlockWithName(secondGroupName, secondBlockGridPropertyName, elementTypeName, 0);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
});

test('can copy and paste a single block into another document', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.ensureNameNotExists(secondContentName);
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithValue(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias);
  const documentType = await umbracoApi.documentType.getByName(documentTypeName)
  await umbracoApi.document.createDefaultDocument(secondContentName, documentType.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockGridBlockButton(groupName, blockGridDataTypeName, elementTypeName);
  await umbracoUi.content.goToContentWithName(secondContentName);
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockGridDataTypeName);
  await umbracoUi.content.selectClipboardEntryWithName(contentName, blockGridDataTypeName, elementTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  // Copied block
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);

  // Clean
  await umbracoApi.document.ensureNameNotExists(secondContentName);
});

test('can copy and paste multiple blocks into the same document and group', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithTwoValues(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName, secondBlockPropertyValue);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickActionsMenuForProperty(groupName, blockGridDataTypeName);
  await umbracoUi.content.clickExactCopyButton();
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockGridDataTypeName);
  await umbracoUi.content.selectClipboardEntriesWithName(contentName, blockGridDataTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Original blocks
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied blocks
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 2);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 3);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
});

test('can copy and paste multiple blocks into the same document but different group', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithTwoValuesAndTwoGroups(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName, secondBlockPropertyValue, secondBlockGridPropertyName, secondGroupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickActionsMenuForProperty(groupName, blockGridDataTypeName);
  await umbracoUi.content.clickExactCopyButton();
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(secondGroupName, secondBlockGridPropertyName);
  await umbracoUi.content.selectClipboardEntriesWithName(contentName, blockGridDataTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Original blocks
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied blocks
  await umbracoUi.content.goToBlockGridBlockWithName(secondGroupName, secondBlockGridPropertyName, elementTypeName, 0);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(secondGroupName, secondBlockGridPropertyName, elementTypeName, 1);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
});

test('can copy and paste multiple blocks into another document', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.ensureNameNotExists(secondContentName);
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithTwoValues(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName, secondBlockPropertyValue);
  const documentType = await umbracoApi.documentType.getByName(documentTypeName)
  await umbracoApi.document.createDefaultDocument(secondContentName, documentType.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickActionsMenuForProperty(groupName, blockGridDataTypeName);
  await umbracoUi.content.clickExactCopyButton();
  await umbracoUi.content.goToContentWithName(secondContentName);
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockGridDataTypeName);
  await umbracoUi.content.selectClipboardEntriesWithName(contentName, blockGridDataTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Copied blocks
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();

  // Original blocks
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockGridBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();

  // Clean
  await umbracoApi.document.ensureNameNotExists(secondContentName);
});
