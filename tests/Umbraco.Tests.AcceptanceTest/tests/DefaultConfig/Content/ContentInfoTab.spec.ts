import { expect } from '@playwright/test';
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
  const notPublishContentLink = 'This document is published but is not in the cache';
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.openContent(contentName);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesLinkHaveText(notPublishContentLink);
  await umbracoUi.content.clickSaveAndPublishButton();
  
  // Assert
  const contentData = await umbracoApi.document.get(contentId);
  // TODO: Uncomment this when front-end is ready. Currently the link is not updated immediately after publishing
  //await umbracoUi.content.doesLinkHaveText(contentData.urls[0].url);
  await umbracoUi.content.doesIdHaveText(contentData.id);
  await umbracoUi.content.doesPublicationStatusHaveText(contentData.variants[0].state === 'Draft' ? 'Unpublished' : contentData.variants[0].state);
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
});

// TODO: Remove skip when the frond-end is ready. Currently the document type is not opened after clicking to the button
test.skip('can open document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.openContent(contentName);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickDocumentTypeByName(documentTypeName);

  // Assert
  await umbracoUi.content.isDocumentTypeModalVisible(documentTypeName);
});

test('can open template', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const templateName = "TestTemplateForContent";
  await umbracoApi.template.ensureNameNotExists(templateName);
  const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(documentTypeName, templateId, true);
  contentId = await umbracoApi.document.createDocumentWithTemplate(contentName, documentTypeId, templateId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  
  // Act
  await umbracoUi.content.openContent(contentName);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickTemplateByName(templateName);

  // Assert
  await umbracoUi.content.isTemplateModalVisible(templateName);

  // Clean
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can change template', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstTemplateName = "TestTemplateOneForContent";
  const secondTemplateName = "TestTemplateTwoForContent";
  await umbracoApi.template.ensureNameNotExists(firstTemplateName);
  await umbracoApi.template.ensureNameNotExists(secondTemplateName);
  const firstTemplateId = await umbracoApi.template.createDefaultTemplate(firstTemplateName);
  const secondTemplateId = await umbracoApi.template.createDefaultTemplate(secondTemplateName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithTwoAllowedTemplates(documentTypeName, firstTemplateId, secondTemplateId, true);
  contentId = await umbracoApi.document.createDocumentWithTemplate(contentName, documentTypeId, firstTemplateId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.openContent(contentName);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.changeTemplate(firstTemplateName, secondTemplateName);
  await umbracoUi.content.clickSaveButton();

  // Assert
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.template.id).toBe(secondTemplateId);

  // Clean
  await umbracoApi.template.ensureNameNotExists(firstTemplateName);
  await umbracoApi.template.ensureNameNotExists(secondTemplateName);
});

test('cannot change to a template that is not allowed in the document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstTemplateName = "TestTemplateOneForContent";
  const secondTemplateName = "TestTemplateTwoForContent";
  await umbracoApi.template.ensureNameNotExists(firstTemplateName);
  await umbracoApi.template.ensureNameNotExists(secondTemplateName);
  const firstTemplateId = await umbracoApi.template.createDefaultTemplate(firstTemplateName);
  await umbracoApi.template.createDefaultTemplate(secondTemplateName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(documentTypeName, firstTemplateId, true);
  contentId = await umbracoApi.document.createDocumentWithTemplate(contentName, documentTypeId, firstTemplateId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.openContent(contentName);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickEditTemplateByName(firstTemplateName);

  // Assert
  // This wait is needed to make sure the template name is visible when the modal is opened
  await umbracoUi.waitForTimeout(1000);
  await umbracoUi.content.isTemplateNameDisabled(secondTemplateName);

  // Clean
  await umbracoApi.template.ensureNameNotExists(firstTemplateName);
  await umbracoApi.template.ensureNameNotExists(secondTemplateName);
});
