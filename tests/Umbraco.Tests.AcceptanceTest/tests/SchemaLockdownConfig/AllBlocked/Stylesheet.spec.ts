import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const stylesheetName = 'LockedStylesheet.css';
const stylesheetFolderName = 'LockedStylesheetFolder';

test('blocks every change to a stylesheet in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.stylesheet.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.stylesheet.isInlineEntityActionVisibleForName('Stylesheets', 'ReloadChildrenOf');
    await umbracoUi.stylesheet.isSidebarActionsMenuVisibleForName('Stylesheets', false);
    await umbracoUi.stylesheet.clickRootFolderCaretButton();
    await umbracoUi.stylesheet.isInlineEntityActionVisibleForName(stylesheetFolderName, 'ReloadChildrenOf');
    await umbracoUi.stylesheet.isSidebarActionsMenuVisibleForName(stylesheetFolderName, false);
    await umbracoUi.stylesheet.isSidebarActionsMenuVisibleForName(stylesheetName, false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.stylesheet.clickTreeItemWithName(stylesheetName);
    await umbracoUi.stylesheet.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.stylesheet.isSaveButtonVisible(false);
    await umbracoUi.stylesheet.isMonacoEditorReadOnly();
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/stylesheet/create/parent/stylesheet-root/null`);
    await umbracoUi.stylesheet.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a stylesheet through the Management API', async ({umbracoApi}) => {
  // Arrange
  const stylesheet = await umbracoApi.stylesheet.get(stylesheetName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.stylesheet}`;
  const path = encodeURIComponent(stylesheet.path);

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${path}`, {content: stylesheet.content});
  const renameResponse = await umbracoApi.put(`${url}/${path}/rename`, {name: stylesheet.name});
  const deleteResponse = await umbracoApi.delete(`${url}/${path}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('stylesheet:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('stylesheet:update');
  expect(renameResponse.status()).toBe(403);
  expect(renameResponse.headers()['umb-schema-lockdown']).toBe('stylesheet:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('stylesheet:delete');
});
