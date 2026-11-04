import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const scriptName = 'LockedScript.js';

test('blocks updating a script in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.script.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.script.clickActionsMenuAtRoot();
    await umbracoUi.script.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.isInlineEntityActionVisibleForName(scriptName, 'Delete');
    await umbracoUi.script.isSidebarActionsMenuVisibleForName(scriptName, false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.script.clickTreeItemWithName(scriptName);
    await umbracoUi.script.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.script.isSaveButtonVisible(false);
    await umbracoUi.script.isMonacoEditorReadOnly();
    await umbracoUi.schemaLockdown.clickLockedWorkspaceActionButton();
    await umbracoUi.script.doesModalHaveText('Changes to this item are locked in this environment.');
  });
});

test('blocks updating a script through the Management API', async ({umbracoApi}) => {
  // Arrange
  const script = await umbracoApi.script.get(scriptName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.script}/${encodeURIComponent(script.path)}`;

  // Act
  const updateResponse = await umbracoApi.put(url, {content: script.content});
  const renameResponse = await umbracoApi.put(`${url}/rename`, {name: script.name});

  // Assert
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('script:update');
  expect(renameResponse.status()).toBe(403);
  expect(renameResponse.headers()['umb-schema-lockdown']).toBe('script:update');
});
