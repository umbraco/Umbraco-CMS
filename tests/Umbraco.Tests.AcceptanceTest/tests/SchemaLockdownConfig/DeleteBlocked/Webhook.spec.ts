import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const webhookName = 'LockedWebhook';

test('blocks deleting a webhook in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.webhook.goToSettingsTreeItem('Webhooks');

  await test.step('Collection', async () => {
    await umbracoUi.webhook.isCollectionActionWithNameVisible('Create');
    await umbracoUi.webhook.isEntityActionVisibleInCollectionRowForName(webhookName, 'Delete', false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.webhook.clickTextButtonWithName(webhookName);
    await umbracoUi.webhook.isSaveButtonVisible();
    await umbracoUi.webhook.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.webhook.isWorkspaceNameEditable();
  });
});

test('blocks deleting a webhook through the Management API', async ({umbracoApi}) => {
  // Arrange
  const webhook = await umbracoApi.webhook.getByName(webhookName);

  // Act
  const response = await umbracoApi.delete(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.webhook}/${webhook.id}`);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('webhook:delete');
});
