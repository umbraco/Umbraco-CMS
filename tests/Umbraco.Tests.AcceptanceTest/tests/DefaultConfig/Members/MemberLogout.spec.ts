import {test} from '@umbraco/acceptance-test-helpers';

const memberTypeName = 'Member Auth Logout Type';
const memberName = 'Member Auth Logout Test';
const username = 'memberauthlogout';
const email = 'memberauthlogout@acceptance.test';
const password = 'Umbraco9Rocks!';
const documentTypeName = 'Member Auth Logout Page Type';
const documentName = 'Member Auth Logout Page';
const templateName = 'Member Auth Logout Template';

test.describe('Member logout', () => {
  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.template.ensureNameNotExists(templateName);
    await umbracoApi.member.ensureNameNotExists(memberName);
    await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  });

  test.afterEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.template.ensureNameNotExists(templateName);
    await umbracoApi.member.ensureNameNotExists(memberName);
    await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
    await umbracoUi.memberAuthentication.clearMemberAuthCookie();
  });

  test('logged-in member can log out', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
    // Arrange — log the member in; the login template's status partial hosts the logout button.
    const memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
    await umbracoApi.member.createApprovedMember(memberName, memberTypeId, email, username, password);
    const templateId = await umbracoApi.template.createMemberLoginTemplate(templateName);
    const url = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
    await umbracoUi.memberAuthentication.navigateToPage(url);
    await umbracoUi.memberAuthentication.fillLoginForm(username, password);
    await umbracoUi.memberAuthentication.submitLoginForm();
    await umbracoUi.memberAuthentication.expectAuthenticated(username);

    // Act
    await umbracoUi.memberAuthentication.clickLogoutButton();

    // Assert
    await umbracoUi.memberAuthentication.expectAnonymous();
  });
});
