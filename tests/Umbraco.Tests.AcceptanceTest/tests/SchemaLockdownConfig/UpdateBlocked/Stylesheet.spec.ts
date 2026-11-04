import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const stylesheetName = 'LockedStylesheet.css';

test('blocks updating a stylesheet in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.stylesheet.clickActionsMenuAtRoot();
    await umbracoUi.stylesheet.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.isInlineEntityActionVisibleForName(stylesheetName, 'Delete');
    await umbracoUi.stylesheet.isSidebarActionsMenuVisibleForName(stylesheetName, false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.stylesheet.clickTreeItemWithName(stylesheetName);
    await umbracoUi.stylesheet.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.stylesheet.isSaveButtonVisible(false);
    await umbracoUi.stylesheet.isMonacoEditorReadOnly();
    await umbracoUi.schemaLockdown.clickLockedWorkspaceActionButton();
    await umbracoUi.stylesheet.doesModalHaveText('Changes to this item are locked in this environment.');
  });
});

test('blocks updating a stylesheet through the Management API', async ({umbracoApi}) => {
  // Arrange
  const stylesheet = await umbracoApi.stylesheet.get(stylesheetName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.stylesheet}/${encodeURIComponent(stylesheet.path)}`;

  // Act
  const updateResponse = await umbracoApi.put(url, {content: stylesheet.content});
  const renameResponse = await umbracoApi.put(`${url}/rename`, {name: stylesheet.name});

  // Assert
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('stylesheet:update');
  expect(renameResponse.status()).toBe(403);
  expect(renameResponse.headers()['umb-schema-lockdown']).toBe('stylesheet:update');
});
