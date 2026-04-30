import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

let documentTypeId = '';
let contentId = '';
const contentName = 'TestContent';
const renamedContentName = 'TestContentRenamed';
const documentTypeName = 'TestDocumentTypeForContentVersioning';
const dataTypeName = 'Textstring';
const originalText = 'Original version text';
const updatedText = 'Updated version text';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(renamedContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(renamedContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

// Arrange NEEDS TO BE MOVED
async function createContentWithTwoPublishedVersions(umbracoApi) {
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, originalText, dataTypeName);
  await umbracoApi.document.publish(contentId);
  const contentData = await umbracoApi.document.get(contentId);
  contentData.values[0].value = updatedText;
  await umbracoApi.document.update(contentId, contentData);
  await umbracoApi.document.publish(contentId);
}

test('can rollback content to a previous published version', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await createContentWithTwoPublishedVersions(umbracoApi);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, updatedText);

  // Act
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.clickContentTab();
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, originalText);
});

test('records a rollback entry in the audit trail', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await createContentWithTwoPublishedVersions(umbracoApi);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();
  await umbracoUi.content.isSuccessNotificationVisible();

  // Assert
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesHistoryItemHaveTag(ConstantHelper.auditTrailTypes.rollback);
  await umbracoUi.content.doesHistoryItemHaveDescription(ConstantHelper.auditTrailMessages.contentRolledBack);
  const currentUser = await umbracoApi.user.getCurrentUser();
  await umbracoUi.content.doesHistoryItemHaveUsername(currentUser.name);
});

test('can rollback to a previous version from the tree action menu', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await createContentWithTwoPublishedVersions(umbracoApi);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickRollbackActionMenuOption();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, originalText);
});

test('can rollback variant content to a previous published version', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const variantPublishSchedules = {publishSchedules: [{culture: 'en-US'}]};
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, originalText, dataTypeName);
  await umbracoApi.document.publish(contentId, variantPublishSchedules);
  const contentData = await umbracoApi.document.get(contentId);
  contentData.values[0].value = updatedText;
  await umbracoApi.document.update(contentId, contentData);
  await umbracoApi.document.publish(contentId, variantPublishSchedules);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.clickContentTab();
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, originalText);
});

test('rollback restores the document name to the previous version', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, originalText, dataTypeName);
  await umbracoApi.document.publish(contentId);
  const contentData = await umbracoApi.document.get(contentId);
  contentData.variants[0].name = renamedContentName;
  contentData.values[0].value = updatedText;
  await umbracoApi.document.update(contentId, contentData);
  await umbracoApi.document.publish(contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(renamedContentName);

  // Act
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  const rolledBackData = await umbracoApi.document.get(contentId);
  expect(rolledBackData.variants[0].name).toBe(contentName);
  expect(rolledBackData.values[0].value).toBe(originalText);
});

test('cancelling the rollback modal leaves the content unchanged', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await createContentWithTwoPublishedVersions(umbracoApi);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.medium);
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackCancelButton();

  // Assert
  await umbracoUi.content.clickContentTab();
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, updatedText);
  const currentData = await umbracoApi.document.get(contentId);
  expect(currentData.values[0].value).toBe(updatedText);
});
