import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const partialViewName = 'LockedPartialView.cshtml';
const partialViewFolderName = 'LockedPartialViewFolder';

test('blocks every change to a partial view in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.partialView.isInlineEntityActionVisibleForName('Partial Views', 'ReloadChildrenOf');
    await umbracoUi.partialView.isSidebarActionsMenuVisibleForName('Partial Views', false);
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await umbracoUi.partialView.isInlineEntityActionVisibleForName(partialViewFolderName, 'ReloadChildrenOf');
    await umbracoUi.partialView.isSidebarActionsMenuVisibleForName(partialViewFolderName, false);
    await umbracoUi.partialView.isSidebarActionsMenuVisibleForName(partialViewName, false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.partialView.clickTreeItemWithName(partialViewName);
    await umbracoUi.partialView.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.partialView.isSaveButtonVisible(false);
    await umbracoUi.partialView.isMonacoEditorReadOnly();
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/partial-view/create/parent/partial-view-root/null`);
    await umbracoUi.partialView.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a partial view through the Management API', async ({umbracoApi}) => {
  // Arrange
  const partialView = await umbracoApi.partialView.get(partialViewName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.partialView}`;
  const path = encodeURIComponent(partialView.path);

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${path}`, {content: partialView.content});
  const renameResponse = await umbracoApi.put(`${url}/${path}/rename`, {name: partialView.name});
  const deleteResponse = await umbracoApi.delete(`${url}/${path}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('partial-view:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('partial-view:update');
  expect(renameResponse.status()).toBe(403);
  expect(renameResponse.headers()['umb-schema-lockdown']).toBe('partial-view:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('partial-view:delete');
});
