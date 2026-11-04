import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const dataTypeName = 'LockedDataType';
const dataTypeFolderName = 'LockedDataTypeFolder';

test('blocks creating a data type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSection(ConstantHelper.sections.settings);
  const settingsSectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.dataType.isInlineEntityActionVisibleForName('Data Types', 'ReloadChildrenOf');
    await umbracoUi.dataType.isSidebarActionsMenuVisibleForName('Data Types', false);
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeFolderName);
    await umbracoUi.dataType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.dataType.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeName);
    await umbracoUi.dataType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.dataType.isEntityActionWithNameVisible('DuplicateTo', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.dataType.goToDataType(dataTypeName);
    await umbracoUi.dataType.isSaveButtonVisible();
    await umbracoUi.dataType.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.dataType.isWorkspaceNameEditable();
    await umbracoUi.dataType.isPropertyEditorUiWithNameReadOnly('number', false);
    await umbracoUi.dataType.isChangePropertyEditorButtonVisible();
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${settingsSectionUrl}/workspace/data-type/create/parent/data-type-root/null`);
    await umbracoUi.dataType.isEntityDetailForbiddenVisible();
  });
});

test('blocks creating a data type through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.post(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.dataType}`, {});

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('data-type:create');
});
