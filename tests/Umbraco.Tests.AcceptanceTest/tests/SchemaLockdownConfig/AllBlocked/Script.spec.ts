import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const scriptName = 'LockedScript.js';
const scriptFolderName = 'LockedScriptFolder';

test('blocks every change to a script in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.script.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.script.isInlineEntityActionVisibleForName('Scripts', 'ReloadChildrenOf');
    await umbracoUi.script.isSidebarActionsMenuVisibleForName('Scripts', false);
    await umbracoUi.script.clickRootFolderCaretButton();
    await umbracoUi.script.isInlineEntityActionVisibleForName(scriptFolderName, 'ReloadChildrenOf');
    await umbracoUi.script.isSidebarActionsMenuVisibleForName(scriptFolderName, false);
    await umbracoUi.script.isSidebarActionsMenuVisibleForName(scriptName, false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.script.clickTreeItemWithName(scriptName);
    await umbracoUi.script.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.script.isSaveButtonVisible(false);
    await umbracoUi.script.isMonacoEditorReadOnly();
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/script/create/parent/script-root/null`);
    await umbracoUi.script.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a script through the Management API', async ({umbracoApi}) => {
  // Arrange
  const script = await umbracoApi.script.get(scriptName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.script}`;
  const path = encodeURIComponent(script.path);

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${path}`, {content: script.content});
  const renameResponse = await umbracoApi.put(`${url}/${path}/rename`, {name: script.name});
  const deleteResponse = await umbracoApi.delete(`${url}/${path}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('script:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('script:update');
  expect(renameResponse.status()).toBe(403);
  expect(renameResponse.headers()['umb-schema-lockdown']).toBe('script:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('script:delete');
});
