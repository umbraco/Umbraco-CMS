import {AliasHelper, ConstantHelper, test} from '@umbraco/playwright-testhelpers';

// DocumentType
const documentTypeName = 'TestDocumentType';
const secondGroupName = 'SecondGroupName';
const groupName = 'TestGroup';
const secondBlockListPropertyName = 'TesterBlockListProperty';

// Content
const contentName = 'TestContent';
const secondContentName = "ContentNumberTwoTest"
const blockPropertyValue = 'This is a test';
const secondBlockPropertyValue = 'Yet Another test';

// ElementType
const elementGroupName = 'ElementGroup';
const elementTypeName = 'TestElement';
const richTextDataTypeUiAlias = 'Richtext editor';
const elementDataTypeUiAlias = 'Umbraco.RichText';
const elementPropertyName = 'TipTapProperty'
let elementTypeId = '';

// DataType
const blockListDataTypeName = 'TestBlockListEditor';
let tipTapDataTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  const tipTapDataType = await umbracoApi.dataType.getByName(richTextDataTypeUiAlias);
  tipTapDataTypeId = tipTapDataType.id;
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, elementGroupName, elementPropertyName, tipTapDataTypeId);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
});

test('can copy a single block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockWithValue(contentName, documentTypeName, blockListDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockListBlockButton(groupName, blockListDataTypeName, elementTypeName);

  // Assert
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockListDataTypeName);
  await umbracoUi.content.doesClipboardHaveCopiedBlockWithName(contentName, blockListDataTypeName, elementTypeName);
});

test('can copy and paste a single block into the same document and group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockWithValue(contentName, documentTypeName, blockListDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockListBlockButton(groupName, blockListDataTypeName, elementTypeName);
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockListDataTypeName);
  await umbracoUi.content.selectClipboardEntryWithName(contentName, blockListDataTypeName, elementTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  // Original block
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied block
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
});

test('can copy and paste a single block into the same document but different group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockWithValueAndTwoGroups(contentName, documentTypeName, blockListDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName, secondBlockListPropertyName, secondGroupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockListBlockButton(groupName, blockListDataTypeName, elementTypeName);
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(secondGroupName, secondBlockListPropertyName);
  await umbracoUi.content.selectClipboardEntryWithName(contentName, blockListDataTypeName, elementTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  // Original block
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied block
  await umbracoUi.content.goToBlockListBlockWithName(secondGroupName, secondBlockListPropertyName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
});

test('can copy and paste a single block into another document', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.ensureNameNotExists(secondContentName);
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockWithValue(contentName, documentTypeName, blockListDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName);
  const documentType = await umbracoApi.documentType.getByName(documentTypeName)
  await umbracoApi.document.createDefaultDocument(secondContentName, documentType.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockListBlockButton(groupName, blockListDataTypeName, elementTypeName);
  await umbracoUi.content.goToContentWithName(secondContentName);
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockListDataTypeName);
  await umbracoUi.content.selectClipboardEntryWithName(contentName, blockListDataTypeName, elementTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  // Copied block
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);

  // Clean
  await umbracoApi.document.ensureNameNotExists(secondContentName);
});

test('can copy and paste multiple blocks into the same document and group', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockWithTwoValues(contentName, documentTypeName, blockListDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName, secondBlockPropertyValue);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickActionsMenuForProperty(groupName, blockListDataTypeName);
  await umbracoUi.content.clickExactCopyButton();
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockListDataTypeName);
  await umbracoUi.content.selectClipboardEntriesWithName(contentName, blockListDataTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  // Original blocks
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied blocks
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 2);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 3);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
});

