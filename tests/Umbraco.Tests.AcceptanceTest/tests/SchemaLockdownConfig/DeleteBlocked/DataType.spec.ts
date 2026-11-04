import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const dataTypeName = 'LockedDataType';
const dataTypeFolderName = 'LockedDataTypeFolder';

test('blocks deleting a data type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.dataType.goToSection(ConstantHelper.sections.settings);

  await test.step('Tree', async () => {
    await umbracoUi.dataType.clickActionsMenuAtRoot();
    await umbracoUi.dataType.isEntityActionWithNameVisible('Create');
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.dataType.clickRootFolderCaretButton();
    await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeFolderName);
    await umbracoUi.dataType.isEntityActionWithNameVisible('Rename');
    await umbracoUi.dataType.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.dataType.clickActionsMenuForDataType(dataTypeName);
    await umbracoUi.dataType.isEntityActionWithNameVisible('MoveTo');
    await umbracoUi.dataType.isEntityActionWithNameVisible('Delete', false);
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
});

test('blocks deleting a data type through the Management API', async ({umbracoApi}) => {
  // Arrange
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);

  // Act
  const response = await umbracoApi.delete(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.dataType}/${dataType.id}`);

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('data-type:delete');
});
