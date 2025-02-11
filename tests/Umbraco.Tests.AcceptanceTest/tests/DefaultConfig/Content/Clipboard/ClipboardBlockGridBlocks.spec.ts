import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// DocumentType
const documentTypeName = 'TestDocumentType';
const secondGroupName = 'SecondGroupName';
const groupName = 'TestGroup';
const secondBlockGridPropertyName = 'TesterBlockGridProperty';

// Content
const contentName = 'TestContent';
const secondContentName = "ContentNumberTwoTest"
const blockPropertyValue = 'This is a test';
const secondBlockPropertyValue = 'Yet Another test';

// ElementType
const elementGroupName = 'ElementGroup';
const elementTypeName = 'TestElement';
const elementDataTypeName = 'Richtext editor';
const richTextDataTypeUiAlias = 'Umbraco.RichText';
const elementPropertyName = 'TipTapProperty'
let elementTypeId = '';

// DataType
const blockGridDataTypeName = 'TestBlockGridEditor';
let tipTapDataTypeId = '';

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

test('can copy a single block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithValue(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, richTextDataTypeUiAlias);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockGridBlockButton(groupName, blockGridDataTypeName, elementTypeName);

  // Assert
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockGridDataTypeName);
  await umbracoUi.content.doesClipboardHaveCopiedBlockWithName(contentName, blockGridDataTypeName, elementTypeName);
});

test('can copy and paste a single block into the same document and group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithValue(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, richTextDataTypeUiAlias, groupName);
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
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied block
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
});

test('can copy and paste a single block into the same document but different group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithValueAndTwoGroups(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, richTextDataTypeUiAlias, groupName, secondBlockGridPropertyName, secondGroupName);
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
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied block
  await umbracoUi.content.goToBlockGridBlockWithName(secondGroupName, secondBlockGridPropertyName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
});

test('can copy and paste a single block into another document', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.ensureNameNotExists(secondContentName);
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithValue(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, richTextDataTypeUiAlias);
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
  await umbracoUi.content.isSuccessNotificationVisible();
  // Copied block
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);

  // Clean
  await umbracoApi.document.ensureNameNotExists(secondContentName);
});

test('can copy and paste multiple blocks into the same document and group', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithTwoValues(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, richTextDataTypeUiAlias, groupName, secondBlockPropertyValue);
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

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  // Original blocks
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied blocks
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 2);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 3);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
});

test('can copy and paste multiple blocks into the same document but different group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithTwoValuesAndTwoGroups(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, richTextDataTypeUiAlias, groupName, secondBlockPropertyValue, secondBlockGridPropertyName, secondGroupName);
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

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  // Original blocks
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied blocks
  await umbracoUi.content.goToBlockGridBlockWithName(secondGroupName, secondBlockGridPropertyName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(secondGroupName, secondBlockGridPropertyName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
});

test('can copy and paste multiple blocks into another document',  async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.ensureNameNotExists(secondContentName);
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithTwoValues(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, richTextDataTypeUiAlias, groupName, secondBlockPropertyValue);
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

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  // Copied blocks
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();

  // Original blocks
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();

  // Clean
  await umbracoApi.document.ensureNameNotExists(secondContentName);
});

test('can replace multiple blocks',  async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithTwoValues(contentName, documentTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, richTextDataTypeUiAlias, groupName, secondBlockPropertyValue);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesBlockGridPropertyHaveBlockAmount(groupName, blockGridDataTypeName, 2);
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.clickCopyBlockGridBlockButton(groupName, blockGridDataTypeName, elementTypeName, 0);

  await umbracoUi.content.clickActionsMenuForProperty(groupName, blockGridDataTypeName);
  await umbracoUi.content.clickExactReplaceButton();
  await umbracoUi.content.selectClipboardEntryWithName(contentName, blockGridDataTypeName, elementTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickPasteButton();
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.doesBlockGridPropertyHaveBlockAmount(groupName, blockGridDataTypeName, 1);
});

test('can copy block from a block grid to a block list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockListDataTypeName = 'TestBlockListEditor';
  const blockListGroupName = 'BlockListGroup';
  const blockListPropertyValue = 'This is a block list test';
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockGridEditorWithSameAllowedBlock(contentName, documentTypeName, blockListDataTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockListPropertyValue, richTextDataTypeUiAlias, blockListGroupName, blockPropertyValue, richTextDataTypeUiAlias, groupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockGridBlockButton(groupName, blockGridDataTypeName, elementTypeName);
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(blockListGroupName, blockListDataTypeName);
  await umbracoUi.content.selectClipboardEntryWithName(contentName, blockGridDataTypeName, elementTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  // Original blocks
  // Block Grid
  await umbracoUi.content.goToBlockGridBlockWithName(groupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Block List
  await umbracoUi.content.goToBlockListBlockWithName(blockListGroupName, blockListDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockListPropertyValue);
  await umbracoUi.content.clickCloseButton();

  // Copied block
  await umbracoUi.content.goToBlockListBlockWithName(blockListGroupName, blockListDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
});

test('can not copy a block from a block grid to a block list without allowed blocks',async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockListElementTypeName = 'SecondElementType';
  const blockListDataTypeName = 'TestBlockListEditor';
  const blockListGroupName = 'BlockListGroup';
  const blockListPropertyValue = 'This is a block list test';
  const blockListElementPropertyName = 'SecondTipTapProperty';
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);
  const secondElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockListElementTypeName, elementGroupName, blockListElementPropertyName, tipTapDataTypeId);
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockGridEditorWithDifferentAllowedBlock(contentName, documentTypeName, blockListDataTypeName, blockGridDataTypeName, secondElementTypeId, AliasHelper.toAlias(blockListElementPropertyName), blockListPropertyValue, richTextDataTypeUiAlias, blockListGroupName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, richTextDataTypeUiAlias, groupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockGridBlockButton(groupName, blockGridDataTypeName, elementTypeName);

  // Assert
  // Checks if the block is visible in the blockList
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(blockListGroupName, blockListDataTypeName);
  await umbracoUi.content.doesClipboardContainCopiedBlocksCount(0);
  await umbracoUi.content.clickCloseButton();

  await umbracoUi.waitForTimeout(500);
  // Checks if the block is visible in the blockGrid
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(blockGridDataTypeName, blockGridDataTypeName);
  await umbracoUi.content.doesClipboardContainCopiedBlocksCount(1);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(blockListElementTypeName);
});
