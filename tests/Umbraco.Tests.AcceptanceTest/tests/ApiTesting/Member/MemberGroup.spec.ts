import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Member Group tests', () => {
  let memberGroupId = '';
  const memberGroupName = 'TestMemberGroup';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.memberGroup.ensureNameNotExists(memberGroupName);
  });

  test('can create a member group', async ({umbracoApi}) => {
    // Act
    memberGroupId = await umbracoApi.memberGroup.create(memberGroupName);

    // Assert
    expect(await umbracoApi.memberGroup.doesExist(memberGroupId)).toBeTruthy();
  });

  test('can rename a member group', async ({umbracoApi}) => {
    // Arrange
    const wrongMemberGroupName = 'Wrong Name';
    await umbracoApi.memberGroup.ensureNameNotExists(wrongMemberGroupName);
    memberGroupId = await umbracoApi.memberGroup.create(wrongMemberGroupName);
    expect(await umbracoApi.memberGroup.doesExist(memberGroupId)).toBeTruthy();

    // Act
    await umbracoApi.memberGroup.rename(memberGroupId, memberGroupName);

    // Assert
    expect(await umbracoApi.memberGroup.doesNameExist(memberGroupName)).toBeTruthy();
    expect(await umbracoApi.memberGroup.doesNameExist(wrongMemberGroupName)).toBeFalsy();
  });

  test('can delete a member group', async ({umbracoApi}) => {
    // Arrange
    memberGroupId = await umbracoApi.memberGroup.create(memberGroupName);
    expect(await umbracoApi.memberGroup.doesExist(memberGroupId)).toBeTruthy();

    // Act
    await umbracoApi.memberGroup.delete(memberGroupId);

    // Assert
    expect(await umbracoApi.memberGroup.doesExist(memberGroupId)).toBeFalsy();
  });
});