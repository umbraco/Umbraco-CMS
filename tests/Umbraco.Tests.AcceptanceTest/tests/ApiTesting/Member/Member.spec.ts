import {test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

test.describe('Member tests', () => {
  let memberId = '';
  let memberTypeId = '';
  const memberName = 'TestMember';
  const memberTypeName = 'TestMemberType';
  const username = 'testmember';
  const email = 'testmember@acceptance.test';
  const password = '0123456789';

  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
    await umbracoApi.member.ensureNameNotExists(memberName);
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
    await umbracoApi.member.ensureNameNotExists(memberName);
  });

  test('can create a member', async ({umbracoApi}) => {
    // Arrange    
    memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);

    // Act  
    memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);

    // Assert
    expect(await umbracoApi.member.doesExist(memberId)).toBeTruthy();
  });

  test('can delete a member', async ({umbracoApi}) => {
    // Arrange    
    memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
    memberId = await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);

    // Act  
    await umbracoApi.member.delete(memberId);

    // Assert
    expect(await umbracoApi.member.doesExist(memberId)).toBeFalsy();
  });
});