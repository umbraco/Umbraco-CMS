import {expect} from "@playwright/test";
import {test} from '@umbraco/playwright-testhelpers';

const webhookName = 'Test Webhook';
let webhookSiteToken = '';

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.webhook.ensureNameNotExists(webhookName);
  await umbracoUi.goToBackOffice();
  webhookSiteToken = await umbracoApi.webhook.generateWebhookSiteToken();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.webhook.ensureNameNotExists(webhookName);
});

test('can create a webhook', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const event = 'Content Deleted';
  const webhookSiteUrl = umbracoApi.webhook.webhookSiteUrl + webhookSiteToken;
  await umbracoUi.webhook.goToWebhooks();

  // Act
  await umbracoUi.webhook.clickWebhookCreateButton();
  await umbracoUi.webhook.enterWebhookName(webhookName);
  await umbracoUi.webhook.enterUrl(webhookSiteUrl);
  await umbracoUi.webhook.clickChooseEventButton();
  await umbracoUi.webhook.clickTextButtonWithName(event);
  await umbracoUi.webhook.clickSubmitButton();
  await umbracoUi.webhook.clickSaveButtonAndWaitForWebhookToBeCreated();

  // Assert
  expect(await umbracoApi.webhook.doesNameExist(webhookName)).toBeTruthy();
  expect(await umbracoApi.webhook.doesWebhookHaveUrl(webhookName, webhookSiteUrl)).toBeTruthy();
  expect(await umbracoApi.webhook.doesWebhookHaveEvent(webhookName, event)).toBeTruthy();
  await umbracoApi.webhook.isWebhookEnabled(webhookName);
});

test('can update webhook name', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedName = 'Updated Webhook';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken);
  await umbracoUi.webhook.goToWebhookWithName(webhookName);

  // Act
  await umbracoUi.webhook.enterWebhookName(updatedName);
  await umbracoUi.webhook.clickSaveButtonAndWaitForWebhookToBeUpdated();

  // Assert
  expect(await umbracoApi.webhook.doesNameExist(updatedName)).toBeTruthy();
  expect(await umbracoApi.webhook.doesNameExist(webhookName)).toBeFalsy();
});

test('can delete a webhook', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken);
  await umbracoUi.webhook.goToWebhooks();

  // Act
  await umbracoUi.webhook.clickDeleteWebhookWithName(webhookName);
  await umbracoUi.webhook.clickConfirmToDeleteButton();

  // Assert
  expect(await umbracoApi.webhook.doesNameExist(webhookName)).toBeFalsy();
});

test('can add content type for a webhook', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const contentTypeName = 'Test Document Type For Webhook';
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentType(contentTypeName);
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken);
  await umbracoUi.webhook.goToWebhookWithName(webhookName);

  // Act
  await umbracoUi.webhook.clickChooseContentTypeButton();
  await umbracoUi.webhook.clickModalMenuItemWithName(contentTypeName);
  await umbracoUi.webhook.clickChooseModalButton();
  await umbracoUi.webhook.clickSaveButtonAndWaitForWebhookToBeUpdated();

  // Assert
  expect(await umbracoApi.webhook.doesNameExist(webhookName)).toBeTruthy();
  expect(await umbracoApi.webhook.doesWebhookHaveContentTypeId(webhookName, documentTypeId)).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(contentTypeName);
});

test('can add header for a webhook', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const headerName = 'test-header-name';
  const headerValue = 'test-header-value';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken);
  await umbracoUi.webhook.goToWebhookWithName(webhookName);

  // Act
  await umbracoUi.webhook.clickAddHeadersButton();
  await umbracoUi.webhook.enterHeaderName(headerName);
  await umbracoUi.webhook.enterHeaderValue(headerValue);
  await umbracoUi.webhook.clickSaveButtonAndWaitForWebhookToBeUpdated();

  // Assert
  expect(await umbracoApi.webhook.doesNameExist(webhookName)).toBeTruthy();
  expect(await umbracoApi.webhook.doesWebhookHaveHeader(webhookName, headerName, headerValue)).toBeTruthy();
});

test('can disable a webhook', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken);
  await umbracoUi.webhook.goToWebhookWithName(webhookName);

  // Act
  await umbracoUi.webhook.clickEnabledToggleButton();
  await umbracoUi.webhook.clickSaveButtonAndWaitForWebhookToBeUpdated();

  // Assert
  expect(await umbracoApi.webhook.doesNameExist(webhookName)).toBeTruthy();
  await umbracoApi.webhook.isWebhookEnabled(webhookName, false);
});

test('cannot remove all events from a webhook', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const event = 'Content Deleted';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event);
  await umbracoUi.webhook.goToWebhookWithName(webhookName);

  // Act
  await umbracoUi.webhook.clickRemoveButtonForName(event);
  await umbracoUi.webhook.clickSaveButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible();
});

test('can remove a content type from a webhook', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const event = 'Media Saved';
  const mediaTypeName = 'Audio';
  const mediaTypeData = await umbracoApi.mediaType.getByName(mediaTypeName);
  await umbracoApi.webhook.createWebhookForSpecificContentType(webhookName, webhookSiteToken, event, mediaTypeName);
  expect(await umbracoApi.webhook.doesWebhookHaveContentTypeId(webhookName, mediaTypeData.id)).toBeTruthy();
  await umbracoUi.webhook.goToWebhookWithName(webhookName);

  // Act
  await umbracoUi.webhook.clickRemoveButtonForName(mediaTypeName);
  await umbracoUi.webhook.clickConfirmRemoveButton();
  await umbracoUi.webhook.clickSaveButtonAndWaitForWebhookToBeUpdated();

  // Assert
  expect(await umbracoApi.webhook.doesWebhookHaveContentTypeId(webhookName, mediaTypeData.id)).toBeFalsy();
});

test('can remove a header from a webhook', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const event = 'Content Published';
  const headerName = 'test-header-name';
  const headerValue = 'automation-test';
  await umbracoApi.webhook.createWebhookWithHeader(webhookName, webhookSiteToken, event, headerName, headerValue);
  expect(await umbracoApi.webhook.doesWebhookHaveHeader(webhookName, headerName, headerValue)).toBeTruthy();
  await umbracoUi.webhook.goToWebhookWithName(webhookName);

  // Act
  await umbracoUi.webhook.clickHeaderRemoveButton();
  await umbracoUi.webhook.clickSaveButtonAndWaitForWebhookToBeUpdated();

  // Assert
  expect(await umbracoApi.webhook.doesWebhookHaveHeader(webhookName, headerName, headerValue)).toBeFalsy();
});

test('cannot add both content event and media event for a webhook', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const event = 'Content Published';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event);
  await umbracoUi.webhook.goToWebhookWithName(webhookName);

  // Act
  await umbracoUi.webhook.clickChooseEventButton();

  // Assert
  await umbracoUi.webhook.isModalMenuItemWithNameDisabled('Media Saved');
  await umbracoUi.webhook.isModalMenuItemWithNameDisabled('Media Deleted');
});
