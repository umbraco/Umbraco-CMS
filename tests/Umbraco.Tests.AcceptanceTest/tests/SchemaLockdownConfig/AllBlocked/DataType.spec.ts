import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const dataTypeName = 'LockedDataType';
const dataTypeFolderName = 'LockedDataTypeFolder';

test('blocks every change to a data type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSection(ConstantHelper.sections.settings);
  const settingsSectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.dataType.isInlineEntityActionVisibleForName('Data Types', 'ReloadChildrenOf');
    await umbracoUi.dataType.isSidebarActionsMenuVisibleForName('Data Types', false);
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.isInlineEntityActionVisibleForName(dataTypeFolderName, 'ReloadChildrenOf');
    await umbracoUi.dataType.isSidebarActionsMenuVisibleForName(dataTypeFolderName, false);
    await umbracoUi.dataType.isSidebarActionsMenuVisibleForName(dataTypeName, false);
  });

  await test.step('Workspace', async () => {
    await umbracoUi.dataType.goToDataType(dataTypeName);
    await umbracoUi.dataType.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.dataType.isSaveButtonVisible(false);
    await umbracoUi.dataType.isWorkspaceNameEditable(false);
    await umbracoUi.dataType.isPropertyEditorUiWithNameReadOnly('number');
    await umbracoUi.dataType.isChangePropertyEditorButtonVisible(false);
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${settingsSectionUrl}/workspace/data-type/create/parent/data-type-root/null`);
    await umbracoUi.dataType.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a data type through the Management API', async ({umbracoApi}) => {
  // Arrange
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.dataType}`;

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${dataType.id}`, dataType);
  const deleteResponse = await umbracoApi.delete(`${url}/${dataType.id}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('data-type:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('data-type:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('data-type:delete');
});
