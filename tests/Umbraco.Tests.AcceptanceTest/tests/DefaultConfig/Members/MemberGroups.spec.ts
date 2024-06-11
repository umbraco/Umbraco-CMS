import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const memberGroupName = 'Test Member Group';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.memberGroup.goToSection(ConstantHelper.sections.members);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
});

test('can create a member group', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.memberGroup.clickMemberGroupsTab();
  await umbracoUi.memberGroup.clickCreateButton();
  await umbracoUi.memberGroup.enterMemberGroupName(memberGroupName);
  await umbracoUi.memberGroup.clickSaveButton();

  // Assert
  await umbracoUi.memberGroup.isSuccessNotificationVisible();
  await umbracoUi.memberGroup.clickLeftArrowButton();
  await umbracoUi.memberGroup.isMemberGroupNameVisible(memberGroupName);
  expect(await umbracoApi.memberGroup.doesNameExist(memberGroupName)).toBeTruthy();
});

test('cannot create member group with empty name', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.memberGroup.clickMemberGroupsTab();
  await umbracoUi.memberGroup.clickCreateButton();
  await umbracoUi.memberGroup.clickSaveButton();

  // Assert
  await umbracoUi.memberGroup.isErrorNotificationVisible();
  expect(await umbracoApi.memberGroup.doesNameExist(memberGroupName)).toBeFalsy();
});

test('cannot create member group with duplicate name', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.memberGroup.create(memberGroupName);
  expect(await umbracoApi.memberGroup.doesNameExist(memberGroupName)).toBeTruthy();

  // Act
  await umbracoUi.memberGroup.clickMemberGroupsTab();
  await umbracoUi.memberGroup.clickCreateButton();
  await umbracoUi.memberGroup.enterMemberGroupName(memberGroupName);
  await umbracoUi.memberGroup.clickSaveButton();

  // Assert
  await umbracoUi.memberGroup.isErrorNotificationVisible();
});

// TODO: Remove skip when the front-end is ready. Currently it is impossible to delete a member group.
test.skip('can delete a member group', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.memberGroup.create(memberGroupName);
  expect(await umbracoApi.memberGroup.doesNameExist(memberGroupName)).toBeTruthy();

  // Act
  await umbracoUi.memberGroup.clickMemberGroupsTab();
  await umbracoUi.memberGroup.clickMemberGroupLinkByName(memberGroupName);
  await umbracoUi.memberGroup.clickActionsButton();
  await umbracoUi.memberGroup.clickDeleteButton();
  await umbracoUi.memberGroup.clickConfirmToDeleteButton();

  // Assert
  await umbracoUi.memberGroup.isSuccessNotificationVisible();
  await umbracoUi.memberGroup.isMemberGroupNameVisible(memberGroupName, false);
  expect(await umbracoApi.memberGroup.doesNameExist(memberGroupName)).toBeFalsy();
});
