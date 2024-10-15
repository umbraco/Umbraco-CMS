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
  await umbracoApi.template.ensureNameNotExists(templateName);
  const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeTemplatesTab();
  await umbracoUi.documentType.clickAddButton();
  await umbracoUi.documentType.clickLabelWithName(templateName);
  await umbracoUi.documentType.clickChooseButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedTemplates[0].id).toBe(templateId);

  // Clean
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can set an allowed template as default for document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.template.ensureNameNotExists(templateName);
  const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
  await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(documentTypeName, templateId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeTemplatesTab();
  await umbracoUi.documentType.clickDefaultTemplateButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedTemplates[0].id).toBe(templateId);
  expect(documentTypeData.defaultTemplate.id).toBe(templateId);
});

// When removing a template, the defaultTemplateId is set to "" which is not correct
test.skip('can remove an allowed template from a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.template.ensureNameNotExists(templateName);
  const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
  await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(documentTypeName, templateId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeTemplatesTab();
  await umbracoUi.documentType.clickRemoveWithName(templateName, true);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.isSuccessNotificationVisible();
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedTemplates).toHaveLength(0);
});
