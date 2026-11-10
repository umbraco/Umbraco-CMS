import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const stylesheetName = 'LockedStylesheet.css';
const stylesheetFolderName = 'LockedStylesheetFolder';

test('blocks creating a stylesheet in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.stylesheet.isInlineEntityActionVisibleForName('Stylesheets', 'ReloadChildrenOf');
    await umbracoUi.stylesheet.isSidebarActionsMenuVisibleForName('Stylesheets', false);
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetFolderName);
    await umbracoUi.stylesheet.isEntityActionWithNameVisible('Delete');
    await umbracoUi.stylesheet.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Create route', async () => {
    // Checked before the workspace, as the code editor check leaves unsaved changes that would prompt before leaving the page.
    await umbracoUi.page.goto(`${sectionUrl}/workspace/stylesheet/create/parent/stylesheet-root/null`);
    await umbracoUi.stylesheet.isEntityDetailForbiddenVisible();
  });

  await test.step('Workspace', async () => {
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickTreeItemWithName(stylesheetName);
    await umbracoUi.stylesheet.isSaveButtonVisible();
    await umbracoUi.stylesheet.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.stylesheet.isMonacoEditorEditable();
  });
});

test('blocks creating a stylesheet through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.post(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.stylesheet}`, {});

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('stylesheet:create');
});
