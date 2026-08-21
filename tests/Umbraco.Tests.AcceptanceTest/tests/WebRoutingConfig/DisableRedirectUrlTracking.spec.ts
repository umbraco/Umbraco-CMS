import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

let documentTypeId = '';
const contentName = 'TestContentRedirectURL';
const updatedContentName = 'UpdatedContentName';
const documentTypeName = 'TestDocumentType';
const rootDocumentName = 'RootDocument';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  await umbracoUi.goToBackOffice();
  // Create a published root document
  const rootDocumentId = await umbracoApi.document.createDefaultDocument(rootDocumentName, documentTypeId);
  await umbracoApi.document.publish(rootDocumentId);
  // Create a published content
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.publish(contentId);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(rootDocumentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can see that the URL tracker is disabled', async ({umbracoUi}) => {
  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();

  // Assert
  await umbracoUi.redirectManagement.doesTrackerStatusHaveText(ConstantHelper.redirectUrlTrackerMessages.disabled);
});

test('cannot enable the URL tracker from the dashboard', async ({umbracoUi}) => {
  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.clickTrackerStatusButton();

  // Assert
  await umbracoUi.redirectManagement.doesUrlTrackerInfoContainText(ConstantHelper.redirectUrlTrackerMessages.enableInstruction);
  await umbracoUi.redirectManagement.doesUrlTrackerInfoContainText(ConstantHelper.redirectUrlTrackerMessages.configurationKey);
  await umbracoUi.redirectManagement.closeUrlTrackerInfo();
  await umbracoUi.redirectManagement.doesTrackerStatusHaveText(ConstantHelper.redirectUrlTrackerMessages.disabled);
});

test('cannot see redirects when the URL tracker is disabled', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);

  // Act
  await umbracoUi.content.enterContentName(updatedContentName);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, true, true);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.isTextWithMessageVisible(ConstantHelper.redirectUrlTrackerMessages.noRedirectsHaveBeenMade);
  await umbracoUi.redirectManagement.doesRedirectManagementRowsHaveCount(0);
});