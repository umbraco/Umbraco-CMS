import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Seeded by AdditionalSetup/SchemaLockdownComposer.cs
const memberTypeName = 'LockedMemberType';
const memberTypeFolderName = 'LockedMemberTypeFolder';

test('blocks creating a member type in the backoffice', async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberType.goToSection(ConstantHelper.sections.settings);
  const settingsSectionUrl = umbracoUi.page.url();

  await test.step('Tree', async () => {
    await umbracoUi.memberType.isInlineEntityActionVisibleForName('Member Types', 'ReloadChildrenOf');
    await umbracoUi.memberType.isSidebarActionsMenuVisibleForName('Member Types', false);
    await umbracoUi.memberType.clickRootFolderCaretButton();
    await umbracoUi.memberType.clickActionsMenuForMemberType(memberTypeFolderName);
    await umbracoUi.memberType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.memberType.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.page.keyboard.press('Escape');
    await umbracoUi.memberType.clickActionsMenuForMemberType(memberTypeName);
    await umbracoUi.memberType.isEntityActionWithNameVisible('Delete');
    await umbracoUi.memberType.isEntityActionWithNameVisible('Create', false);
    await umbracoUi.memberType.isEntityActionWithNameVisible('DuplicateTo', false);
    await umbracoUi.page.keyboard.press('Escape');
  });

  await test.step('Workspace', async () => {
    await umbracoUi.memberType.goToMemberType(memberTypeName);
    await umbracoUi.memberType.isSaveButtonVisible();
    await umbracoUi.memberType.isReadOnlyTagVisible(false);
    await umbracoUi.schemaLockdown.isLockedWorkspaceActionVisible(false);
    await umbracoUi.memberType.isWorkspaceNameEditable();
    await umbracoUi.memberType.isPropertyTypeEditable('LockedProperty');
  });

  await test.step('Create route', async () => {
    await umbracoUi.page.goto(`${settingsSectionUrl}/workspace/member-type/create/parent/member-type-root/null`);
    await umbracoUi.memberType.isEntityDetailForbiddenVisible();
  });
});

test('blocks creating a member type through the Management API', async ({umbracoApi}) => {
  // Act
  const response = await umbracoApi.post(`${umbracoApi.baseUrl}${ConstantHelper.apiEndpoints.memberType}`, {});

  // Assert
  expect(response.status()).toBe(403);
  expect(response.headers()['umb-schema-lockdown']).toBe('member-type:create');
});
