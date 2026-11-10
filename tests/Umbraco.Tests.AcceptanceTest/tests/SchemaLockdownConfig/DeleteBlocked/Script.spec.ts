import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const scriptName = 'LockedScript.js';
const scriptFolderName = 'LockedScriptFolder';

test('blocks deleting a script in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.script.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.script.clickActionsMenuAtRoot();
    await umbracoUi.script.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.isEntityActionWithNameVisible('Create');
    await umbracoUi.script.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.script.isInlineEntityActionVisibleForName(scriptName, 'Rename');
    await umbracoUi.script.isSidebarActionsMenuVisibleForName(scriptName, false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.script.clickTreeItemWithName(scriptName);
    await umbracoUi.script.isSaveButtonVisible();
    await umbracoUi.script.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.script.isMonacoEditorEditable();
  });
});

test('blocks deleting a script through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.delete(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.script}/${encodeURIComponent(scriptName)}`);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('script:delete');
});
