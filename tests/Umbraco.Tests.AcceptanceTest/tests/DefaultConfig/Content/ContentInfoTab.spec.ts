import {expect} from '@playwright/test';
import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

let documentTypeId = '';
let contentId = '';
const contentName = 'TestInfoTab';
const documentTypeName = 'TestDocumentTypeForContent';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can see correct information when published', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const notPublishContentLink = 'This item is not published';
  const dataTypeName = 'Textstring';
  const contentText = 'This is test content text';
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentHaveLink(notPublishContentLink);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  await umbracoUi.content.doesIdHaveText(contentData.id);
  const expectedCreatedDate = new Date(contentData.variants[0].createDate).toLocaleString("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "numeric",
    minute: "numeric",
    second: "numeric",
    hour12: true,
  });
  await umbracoUi.content.doesCreatedDateHaveText(expectedCreatedDate);
  await umbracoUi.content.doesDocumentHaveLink(contentData.urls[0].url ? contentData.urls[0].url : '/');
  // TODO: Uncomment this when front-end is ready. Currently the publication status of content is not changed to "Published" immediately after publishing it
  //await umbracoUi.content.doesPublicationStatusHaveText(contentData.variants[0].state === 'Draft' ? 'Unpublished' : contentData.variants[0].state);
});

test('can open document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickDocumentTypeByName(documentTypeName);

  // Assert
  await umbracoUi.content.isDocumentTypeModalVisible(documentTypeName);
});

test('can open template', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const templateName = 'TestTemplateForContent';
  await umbracoApi.template.ensureNameNotExists(templateName);
  const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(documentTypeName, templateId, true);
  contentId = await umbracoApi.document.createDocumentWithTemplate(contentName, documentTypeId, templateId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickTemplateByName(templateName);

  // Assert
  await umbracoUi.content.isTemplateModalVisible(templateName);

  // Clean
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can change template', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstTemplateName = 'TestTemplateOneForContent';
  const secondTemplateName = 'TestTemplateTwoForContent';
  await umbracoApi.template.ensureNameNotExists(firstTemplateName);
  await umbracoApi.template.ensureNameNotExists(secondTemplateName);
  const firstTemplateId = await umbracoApi.template.createDefaultTemplate(firstTemplateName);
  const secondTemplateId = await umbracoApi.template.createDefaultTemplate(secondTemplateName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithTwoAllowedTemplates(documentTypeName, firstTemplateId, secondTemplateId, true);
  contentId = await umbracoApi.document.createDocumentWithTemplate(contentName, documentTypeId, firstTemplateId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.changeTemplate(firstTemplateName, secondTemplateName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  //await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.isErrorNotificationVisible(false);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.template.id).toBe(secondTemplateId);

  // Clean
  await umbracoApi.template.ensureNameNotExists(firstTemplateName);
  await umbracoApi.template.ensureNameNotExists(secondTemplateName);
});

test('cannot change to a template that is not allowed in the document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstTemplateName = 'TestTemplateOneForContent';
  const secondTemplateName = 'TestTemplateTwoForContent';
  await umbracoApi.template.ensureNameNotExists(firstTemplateName);
  await umbracoApi.template.ensureNameNotExists(secondTemplateName);
  const firstTemplateId = await umbracoApi.template.createDefaultTemplate(firstTemplateName);
  await umbracoApi.template.createDefaultTemplate(secondTemplateName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(documentTypeName, firstTemplateId, true);
  contentId = await umbracoApi.document.createDocumentWithTemplate(contentName, documentTypeId, firstTemplateId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickEditTemplateByName(firstTemplateName);

  // Assert
  await umbracoUi.content.isTemplateNameDisabled(secondTemplateName);

  // Clean
  await umbracoApi.template.ensureNameNotExists(firstTemplateName);
  await umbracoApi.template.ensureNameNotExists(secondTemplateName);
});
