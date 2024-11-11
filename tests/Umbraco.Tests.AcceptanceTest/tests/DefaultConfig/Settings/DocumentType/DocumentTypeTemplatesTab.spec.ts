import {ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

const documentTypeName = 'TestDocumentType';
const templateName = 'TestTemplate';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can add an allowed template to a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeTemplatesTab();
  await umbracoUi.documentType.clickChooseButton();
  await umbracoUi.documentType.clickLabelWithName(templateName);
  await umbracoUi.documentType.clickChooseModalButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedTemplates[0].id).toBe(templateId);

  // Clean
  await umbracoApi.template.ensureNameNotExists(templateName);
});

// TODO: Need to uodate Act steps
test.skip('can set an allowed template as default for document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondTemplateName = 'Test Second Template';
  const firstTemplateId = await umbracoApi.template.createDefaultTemplate(templateName);
  const secondTemplateId = await umbracoApi.template.createDefaultTemplate(secondTemplateName);
  await umbracoApi.documentType.createDocumentTypeWithTwoAllowedTemplates(documentTypeName, firstTemplateId, secondTemplateId, true, firstTemplateId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeTemplatesTab();
  await umbracoUi.documentType.clickSetAsDefaultButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.defaultTemplate.id).toBe(secondTemplateId);
});

// When removing a template, the defaultTemplateId is set to "" which is not correct
test.skip('can remove an allowed template from a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
  await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(documentTypeName, templateId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeTemplatesTab();
  await umbracoUi.documentType.clickRemoveWithName(templateName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedTemplates).toHaveLength(0);
});
