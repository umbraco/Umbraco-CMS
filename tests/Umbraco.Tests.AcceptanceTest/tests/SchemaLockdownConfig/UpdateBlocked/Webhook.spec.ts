import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const webhookName = 'LockedWebhook';

test('blocks updating a webhook in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.settings);
  await umbracoUi.webhook.goToSettingsTreeItem('Webhooks');

  await test.step('Collection', async () => {
    await umbracoUi.webhook.isCollectionActionWithNameVisible('Create');
    await umbracoUi.webhook.isEntityActionVisibleInCollectionRowForName(webhookName, 'Delete');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.webhook.clickTextButtonWithName(webhookName);
    await umbracoUi.webhook.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.webhook.isSaveButtonVisible(false);
    await umbracoUi.webhook.isWorkspaceNameEditable(false);
    await umbracoUi.schemaLockdown.clickLockedWorkspaceActionButton();
    await umbracoUi.webhook.doesModalHaveText('Changes to this item are locked in this environment.');
  });
});

test('blocks updating a webhook through the Management API', async ({umbracoApi}) => {
  // Arrange
  const webhook = await umbracoApi.webhook.getByName(webhookName);

  // Act
  const response = await umbracoApi.put(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.webhook}/${webhook.id}`, webhook);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('webhook:update');
});
