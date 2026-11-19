import {test} from '@umbraco/acceptance-test-helpers';

const memberName = 'BackofficeSearchMember';
const memberUsername = 'backofficeSearchMember';
const memberEmail = 'backoffice-search-member@acceptance.test';
const memberPassword = '0123456789';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.member.ensureNameNotExists(memberName);
});

test('can find a member by name', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const defaultMemberType = await umbracoApi.memberType.getByName('Member');
  const memberId = await umbracoApi.member.createDefaultMember(
    memberName,
    defaultMemberType.id,
    memberEmail,
    memberUsername,
    memberPassword,
  );
  await umbracoApi.member.waitUntilIndexed(memberName, memberId);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.clickSearchProvider('Members');
  await umbracoUi.backofficeSearch.searchForMember(memberName);

  // Assert
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(memberName);
});
