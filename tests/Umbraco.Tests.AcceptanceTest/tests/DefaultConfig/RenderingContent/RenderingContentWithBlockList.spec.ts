import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

// Content
const contentName = 'Test Rendering Content';
// Document Type
const documentTypeName = 'TestDocumentTypeForContent';
// Template
const templateName = 'TestTemplateForContent';
let templateId: string;
// Data Types
const blockListDataTypeName = 'CustomBlockList';
const textstringDataTypeName = 'Textstring';
const elementPropertyEditorAlias = 'Umbraco.TextBox';
// Element Type
const elementTypeName = 'BlockListElement';
const elementGroupName = 'ElementGroup';
const elementPropertyAlias = AliasHelper.toAlias(textstringDataTypeName);
let elementTypeId: string;
// Other
const noBlocksMessage = 'Test no block list mesage';

test.beforeEach(async ({umbracoApi}) => {
  const textstringData = await umbracoApi.dataType.getByName(textstringDataTypeName);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, elementGroupName, textstringDataTypeName, textstringData.id) || '';
  templateId = await umbracoApi.template.createTemplateWithDisplayingBlockListItems(templateName, blockListDataTypeName, elementPropertyAlias, noBlocksMessage);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockListDataTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can render content with a block list containing a textstring value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockTextValue = 'This is block list content';
  // Create document with a block
  const documentId = await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockWithValue(
    contentName,
    documentTypeName,
    blockListDataTypeName,
    elementTypeId,
    elementPropertyAlias,
    blockTextValue,
    elementPropertyEditorAlias,
    elementGroupName,
    templateId
  );
  // Publish the document
  await umbracoApi.document.publish(documentId);
  const contentURL = await umbracoApi.document.getDocumentUrl(documentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(blockTextValue);
});

test('can render content with a block list containing multiple blocks', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstBlockTextValue = 'First block content';
  const secondBlockTextValue = 'Second block content';
  // Create document with two blocks
  const documentId = await umbracoApi.document.createDefaultDocumentWithABlockListEditorAndBlockWithTwoValues(
    contentName,
    documentTypeName,
    blockListDataTypeName,
    elementTypeId,
    elementPropertyAlias,
    firstBlockTextValue,
    elementPropertyEditorAlias,
    elementGroupName,
    secondBlockTextValue,
    templateId
  );
  // Publish the document
  await umbracoApi.document.publish(documentId);
  const contentURL = await umbracoApi.document.getDocumentUrl(documentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(firstBlockTextValue);
  await umbracoUi.contentRender.doesContentRenderValueContainText(secondBlockTextValue);
});

test('can render content with an empty block list', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create document with no blocks
  const blockListDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListDataTypeName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, blockListDataTypeId, blockListDataTypeName, templateId);
  const documentId = await umbracoApi.document.createDocumentWithTemplate(contentName, documentTypeId, templateId);
  // Publish the document
  await umbracoApi.document.publish(documentId);
  const contentURL = await umbracoApi.document.getDocumentUrl(documentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(noBlocksMessage);
});