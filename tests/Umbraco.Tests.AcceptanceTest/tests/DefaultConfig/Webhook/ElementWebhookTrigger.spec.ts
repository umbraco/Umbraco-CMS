import {expect} from "@playwright/test";
import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// Webhook
const webhookName = 'Test Element Webhook';
let webhookSiteToken = '';

// Element
const elementName = 'Test Webhook Element';
const elementTypeName = 'TestElementTypeForWebhook';
const dataTypeName = 'Textstring';
let elementTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.webhook.ensureNameNotExists(webhookName);
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  webhookSiteToken = await umbracoApi.webhook.generateWebhookSiteToken();
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataTypeData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.webhook.ensureNameNotExists(webhookName);
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.element.emptyRecycleBin();
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can trigger when element is published', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Element Published';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event);
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBePublished();

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken, undefined, undefined, elementId);
  expect(webhookSiteData[0].content).toContain(elementId);
});

test('can trigger when element is unpublished', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Element Unpublished';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event);
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.publish(elementId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickUnpublishActionMenuOption();
  await umbracoUi.library.clickConfirmToUnpublishButton();
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.elementUnpublished);

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken, undefined, undefined, elementId);
  expect(webhookSiteData[0].content).toContain(elementId);
});

test('can trigger when element is deleted', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Element Deleted';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event);
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.moveToRecycleBin(elementId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.isItemVisibleInRecycleBin(elementName);

  // Act
  await umbracoUi.library.clickDeleteButtonForTrashedElememtWithName(elementName);
  await umbracoUi.library.clickConfirmToDeleteButtonAndWaitForElementToBeDeleted();

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken, undefined, undefined, elementId);
  expect(webhookSiteData[0].content).toContain(elementId);
});

test('can trigger the webhook for a specific element type', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Element Published';
  const secondElementName = 'Second Test Webhook Element';
  const secondElementTypeName = 'SecondTestElementTypeForWebhook';
  await umbracoApi.element.ensureNameNotExists(secondElementName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const secondElementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(secondElementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataTypeData.id);
  await umbracoApi.webhook.createWebhookForSpecificContentType(webhookName, webhookSiteToken, event, elementTypeName);
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  const secondElementId = await umbracoApi.element.createDefaultElement(secondElementName, secondElementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(secondElementName);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBePublished();
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBePublished();

  // Assert
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken, undefined, undefined, elementId);
  expect(webhookSiteData[0].content).toContain(elementId);
  const allRequests = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken);
  for (const request of allRequests) {
    expect(request.content).not.toContain(secondElementId);
  }

  // Clean
  await umbracoApi.element.ensureNameNotExists(secondElementName);
  await umbracoApi.documentType.ensureNameNotExists(secondElementTypeName);
});

test('cannot trigger element event when the webhook is disabled', async ({umbracoApi, umbracoUi}) => {
  test.slow();

  // Arrange
  const event = 'Element Published';
  await umbracoApi.webhook.createDefaultWebhook(webhookName, webhookSiteToken, event, false);
  await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBePublished();

  // Assert
  // Longer than the webhook firing period, so an enabled webhook would have been delivered.
  const webhookSiteData = await umbracoApi.webhook.getWebhookSiteRequestResponse(webhookSiteToken, 15000);
  expect(webhookSiteData).toBeFalsy();
});
