import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const memberTypeName = 'LockedMemberType';
const memberTypeFolderName = 'LockedMemberTypeFolder';

test('blocks every change to a member type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberType.goToSection(ConstantHelper.sections.settings);
  const settingsSectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.memberType.isInlineEntityActionVisibleForName('Member Types', 'ReloadChildrenOf');
    await umbracoUi.memberType.isSidebarActionsMenuVisibleForName('Member Types', false);
    await umbracoUi.memberType.clickRootFolderCaretButton();
    await umbracoUi.memberType.isInlineEntityActionVisibleForName(memberTypeFolderName, 'ReloadChildrenOf');
    await umbracoUi.memberType.isSidebarActionsMenuVisibleForName(memberTypeFolderName, false);
    await umbracoUi.memberType.clickActionsMenuForMemberType(memberTypeName);
    await umbracoUi.memberType.isEntityActionWithNameVisible('Export');
    await umbracoUi.memberType.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.memberType.isEntityActionWithNameVisible('DuplicateTo', false);
    await umbracoUi.memberType.isEntityActionWithNameVisible('MoveTo', false);
    await umbracoUi.memberType.isEntityActionWithNameVisible('Delete', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.memberType.goToMemberType(memberTypeName);
    await umbracoUi.memberType.isReadOnlyTagVisible();
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible();
    await umbracoUi.memberType.isSaveButtonVisible(false);
    await umbracoUi.memberType.isWorkspaceNameEditable(false);
    await umbracoUi.memberType.isPropertyTypeEditable('LockedProperty', false);
    await umbracoUi.memberType.isAddPropertyButtonVisible(false);
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${settingsSectionUrl}/workspace/member-type/create/parent/member-type-root/null`);
    await umbracoUi.memberType.isEntityDetailForbiddenVisible();
  });
});

test('blocks every change to a member type through the Management API', async ({umbracoApi}) => {
  // Arrange
  const memberType = await umbracoApi.memberType.getByName(memberTypeName);
  const url = `${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.memberType}`;

  // Act
  const createResponse = await umbracoApi.post(url, {});
  const updateResponse = await umbracoApi.put(`${url}/${memberType.id}`, memberType);
  const deleteResponse = await umbracoApi.delete(`${url}/${memberType.id}`);

  // Assert
  expect(createResponse.status()).toBe(403);
  expect(createResponse.headers()['umb-schema-lockdown']).toBe('member-type:create');
  expect(updateResponse.status()).toBe(403);
  expect(updateResponse.headers()['umb-schema-lockdown']).toBe('member-type:update');
  expect(deleteResponse.status()).toBe(403);
  expect(deleteResponse.headers()['umb-schema-lockdown']).toBe('member-type:delete');
});
