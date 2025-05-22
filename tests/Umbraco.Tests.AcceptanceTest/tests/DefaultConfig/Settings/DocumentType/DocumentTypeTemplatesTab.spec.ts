import {ConstantHelper, test} from "@umbraco/playwright-testhelpers";
import {expect} from "@playwright/test";

const documentTypeName = 'TestDocumentType';
const templateName = 'TestTemplate';
let templateId = '';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  templateId = await umbracoApi.template.createDefaultTemplate(templateName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test('can add an allowed template to a document type', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDefaultDocumentType(documentTypeName);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeTemplatesTab();
  await umbracoUi.documentType.clickChooseButton();
  await umbracoUi.documentType.clickLabelWithName(templateName);
  await umbracoUi.documentType.clickChooseModalButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.isSuccessNotificationVisible();
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedTemplates[0].id).toBe(templateId);
});

test('can set an allowed template as default for document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondTemplateName = 'SecondTemplateName';
  await umbracoApi.template.ensureNameNotExists(secondTemplateName);
  const secondTemplateId = await umbracoApi.template.createDefaultTemplate(secondTemplateName);
  await umbracoApi.documentType.createDocumentTypeWithTwoAllowedTemplates(documentTypeName, templateId, secondTemplateId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeTemplatesTab();
  await umbracoUi.documentType.clickSetAsDefaultButton();
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.isSuccessNotificationVisible();
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedTemplates).toHaveLength(2);
  expect(documentTypeData.defaultTemplate.id).toBe(secondTemplateId);

  // Clean
  await umbracoApi.template.ensureNameNotExists(secondTemplateName);
});

// TODO: Remove skip when the front-end is ready. Currently the error displays when remove an allowed template
test.skip('can remove an allowed template from a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createDocumentTypeWithAllowedTemplate(documentTypeName, templateId);
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.goToDocumentType(documentTypeName);
  await umbracoUi.documentType.clickDocumentTypeTemplatesTab();
  await umbracoUi.documentType.clickRemoveWithName(templateName, true);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  //await umbracoUi.documentType.isSuccessNotificationVisible();
  await umbracoUi.documentType.isErrorNotificationVisible(false);
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.allowedTemplates).toHaveLength(0);
});
