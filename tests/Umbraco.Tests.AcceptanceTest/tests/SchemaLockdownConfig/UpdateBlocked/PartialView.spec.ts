import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const partialViewName = 'LockedPartialView.cshtml';

test('blocks updating a partial view in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.partialView.clickActionsMenuAtRoot();
    await umbracoUi.partialView.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await umbracoUi.partialView.isInlineEntityActionVisibleForName(partialViewName, 'Delete');
    await umbracoUi.partialView.isSidebarActionsMenuVisibleForName(partialViewName, false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.partialView.clickTreeItemWithName(partialViewName);
    await umbracoUi.partialView.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.partialView.isSaveButtonVisible(false);
    await umbracoUi.partialView.isMonacoEditorReadOnly();
    await umbracoUi.schemaLockdown.clickLockedWorkspaceActionButton();
    await umbracoUi.partialView.doesModalHaveText('Changes to this item are locked in this environment.');
  });
});

test('blocks updating a partial view through the Management API', async ({umbracoApi}) => {
  // Arrange
  const partialView = await umbracoApi.partialView.get(partialViewName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.partialView}/${encodeURIComponent(partialView.path)}`;

  // Act
  const updateResponse = await umbracoApi.put(url, {content: partialView.content});
  const renameResponse = await umbracoApi.put(`${url}/rename`, {name: partialView.name});

  // Assert
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('partial-view:update');
  expect(renameResponse.status()).toBe(403);
  expect(renameResponse.headers()['umb-schema-lockdown']).toBe('partial-view:update');
});
