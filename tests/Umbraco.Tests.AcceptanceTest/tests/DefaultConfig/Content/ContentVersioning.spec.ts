import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

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

test('can rollback content to a previous published version', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createPublishedDocumentWithTwoTextVersions(contentName, documentTypeName, dataTypeName, originalText, updatedText);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.content.waitForRollbackItems();
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.clickContentTab();
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, originalText);
});

test('records a rollback entry in the audit trail', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createPublishedDocumentWithTwoTextVersions(contentName, documentTypeName, dataTypeName, originalText, updatedText);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.content.waitForRollbackItems();
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
  await umbracoApi.document.createPublishedDocumentWithTwoTextVersions(contentName, documentTypeName, dataTypeName, originalText, updatedText);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickRollbackActionMenuOption();
  await umbracoUi.content.waitForRollbackItems();
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, originalText);
});

test('can rollback content with a culture variant to a previous published version', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  const documentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, originalText, dataTypeName);
  await umbracoApi.document.publishDocumentWithCulture(documentId, 'en-US');
  const documentData = await umbracoApi.document.get(documentId);
  documentData.values[0].value = updatedText;
  await umbracoApi.document.update(documentId, documentData);
  await umbracoApi.document.publishDocumentWithCulture(documentId, 'en-US');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.content.waitForRollbackItems();
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.clickContentTab();
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, originalText);
  await umbracoApi.document.verifyDocumentValueForCulture(documentId, originalText);
});

test('rollback restores the document name to the previous version', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
  const documentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, originalText, dataTypeName);
  await umbracoApi.document.publish(documentId);
  const contentData = await umbracoApi.document.get(documentId);
  contentData.variants[0].name = renamedContentName;
  contentData.values[0].value = updatedText;
  await umbracoApi.document.update(documentId, contentData);
  await umbracoApi.document.publish(documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(renamedContentName);

  // Act
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.content.waitForRollbackItems();
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackContainerButton();

  // Assert
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoApi.document.verifyDocumentNameForCulture(documentId, contentName);
  await umbracoApi.document.verifyDocumentValueForCulture(documentId, originalText);
});

test('cancelling the rollback modal leaves the content unchanged', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentId = await umbracoApi.document.createPublishedDocumentWithTwoTextVersions(contentName, documentTypeName, dataTypeName, originalText, updatedText);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.clickRollbackButton();
  await umbracoUi.content.waitForRollbackItems();
  await umbracoUi.content.clickLatestRollBackItem();
  await umbracoUi.content.clickRollbackCancelButton();

  // Assert
  await umbracoUi.content.clickContentTab();
  await umbracoUi.content.doesDocumentPropertyHaveValue(dataTypeName, updatedText);
  await umbracoApi.document.verifyDocumentValueForCulture(documentId, updatedText);
});
