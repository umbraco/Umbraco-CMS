import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const webhookName = 'LockedWebhook';

test('blocks creating a webhook in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.webhook.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();
  await umbracoUi.webhook.goToSettingsTreeItem('Webhooks');

  await test.step('Collection', async () => {
    await umbracoUi.webhook.isEntityActionVisibleInCollectionRowForName(webhookName, 'Delete');
    await umbracoUi.webhook.isCollectionActionWithNameVisible('Create', false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.webhook.clickTextButtonWithName(webhookName);
    await umbracoUi.webhook.isSaveButtonVisible();
    await umbracoUi.webhook.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.webhook.isWorkspaceNameEditable();
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/webhook/create`);
    await umbracoUi.webhook.isEntityDetailForbiddenVisible();
  });
});

test('blocks creating a webhook through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.post(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.webhook}`, {});

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('webhook:create');
});
