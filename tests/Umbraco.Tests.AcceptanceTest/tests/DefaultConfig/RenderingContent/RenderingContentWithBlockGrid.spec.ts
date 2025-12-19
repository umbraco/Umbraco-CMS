import {AliasHelper, test} from '@umbraco/playwright-testhelpers';

// Content
const contentName = 'Test Rendering Content';
// Document Type
const documentTypeName = 'TestDocumentTypeForContent';
// Template
const templateName = 'TestTemplateForContent';
let templateId: string;
// Data Types
const blockGridDataTypeName = 'CustomBlockGrid';
const textstringDataTypeName = 'Textstring';
const elementPropertyEditorAlias = 'Umbraco.TextBox';
// Element Type
const elementTypeName = 'BlockGridElement';
const elementGroupName = 'ElementGroup';
const elementPropertyAlias = AliasHelper.toAlias(textstringDataTypeName);
let elementTypeId: string;
// Other
const noBlocksMessage = 'Test no block grid message';

test.beforeEach(async ({umbracoApi}) => {
  const textstringData = await umbracoApi.dataType.getByName(textstringDataTypeName);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, elementGroupName, textstringDataTypeName, textstringData.id) || '';
  templateId = await umbracoApi.template.createTemplateWithDisplayingBlockGridItems(templateName, blockGridDataTypeName, elementPropertyAlias, noBlocksMessage);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockGridDataTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can render content with a block grid containing a textstring value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockTextValue = 'This is block grid content';
  // Create document with a block
  const documentId = await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithValue(
    contentName,
    documentTypeName,
    blockGridDataTypeName,
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

test('can render content with a block grid containing multiple blocks', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstBlockTextValue = 'First block content';
  const secondBlockTextValue = 'Second block content';
  // Create document with two blocks
  const documentId = await umbracoApi.document.createDefaultDocumentWithABlockGridEditorAndBlockWithTwoValues(
    contentName,
    documentTypeName,
    blockGridDataTypeName,
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

test('can render content with an empty block grid', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create block grid data type
  const blockGridDataTypeId = await umbracoApi.dataType.createBlockGridWithABlock(blockGridDataTypeName, elementTypeId);
  // Create document type with block grid property and allowed template
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, blockGridDataTypeId, blockGridDataTypeName, templateId);
  // Create document with no blocks
  const documentId = await umbracoApi.document.createDocumentWithTemplate(contentName, documentTypeId, templateId);
  // Publish the document
  await umbracoApi.document.publish(documentId);
  const contentURL = await umbracoApi.document.getDocumentUrl(documentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(noBlocksMessage);
});
