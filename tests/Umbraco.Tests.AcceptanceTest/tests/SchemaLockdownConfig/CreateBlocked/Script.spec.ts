import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const scriptName = 'LockedScript.js';
const scriptFolderName = 'LockedScriptFolder';

test('blocks creating a script in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.script.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.script.isInlineEntityActionVisibleForName('Scripts', 'ReloadChildrenOf');
    await umbracoUi.script.isSidebarActionsMenuVisibleForName('Scripts', false);
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickActionsMenuForScript(scriptFolderName);
    await umbracoUi.script.isEntityActionWithNameVisible('Delete');
    await umbracoUi.script.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Create route', async () => {
    // Checked before the workspace, as the code editor check leaves unsaved changes that would prompt before leaving the page.
    await umbracoUi.page.goto(`${sectionUrl}/workspace/script/create/parent/script-root/null`);
    await umbracoUi.script.isEntityDetailForbiddenVisible();
  });

  await test.step('Workspace', async () => {
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.clickTreeItemWithName(scriptName);
    await umbracoUi.script.isSaveButtonVisible();
    await umbracoUi.script.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.script.isMonacoEditorEditable();
  });
});

test('blocks creating a script through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.post(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.script}`, {});

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('script:create');
});
