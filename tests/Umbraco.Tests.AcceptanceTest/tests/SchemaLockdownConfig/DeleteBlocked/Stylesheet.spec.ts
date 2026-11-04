import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const stylesheetName = 'LockedStylesheet.css';
const stylesheetFolderName = 'LockedStylesheetFolder';

test('blocks deleting a stylesheet in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.clickActionsMenuForStylesheet(stylesheetFolderName);
    await umbracoUi.stylesheet.isEntityActionWithNameVisible('Create');
    await umbracoUi.stylesheet.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.stylesheet.isInlineEntityActionVisibleForName(stylesheetName, 'Rename');
    await umbracoUi.stylesheet.isSidebarActionsMenuVisibleForName(stylesheetName, false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.stylesheet.clickTreeItemWithName(stylesheetName);
    await umbracoUi.stylesheet.isSaveButtonVisible();
    await umbracoUi.stylesheet.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.stylesheet.isMonacoEditorEditable();
  });
});

test('blocks deleting a stylesheet through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.delete(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.stylesheet}/${encodeURIComponent(stylesheetName)}`);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('stylesheet:delete');
});
