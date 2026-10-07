import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const contentName = 'TestRenderReusable';
const documentTypeName = 'TestDocumentTypeForRenderReusable';
const customDataTypeName = 'Custom Block List Render Reusable';
const elementTypeName = 'RenderReusableElement';
const libraryElementName = 'MyRenderLibraryElement';
const templateName = 'ReusableBlockTemplate';
const gridContentName = 'TestRenderReusableGrid';
const gridDocumentTypeName = 'TestDocumentTypeForRenderReusableGrid';
const gridCustomDataTypeName = 'Custom Block Grid Render Reusable';
const gridTemplateName = 'ReusableBlockGridTemplate';
const singleContentName = 'TestRenderReusableSingle';
const singleDocumentTypeName = 'TestDocumentTypeForRenderReusableSingle';
const singleCustomDataTypeName = 'Custom Single Block Render Reusable';
const singleTemplateName = 'ReusableSingleBlockTemplate';
const rteContentName = 'TestRenderReusableRte';
const rteDocumentTypeName = 'TestDocumentTypeForRenderReusableRte';
const rteCustomDataTypeName = 'Custom RichText Render Reusable';
const rteTemplateName = 'ReusableRichTextTemplate';
const rteSecondElementTypeName = 'RenderReusableSecondElement';
const rteTransferElementName = 'TransferredRenderRichTextElement';
const propertyInBlock = 'Textstring';
const groupName = 'testGroup';
let elementTypeId = '';
let rteBlockPartialView: {path: string; createdFolders: string[]} | undefined;

