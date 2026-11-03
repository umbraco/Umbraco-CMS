import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const templateName = 'LockedTemplate';

test('blocks every change to a template in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.template.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.template.isInlineEntityActionVisibleForName('Templates', 'ReloadChildrenOf');
    await umbracoUi.template.isSidebarActionsMenuVisibleForName('Templates', false);
    await umbracoUi.template.clickRootFolderCaretButton();
    await umbracoUi.template.isTreeItemVisible(templateName);
    await umbracoUi.template.isSidebarActionsMenuVisibleForName(templateName, false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.template.clickTreeItemWithName(templateName);
    await umbracoUi.template.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.template.isSaveButtonVisible(false);
    await umbracoUi.template.isWorkspaceNameEditable(false);
    await umbracoUi.template.isMonacoEditorReadOnly();
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${sectionUrl}/workspace/template/create/parent/template-root/null`);
    await umbracoUi.template.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a template through the Management API', async ({umbracoApi}) => {
  // Arrange
  const template = await umbracoApi.template.getByName(templateName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.template}`;

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${template.id}`, template);
  const deleteResponse = await umbracoApi.delete(`${url}/${template.id}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('template:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('template:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('template:delete');
});
