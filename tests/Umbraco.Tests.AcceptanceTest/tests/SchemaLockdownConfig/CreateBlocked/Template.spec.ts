import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const templateName = 'LockedTemplate';

test('blocks creating a template in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.template.goToSection(ConstantHelper.sections.settings);
  const sectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.template.isInlineEntityActionVisibleForName('Templates', 'ReloadChildrenOf');
    await umbracoUi.template.isSidebarActionsMenuVisibleForName('Templates', false);
    await umbracoUi.template.clickRootFolderCaretButton();
    await umbracoUi.template.clickActionsMenuForTemplate(templateName);
    await umbracoUi.template.isEntityActionWithNameVisible('Delete');
    await umbracoUi.template.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Create route', async () => {
    // Checked before the workspace, as the code editor check leaves unsaved changes that would prompt before leaving the page.
    await umbracoUi.page.goto(`${sectionUrl}/workspace/template/create/parent/template-root/null`);
    await umbracoUi.template.isEntityDetailForbiddenVisible();
  });

  await test.step('Workspace', async () => {
    await umbracoUi.template.clickRootFolderCaretButton();
    await umbracoUi.template.clickTreeItemWithName(templateName);
    await umbracoUi.template.isSaveButtonVisible();
    await umbracoUi.template.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.template.isWorkspaceNameEditable();
    await umbracoUi.template.isMonacoEditorEditable();
  });
});

test('blocks creating a template through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.post(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.template}`, {});

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('template:create');
});
