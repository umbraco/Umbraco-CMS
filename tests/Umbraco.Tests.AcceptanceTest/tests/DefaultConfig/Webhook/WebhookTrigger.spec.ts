import {expect} from "@playwright/test";
import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';

// Webhook
const webhookName = 'Test Webhook';
let webhookSiteToken = '';

// Content
const documentName = 'Test Webhook Content';
const documentTypeName = 'TestDocumentTypeForWebhook';
let documentTypeId = '';

// Media
const mediaName = 'Test Webhook Media';
const mediaTypeName = 'Image';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.webhook.ensureNameNotExists(webhookName);
  webhookSiteToken = await umbracoApi.webhook.generateWebhookSiteToken();
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.webhook.ensureNameNotExists(webhookName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test('can trigger when content is published', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Content Published';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event);
  await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(documentName);
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken);
  expect(webhookSiteData[0].content).toContain(documentName);
});

test('can trigger when content is deleted', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Content Deleted';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event);
  const contentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.document.moveToRecycleBin(contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickEmptyRecycleBinButton();
  await umbracoUi.content.clickConfirmEmptyRecycleBinButtonAndWaitForRecycleBinToBeEmptied();

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken);
  expect(webhookSiteData[0].content).toContain(contentId);
});

test('can trigger when content is unpublished', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Content Unpublished';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event);
  const contentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
  await umbracoApi.document.publish(contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(documentName);
  await umbracoUi.content.clickUnpublishActionMenuOption();
  await umbracoUi.content.clickConfirmToUnpublishButton();
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.unpublished);

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken);
  expect(webhookSiteData[0].content).toContain(contentId);
});

test('can trigger when media is saved', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Media Saved';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event);
  const mediaId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.goToMediaWithName(mediaName);
  await umbracoUi.media.clickSaveButtonAndWaitForMediaToBeUpdated();

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken);
  expect(webhookSiteData[0].content).toContain(mediaName);
  expect(webhookSiteData[0].content).toContain(mediaId);
});

test('can trigger when media is deleted', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Media Deleted';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event);
  const mediaId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  await umbracoApi.media.trashMediaItem(mediaName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaName, true, true);
  await umbracoUi.media.deleteMediaItemAndWaitForMediaToBeDeleted(mediaName);

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken);
  expect(webhookSiteData[0].content).toContain(mediaId);
});

test('can trigger the webhook for a specific media type', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Media Deleted';
  const secondMediaName = 'Test Second Media';
  await umbracoApi.webhook.createWebhookForSpecificContentType(webhookName, webhookSiteToken, event, mediaTypeName);
  const mediaId = await umbracoApi.media.createDefaultMediaWithImage(mediaName);
  const secondMediaId = await umbracoApi.media.createDefaultMediaWithArticle(secondMediaName);
  await umbracoApi.media.trashMediaItem(mediaName);
  await umbracoApi.media.trashMediaItem(secondMediaName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.media);

  // Act
  await umbracoUi.media.isItemVisibleInRecycleBin(mediaName, true, true);
  await umbracoUi.media.deleteMediaItem(mediaName);
  await umbracoUi.media.deleteMediaItem(secondMediaName);

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken);
  expect(webhookSiteData[0].content).toContain(mediaId);
  expect(webhookSiteData[0].content).not.toContain(secondMediaId);

  // Clean
  await umbracoApi.media.ensureNameNotExists(secondMediaName);
});

test('can trigger the webhook for a specific content type', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Content Published';
  const secondDocumentName = 'Second Test Webhook Content';
  const secondDocumentTypeName = 'SecondTestDocumentTypeForWebhook';
  const secondDocumentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(secondDocumentTypeName);
  await umbracoApi.document.createDefaultDocument(secondDocumentName, secondDocumentTypeId);
  await umbracoApi.webhook.createWebhookForSpecificContentType(webhookName, webhookSiteToken, event, documentTypeName);
  await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(documentName);
  await umbracoUi.content.clickPublishActionMenuOption();
  await umbracoUi.content.clickConfirmToPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
  await umbracoUi.content.clickActionsMenuForContent(secondDocumentName);
  await umbracoUi.content.clickPublishActionMenuOption();
  await umbracoUi.content.clickConfirmToPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken);
  expect(webhookSiteData[0].content).toContain(documentName);
  expect(webhookSiteData[0].content).not.toContain(secondDocumentName);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(secondDocumentTypeName);
  await umbracoApi.document.ensureNameNotExists(secondDocumentName);
});

test('cannot trigger when the webhook is disabled', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Content Published';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event, false);
  await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(documentName);
  await umbracoUi.content.clickPublishActionMenuOption();
  await umbracoUi.content.clickConfirmToPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken, 7000);
  expect(webhookSiteData).toBeFalsy();
});

test('can custom header for the webhook request', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Content Published';
  const headerName = 'test-header-name';
  const headerValue = 'automation-test';
  await umbracoApi.webhook.createWebhookWithHeader(webhookName, webhookSiteToken, event, headerName, headerValue);
  await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);

  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(documentName);
  await umbracoUi.content.clickPublishActionMenuOption();
  await umbracoUi.content.clickConfirmToPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken);
  expect(webhookSiteData[0].headers[headerName]).toEqual([headerValue]);
});