test.beforeEach(async ({umbracoApi}) => {
  const textStringData = await umbracoApi.dataType.getByName(propertyInBlock);
  elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, groupName, propertyInBlock, textStringData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(gridContentName);
  await umbracoApi.element.ensureNameNotExists(libraryElementName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(gridDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  await umbracoApi.dataType.ensureNameNotExists(gridCustomDataTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.template.ensureNameNotExists(gridTemplateName);
  await umbracoApi.document.ensureNameNotExists(singleContentName);
  await umbracoApi.documentType.ensureNameNotExists(singleDocumentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(singleCustomDataTypeName);
  await umbracoApi.template.ensureNameNotExists(singleTemplateName);
  await umbracoApi.document.ensureNameNotExists(rteContentName);
  await umbracoApi.documentType.ensureNameNotExists(rteDocumentTypeName);
  await umbracoApi.element.ensureNameNotExists(rteTransferElementName);
  await umbracoApi.documentType.ensureNameNotExists(rteSecondElementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(rteCustomDataTypeName);
  await umbracoApi.template.ensureNameNotExists(rteTemplateName);
  if (rteBlockPartialView) {
    await umbracoApi.partialView.deleteRichTextBlockPartialView(rteBlockPartialView);
    rteBlockPartialView = undefined;
  }
});

test('can render the referenced Library element content on the published page', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryText = 'Reusable content rendered on the front-end';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, libraryText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  const templateId = await umbracoApi.template.createTemplateWithDisplayingBlockListItems(templateName, customDataTypeName, propertyInBlock);
  const customDataTypeId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(customDataTypeName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, customDataTypeId, customDataTypeName, templateId);
  const documentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();
  const contentURL = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(libraryText);
});

test('can render the referenced Library element content on the published page for a Block Grid', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryText = 'Reusable grid content rendered on the front-end';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, libraryText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  const templateId = await umbracoApi.template.createTemplateWithDisplayingBlockGridItems(gridTemplateName, gridCustomDataTypeName, propertyInBlock);
  const customDataTypeId = await umbracoApi.dataType.createBlockGridWithABlock(gridCustomDataTypeName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(gridDocumentTypeName, customDataTypeId, gridCustomDataTypeName, templateId);
  const documentId = await umbracoApi.document.createDefaultDocument(gridContentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(gridContentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'grid');
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();
  const contentURL = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(libraryText);
});

test('can render the referenced Library element content on the published page for a Single Block', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryText = 'Reusable single block content rendered on the front-end';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, libraryText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  const templateId = await umbracoApi.template.createTemplateWithDisplayingSingleBlockItem(singleTemplateName, singleCustomDataTypeName, propertyInBlock);
  const customDataTypeId = await umbracoApi.dataType.createSingleBlockDataTypeWithABlock(singleCustomDataTypeName, elementTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(singleDocumentTypeName, customDataTypeId, singleCustomDataTypeName, templateId);
  const documentId = await umbracoApi.document.createDefaultDocument(singleContentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(singleContentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'single');
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();
  const contentURL = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(libraryText);
});

test('can render the referenced Library element content on the published page for a Rich Text Editor', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryText = 'Reusable rich text block content rendered on the front-end';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, libraryText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  rteBlockPartialView = await umbracoApi.partialView.createRichTextBlockPartialView(elementTypeName, propertyInBlock);
  const templateId = await umbracoApi.template.createTemplateWithDisplayingRichTextValue(rteTemplateName, rteCustomDataTypeName);
  // A second block type keeps the toolbar's Insert Block on the catalogue rather than the create workspace.
  const secondElementTypeId = await umbracoApi.documentType.createEmptyElementType(rteSecondElementTypeName);
  const customDataTypeId = await umbracoApi.dataType.createRichTextEditorWithBlocks(rteCustomDataTypeName, [elementTypeId, secondElementTypeId]);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(rteDocumentTypeName, customDataTypeId, rteCustomDataTypeName, templateId);
  const documentId = await umbracoApi.document.createDefaultDocument(rteContentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(rteContentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'rte');
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();
  const contentURL = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(libraryText);
});

// Product bug (#24129): after a transfer to the Library, the rich text markup keeps the block's old
// data-content-key, so the block is not rendered on the published page.
test.skip('can render a Rich Text Editor block after transferring it to the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const blockText = 'Rich text block content transferred to the Library';
  rteBlockPartialView = await umbracoApi.partialView.createRichTextBlockPartialView(elementTypeName, propertyInBlock);
  const templateId = await umbracoApi.template.createTemplateWithDisplayingRichTextValue(rteTemplateName, rteCustomDataTypeName);
  const secondElementTypeId = await umbracoApi.documentType.createEmptyElementType(rteSecondElementTypeName);
  const customDataTypeId = await umbracoApi.dataType.createRichTextEditorWithBlocks(rteCustomDataTypeName, [elementTypeId, secondElementTypeId]);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(rteDocumentTypeName, customDataTypeId, rteCustomDataTypeName, templateId);
  const documentId = await umbracoApi.document.createDefaultDocument(rteContentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(rteContentName);
  await umbracoUi.content.clickInsertBlockButtonInRte();
  await umbracoUi.content.clickBlockElementWithName(elementTypeName);
  await umbracoUi.content.enterTextstring(blockText);
  await umbracoUi.content.clickCreateModalButton();

  // Act
  await umbracoUi.content.clickTransferToLibraryBlockButton('rte');
  await umbracoUi.content.transferBlockToLibraryRoot(rteTransferElementName);
  await umbracoUi.content.isBlockMarkedAsReference(true, 'rte');
  const transferredElement = await umbracoApi.element.getByName(rteTransferElementName);
  await umbracoApi.element.publish(transferredElement.id);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();
  const contentURL = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(blockText);
});

// Product bug (#24129): after a disconnect from the Library, the rich text markup keeps the block's old
// data-content-key, so the block is not rendered on the published page.
test.skip('can render a Rich Text Editor block after disconnecting it from the Library', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const libraryText = 'Rich text block content disconnected from the Library';
  const libraryElementId = await umbracoApi.element.createElementWithTextContent(libraryElementName, elementTypeId, libraryText, propertyInBlock);
  await umbracoApi.element.publish(libraryElementId);
  rteBlockPartialView = await umbracoApi.partialView.createRichTextBlockPartialView(elementTypeName, propertyInBlock);
  const templateId = await umbracoApi.template.createTemplateWithDisplayingRichTextValue(rteTemplateName, rteCustomDataTypeName);
  const secondElementTypeId = await umbracoApi.documentType.createEmptyElementType(rteSecondElementTypeName);
  const customDataTypeId = await umbracoApi.dataType.createRichTextEditorWithBlocks(rteCustomDataTypeName, [elementTypeId, secondElementTypeId]);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(rteDocumentTypeName, customDataTypeId, rteCustomDataTypeName, templateId);
  const documentId = await umbracoApi.document.createDefaultDocument(rteContentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(rteContentName);
  await umbracoUi.content.insertBlockFromLibraryWithName(libraryElementName, 'rte');
  await umbracoUi.content.clickSaveButtonAndWaitForContentToBeUpdated();

  // Act
  await umbracoUi.content.clickDisconnectFromLibraryBlockButton('rte');
  await umbracoUi.content.clickConfirmDisconnectFromLibraryButton('rte');
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();
  const contentURL = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(libraryText);
});
