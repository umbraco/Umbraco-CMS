import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const partialViewName = 'LockedPartialView.cshtml';
const partialViewFolderName = 'LockedPartialViewFolder';

test('blocks creating a partial view in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.partialView.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.partialView.isInlineEntityActionVisibleForName('Partial Views', 'ReloadChildrenOf');
    await umbracoUi.partialView.isSidebarActionsMenuVisibleForName('Partial Views', false);
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await umbracoUi.partialView.clickActionsMenuForPartialView(partialViewFolderName);
    await umbracoUi.partialView.isEntityActionWithNameVisible('Delete');
    await umbracoUi.partialView.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Create route', async () => {
    // Checked before the workspace, as the code editor check leaves unsaved changes that would prompt before leaving the page.
    await umbracoUi.page.goto(`${sectionUrl}/workspace/partial-view/create/parent/partial-view-root/null`);
    await umbracoUi.partialView.isEntityDetailForbiddenVisible();
  });

  await test.step('Workspace', async () => {
    await umbracoUi.partialView.clickRootFolderCaretButton();
    await umbracoUi.partialView.clickTreeItemWithName(partialViewName);
    await umbracoUi.partialView.isSaveButtonVisible();
    await umbracoUi.partialView.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.partialView.isMonacoEditorEditable();
  });
});

test('blocks creating a partial view through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.post(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.partialView}`, {});

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('partial-view:create');
});
