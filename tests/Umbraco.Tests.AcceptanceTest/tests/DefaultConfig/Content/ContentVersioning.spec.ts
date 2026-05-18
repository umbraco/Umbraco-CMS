import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const contentName = 'TestContent';
const renamedContentName = 'TestContentRenamed';
const documentTypeName = 'TestDocumentTypeForContentVersioning';
const dataTypeName = 'Textstring';
const originalText = 'Original version text';
const updatedText = 'Updated version text';

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(renamedContentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can rollback content to a previous published version', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createPublishedDocumentWithTwoNameVersionsAndTwoTextVersions(contentName, contentName, documentTypeName, dataTypeName, originalText, updatedText);
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
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesHistoryItemHaveTag(ConstantHelper.auditTrailTypes.rollback);
  await umbracoUi.content.doesHistoryItemHaveDescription(ConstantHelper.auditTrailMessages.contentRolledBack);
  const currentUser = await umbracoApi.user.getCurrentUser();
  await umbracoUi.content.doesHistoryItemHaveUsername(currentUser.name);
});

test('can rollback to a previous version from the tree action menu', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createPublishedDocumentWithTwoNameVersionsAndTwoTextVersions(contentName, contentName, documentTypeName, dataTypeName, originalText, updatedText);
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

test('can rollback a variant document to a previous published version', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', true, true);
  const documentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, originalText, dataTypeName, true);
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
  await umbracoApi.document.doesDocumentWithCultureHaveValue(documentId, originalText, 'en-US');
});

test('rollback restores the document name to the previous version', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentId = await umbracoApi.document.createPublishedDocumentWithTwoNameVersionsAndTwoTextVersions(contentName, renamedContentName, documentTypeName, dataTypeName, originalText, updatedText);
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
  await umbracoUi.content.doesDocumentHaveName(contentName);
  await umbracoApi.document.doesDocumentWithCultureHaveName(documentId, contentName, null);
  await umbracoApi.document.doesDocumentWithCultureHaveValue(documentId, originalText, null);
});

test('cancelling the rollback modal leaves the content unchanged', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentId = await umbracoApi.document.createPublishedDocumentWithTwoNameVersionsAndTwoTextVersions(contentName, contentName, documentTypeName, dataTypeName, originalText, updatedText);
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
  await umbracoApi.document.doesDocumentWithCultureHaveValue(documentId, updatedText, null);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesHistoryItemHaveTag(ConstantHelper.auditTrailTypes.publish);
  await umbracoUi.content.doesHistoryItemHaveDescription(ConstantHelper.auditTrailMessages.contentSavedAndPublished);
});
