import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const documentTypeName = 'TestDocumentType';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create a document type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.clickDocumentTypeMenu();
  await umbracoUi.documentType.clickCreateActionWithOptionName('Document Types');
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  await umbracoUi.documentType.reloadTree('Document Types');
  await umbracoUi.documentType.isDocumentTreeItemVisible(documentTypeName);
});

test('can create a document type with a template', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.clickDocumentTypeMenu();
  await umbracoUi.documentType.clickCreateActionWithOptionName('Document Type with Template');
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  // Checks if both the success notification for document Types and the template are visible
  await umbracoUi.documentType.doesSuccessNotificationsHaveCount(2);
  // Checks if the documentType contains the template
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  const templateData = await umbracoApi.template.getByName(documentTypeName);
  expect(documentTypeData.allowedTemplates[0].id).toEqual(templateData.id);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();

  // Clean
  await umbracoApi.template.ensureNameNotExists(documentTypeName);
});

test('can create a element type', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.documentType.goToSection(ConstantHelper.sections.settings);

  // Act
  await umbracoUi.documentType.clickDocumentTypeMenu();
  await umbracoUi.documentType.clickCreateActionWithOptionName('Element Type');
  await umbracoUi.documentType.enterDocumentTypeName(documentTypeName);
  await umbracoUi.documentType.clickSaveButton();

  // Assert
  await umbracoUi.documentType.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.documentType.doesNameExist(documentTypeName)).toBeTruthy();
  // Checks if the isElement is true
  const documentTypeData = await umbracoApi.documentType.getByName(documentTypeName);
  expect(documentTypeData.isElement).toBeTruthy();
});
