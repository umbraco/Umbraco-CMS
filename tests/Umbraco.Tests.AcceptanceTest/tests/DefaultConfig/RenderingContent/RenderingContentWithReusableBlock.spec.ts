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
const propertyInBlock = 'Textstring';
const groupName = 'testGroup';
let elementTypeId = '';

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
});

test('renders the referenced Library element content on the published page', async ({umbracoApi, umbracoUi}) => {
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

test('renders the referenced Library element content on the published page for a Block Grid', async ({umbracoApi, umbracoUi}) => {
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
