import {test} from '@umbraco/playwright-testhelpers';

// Content
const contentName = 'Test Content';
const secondContentName = 'SecondContent';

// Document Type
const documentTypeName = 'DocumentTypeForContent';
const secondDocumentTypeName = 'TestSecondType';

// Template
const templateName = 'TestTemplateForContent';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(secondContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(secondDocumentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can get a sibling of a content item of a different content type using SiblingsOfType method', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(secondDocumentTypeName);
  const secondContentId = await umbracoApi.document.createDefaultDocument(secondContentName, secondDocumentTypeId);
  await umbracoApi.document.publish(secondContentId);
  const templateId = await umbracoApi.template.createTemplateUsingSiblingOfTypeMethod(templateName, secondDocumentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(documentTypeName, templateId, true);
  const contentId = await umbracoApi.document.createDocumentWithTemplate(contentName, documentTypeId, templateId);
  await umbracoApi.document.publish(contentId);
  const contentURL = await umbracoApi.document.getDocumentUrl(contentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentURL);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(secondContentName, true);
});