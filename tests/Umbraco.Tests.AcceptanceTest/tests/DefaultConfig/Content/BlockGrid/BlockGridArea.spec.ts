import {expect} from '@playwright/test';
import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Document Type
const documentTypeName = 'TestDocumentTypeForContent';
let documentTypeId = '';

// Content
const contentName = 'TestContent';

// Element Types
const firstElementTypeName = 'FirstBlockGridElement';
let firstElementTypeId = null;
const secondElementTypeName = 'SecondBlockGridElement';

// Block Grid Data Type
const blockGridDataTypeName = 'BlockGridTester';
let blockGridDataTypeId = null;
const firstAreaName = 'FirstArea';
const areaCreateLabel = 'CreateLabel';
const toAllowInAreas = true;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(firstElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(firstElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
});

test('can create content with a block grid with an empty block in a area', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  firstElementTypeId = await umbracoApi.documentType.createEmptyElementType(firstElementTypeName);
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithAnAreaInABlockWithAllowInAreas(blockGridDataTypeName, firstElementTypeId, firstAreaName, toAllowInAreas, areaCreateLabel);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockGridElementWithName(firstElementTypeName);
  await umbracoUi.content.clickSelectBlockElementWithName(firstElementTypeName);
  await umbracoUi.content.clickLinkWithName(areaCreateLabel);
  await umbracoUi.content.clickSelectBlockElementInAreaWithName(firstElementTypeName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesBlockContainBlockInAreaWithName(firstElementTypeName, firstAreaName, firstElementTypeName);
  await umbracoUi.content.doesBlockContainBlockCountInArea(firstElementTypeName, firstAreaName, 1);
});

test('can create content with a block grid with two empty blocks in a area', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  firstElementTypeId = await umbracoApi.documentType.createEmptyElementType(firstElementTypeName);
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithAnAreaInABlockWithAllowInAreas(blockGridDataTypeName, firstElementTypeId, firstAreaName, toAllowInAreas, areaCreateLabel);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockGridElementWithName(firstElementTypeName);
  await umbracoUi.content.clickSelectBlockElementWithName(firstElementTypeName);
  await umbracoUi.content.clickLinkWithName(areaCreateLabel);
  await umbracoUi.content.clickSelectBlockElementInAreaWithName(firstElementTypeName);
  await umbracoUi.content.addBlockToAreasWithExistingBlock(firstElementTypeName, firstAreaName, 0, 0);
  await umbracoUi.content.clickSelectBlockElementInAreaWithName(firstElementTypeName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesBlockContainCountOfBlockInArea(firstElementTypeName, firstAreaName, firstElementTypeName, 2);

  // Checks if the block grid contains the blocks through the API
  const parentBlockKey = await umbracoUi.content.getBlockAtRootDataElementKey(firstElementTypeName, 0);
  const areaKey = await umbracoUi.content.getBlockAreaKeyFromParentBlockDataElementKey(parentBlockKey, 0);
  const firstBlockInAreaKey = await umbracoUi.content.getBlockDataElementKeyInArea(firstElementTypeName, firstAreaName, firstElementTypeName, 0, 0);
  const secondBlockInAreaKey = await umbracoUi.content.getBlockDataElementKeyInArea(firstElementTypeName, firstAreaName, firstElementTypeName, 0, 1);
  expect(await umbracoApi.document.doesBlockGridContainBlocksWithDataElementKeyInAreaWithKey(contentName, AliasHelper.toAlias(blockGridDataTypeName), parentBlockKey, areaKey, [firstBlockInAreaKey, secondBlockInAreaKey])).toBeTruthy();
});