test('can copy and paste multiple blocks into the same document but different group', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockWithTwoValuesAndTwoGroups(contentName, documentTypeName, blockListDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName, secondBlockPropertyValue, secondBlockListPropertyName, secondGroupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickActionsMenuForProperty(groupName, blockListDataTypeName);
  await umbracoUi.content.clickExactCopyButton();
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(secondGroupName, secondBlockListPropertyName);
  await umbracoUi.content.selectClipboardEntriesWithName(contentName, blockListDataTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  // Original blocks
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Copied blocks
  await umbracoUi.content.goToBlockListBlockWithName(secondGroupName, secondBlockListPropertyName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockListBlockWithName(secondGroupName, secondBlockListPropertyName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
});

test('can copy and paste multiple blocks into another document', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.ensureNameNotExists(secondContentName);
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockWithTwoValues(contentName, documentTypeName, blockListDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName, secondBlockPropertyValue);
  const documentType = await umbracoApi.documentType.getByName(documentTypeName)
  await umbracoApi.document.createDefaultDocument(secondContentName, documentType.id);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickActionsMenuForProperty(groupName, blockListDataTypeName);
  await umbracoUi.content.clickExactCopyButton();
  await umbracoUi.content.goToContentWithName(secondContentName);
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockListDataTypeName);
  await umbracoUi.content.selectClipboardEntriesWithName(contentName, blockListDataTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  // Copied blocks
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();

  // Original blocks
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, secondBlockPropertyValue);
  await umbracoUi.content.clickCloseButton();

  // Clean
  await umbracoApi.document.ensureNameNotExists(secondContentName);
});

test('can replace multiple blocks', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockWithTwoValues(contentName, documentTypeName, blockListDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, elementDataTypeUiAlias, groupName, secondBlockPropertyValue);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesBlockListPropertyHaveBlockAmount(groupName, blockListDataTypeName, 2);
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  await umbracoUi.content.clickCopyBlockListBlockButton(groupName, blockListDataTypeName, elementTypeName, 0);

  await umbracoUi.content.clickActionsMenuForProperty(groupName, blockListDataTypeName);
  await umbracoUi.content.clickExactReplaceButton();
  await umbracoUi.content.selectClipboardEntryWithName(contentName, blockListDataTypeName, elementTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickPasteButton();
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  await umbracoUi.content.doesBlockListPropertyHaveBlockAmount(groupName, blockListDataTypeName, 1);
});

test('can copy block from a block list to a block grid',  async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockGridDataTypeName = 'TestBlockGridEditor';
  const blockGridGroupName = 'BlockGridGroup';
  const blockGridPropertyValue = 'This is a block grid test';
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockGridEditorWithSameAllowedBlock(contentName, documentTypeName, blockListDataTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, richTextDataTypeUiAlias, groupName, blockGridPropertyValue, richTextDataTypeUiAlias, blockGridGroupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockListBlockButton(groupName, blockListDataTypeName, elementTypeName);
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(blockGridGroupName, blockGridDataTypeName);
  await umbracoUi.content.selectClipboardEntryWithName(contentName, blockListDataTypeName, elementTypeName);
  await umbracoUi.content.clickSubmitButton();
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  // Original blocks
  // Block List
  await umbracoUi.content.goToBlockListBlockWithName(groupName, blockListDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
  await umbracoUi.content.clickCloseButton();
  // Block Grid
  await umbracoUi.content.goToBlockGridBlockWithName(blockGridGroupName, blockGridDataTypeName, elementTypeName, 0);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockGridPropertyValue);
  await umbracoUi.content.clickCloseButton();

  // Copied block
  await umbracoUi.content.goToBlockGridBlockWithName(blockGridGroupName, blockGridDataTypeName, elementTypeName, 1);
  await umbracoUi.content.doesBlockEditorBlockWithNameContainValue(elementGroupName, elementPropertyName, ConstantHelper.inputTypes.tipTap, blockPropertyValue);
});

test('can not copy a block from a block list to a block grid without allowed blocks', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockGridElementTypeName = 'SecondElementType';
  const blockGridDataTypeName = 'TestBlockGridEditor';
  const blockGridGroupName = 'BlockGridGroup';
  const blockGridPropertyValue = 'This is a block grid test';
  const blockGridElementPropertyName = 'SecondTipTapProperty';
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
  const secondElementTypeId = await umbracoApi.documentType.createDefaultElementType(blockGridElementTypeName, elementGroupName, blockGridElementPropertyName, tipTapDataTypeId);
  await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockGridEditorWithDifferentAllowedBlock(contentName, documentTypeName, blockListDataTypeName, blockGridDataTypeName, elementTypeId, AliasHelper.toAlias(elementPropertyName), blockPropertyValue, richTextDataTypeUiAlias, groupName, secondElementTypeId, AliasHelper.toAlias(blockGridElementPropertyName), blockGridPropertyValue, richTextDataTypeUiAlias, blockGridGroupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickCopyBlockListBlockButton(groupName, blockListDataTypeName, elementTypeName);

  // Assert
  // Checks if the block is visible in the blockList
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(blockGridGroupName, blockGridDataTypeName);
  await umbracoUi.content.doesClipboardContainCopiedBlocksCount(0);
  await umbracoUi.content.clickCloseButton();

  await umbracoUi.waitForTimeout(500);
  // Checks if the block is visible in the blockGrid
  await umbracoUi.content.clickPasteFromClipboardButtonForProperty(groupName, blockListDataTypeName);
  await umbracoUi.content.doesClipboardContainCopiedBlocksCount(1);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(blockGridElementTypeName);
});
