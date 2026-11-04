import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const webhookName = 'LockedWebhook';

test('blocks every change to a webhook in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();
  await umbracoUi.webhook.goToSettingsTreeItem('Webhooks');

  await test.step('Collection', async () => {
    await umbracoUi.webhook.isWebhookNameVisible(webhookName);
    await umbracoUi.webhook.isCollectionActionWithNameVisible('Create', false);
    await umbracoUi.webhook.isEntityActionVisibleInCollectionRowForName(webhookName, 'Delete', false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.webhook.clickTextButtonWithName(webhookName);
    await umbracoUi.webhook.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.webhook.isSaveButtonVisible(false);
    await umbracoUi.webhook.isWorkspaceNameEditable(false);
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/webhook/create`);
    await umbracoUi.webhook.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a webhook through the Management API', async ({umbracoApi}) => {
  // Arrange
  const webhook = await umbracoApi.webhook.getByName(webhookName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.webhook}`;

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${webhook.id}`, webhook);
  const deleteResponse = await umbracoApi.delete(`${url}/${webhook.id}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('webhook:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('webhook:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('webhook:delete');
});
