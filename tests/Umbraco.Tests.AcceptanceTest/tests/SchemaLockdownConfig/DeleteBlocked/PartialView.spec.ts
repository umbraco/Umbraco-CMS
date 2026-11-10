import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const partialViewName = 'LockedPartialView.cshtml';
const partialViewFolderName = 'LockedPartialViewFolder';

test('blocks deleting a partial view in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.partialView.clickActionsMenuAtRoot();
    await umbracoUi.partialView.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await umbracoUi.partialView.clickActionsMenuForPartialView(partialViewFolderName);
    await umbracoUi.partialView.isEntityActionWithNameVisible('Create');
    await umbracoUi.partialView.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.partialView.isInlineEntityActionVisibleForName(partialViewName, 'Rename');
    await umbracoUi.partialView.isSidebarActionsMenuVisibleForName(partialViewName, false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.partialView.clickTreeItemWithName(partialViewName);
    await umbracoUi.partialView.isSaveButtonVisible();
    await umbracoUi.partialView.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.partialView.isMonacoEditorEditable();
  });
});

test('blocks deleting a partial view through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.delete(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.partialView}/${encodeURIComponent(partialViewName)}`);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('partial-view:delete');
});