test('can create content with block grid area with a create label', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  firstElementTypeId = await umbracoApi.documentType.createEmptyElementType(firstElementTypeName);
  const createLabel = 'ThisIsACreateLabel';
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithAnAreaInABlockWithACreateLabel(blockGridDataTypeName, firstElementTypeId, createLabel, firstAreaName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockGridElementWithName(firstElementTypeName);
  await umbracoUi.content.clickSelectBlockElementWithName(firstElementTypeName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  await umbracoUi.content.doesBlockGridBlockWithAreaContainCreateLabel(firstElementTypeName, createLabel);
});

test('can create content with block grid area with column span', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  firstElementTypeId = await umbracoApi.documentType.createEmptyElementType(firstElementTypeName);
  const columnSpan = 2;
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithAnAreaInABlockWithColumnSpanAndRowSpan(blockGridDataTypeName, firstElementTypeId, columnSpan, 1, firstAreaName, areaCreateLabel);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockGridElementWithName(firstElementTypeName);
  await umbracoUi.content.clickSelectBlockElementWithName(firstElementTypeName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  await umbracoUi.content.doesBlockAreaContainColumnSpan(firstElementTypeName, firstAreaName, columnSpan, 0);
});

test('can create content with block grid area with row span', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  firstElementTypeId = await umbracoApi.documentType.createEmptyElementType(firstElementTypeName);
  const rowSpan = 4;
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithAnAreaInABlockWithColumnSpanAndRowSpan(blockGridDataTypeName, firstElementTypeId, 12, rowSpan, firstAreaName, areaCreateLabel);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockGridElementWithName(firstElementTypeName);
  await umbracoUi.content.clickSelectBlockElementWithName(firstElementTypeName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();
  await umbracoUi.content.doesBlockAreaContainRowSpan(firstElementTypeName, firstAreaName, rowSpan, 0);
});

test('can create content with block grid area with min allowed', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  firstElementTypeId = await umbracoApi.documentType.createEmptyElementType(firstElementTypeName);
  const secondElementTypeId = await umbracoApi.documentType.createEmptyElementType(secondElementTypeName);
  const minAllowed = 2;
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithAnAreaInABlockWithMinAndMaxAllowed(blockGridDataTypeName, firstElementTypeId, secondElementTypeId, minAllowed, 10, firstAreaName, areaCreateLabel);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockGridElementWithName('content');
  await umbracoUi.content.clickSelectBlockElementWithName(firstElementTypeName);
  await umbracoUi.content.clickLinkWithName(areaCreateLabel);
  await umbracoUi.content.clickSelectBlockElementInAreaWithName(secondElementTypeName);
  await umbracoUi.content.isTextWithExactNameVisible('Minimum 2 entries, requires 1 more.');
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  await umbracoUi.content.clickInlineAddToAreaButton(firstElementTypeName, firstAreaName, 0, 1);
  await umbracoUi.content.clickSelectBlockElementInAreaWithName(secondElementTypeName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeName);
});

test('can create content with block grid area with max allowed', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  firstElementTypeId = await umbracoApi.documentType.createEmptyElementType(firstElementTypeName);
  const secondElementTypeId = await umbracoApi.documentType.createEmptyElementType(secondElementTypeName);
  const maxAllowed = 0;
  blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithAnAreaInABlockWithMinAndMaxAllowed(blockGridDataTypeName, firstElementTypeId, secondElementTypeId, 0, maxAllowed, firstAreaName, areaCreateLabel);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, blockGridDataTypeName, blockGridDataTypeId);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickAddBlockGridElementWithName('content');
  await umbracoUi.content.clickSelectBlockElementWithName(firstElementTypeName);
  await umbracoUi.content.clickLinkWithName(areaCreateLabel);
  await umbracoUi.content.clickSelectBlockElementInAreaWithName(secondElementTypeName);
  await umbracoUi.content.isTextWithExactNameVisible('Maximum 0 entries, you have entered 1 too many.');
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  await umbracoUi.content.removeBlockFromArea(firstElementTypeName, firstAreaName, secondElementTypeName);
  await umbracoUi.content.clickConfirmToDeleteButton();
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.isSuccessStateVisibleForSaveAndPublishButton();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeName);
});
