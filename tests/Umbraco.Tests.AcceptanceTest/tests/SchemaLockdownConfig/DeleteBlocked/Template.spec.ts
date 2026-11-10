import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const templateName = 'LockedTemplate';

test('blocks deleting a template in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.template.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.template.clickActionsMenuAtRoot();
    await umbracoUi.template.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.template.clickRootFolderCaretButton();
    await umbracoUi.template.clickActionsMenuForTemplate(templateName);
    await umbracoUi.template.isEntityActionWithNameVisible('Create');
    await umbracoUi.template.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.template.clickTreeItemWithName(templateName);
    await umbracoUi.template.isSaveButtonVisible();
    await umbracoUi.template.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.template.isWorkspaceNameEditable();
    await umbracoUi.template.isMonacoEditorEditable();
  });
});

test('blocks deleting a template through the Management API', async ({umbracoApi}) => {
  // Arrange
  const template = await umbracoApi.template.getByName(templateName);

  // Act
  const response = await umbracoApi.delete(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.template}/${template.id}`);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('template:delete');
});
