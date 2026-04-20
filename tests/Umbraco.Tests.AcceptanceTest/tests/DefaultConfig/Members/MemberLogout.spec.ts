import {test} from '@umbraco/acceptance-test-helpers';

const memberTypeName = 'Test Logout Member Type';
const memberName = 'Test Logout Member';
const username = 'testlogoutmember';
const email = 'testlogoutmember@acceptance.test';
const password = '0123456789';
const documentTypeName = 'Test Logout Page Type';
const documentName = 'Test Logout Page';
const templateName = 'Test Logout Template';
let memberTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
});

test.afterEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.member.ensureNameNotExists(memberName);
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  await umbracoUi.memberAuthentication.clearMemberAuthCookie();
});

test('logged-in member can log out', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.member.createApprovedMember(memberName, memberTypeId, email, username, password);
  const templateId = await umbracoApi.template.createMemberLoginTemplate(templateName);
  const documentId = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
  const url = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(url);
  await umbracoUi.memberAuthentication.fillLoginForm(username, password);
  await umbracoUi.memberAuthentication.submitLoginForm();
  await umbracoUi.memberAuthentication.isAuthenticated(username);

  // Act
  await umbracoUi.memberAuthentication.clickLogoutButton();

  // Assert
  await umbracoUi.memberAuthentication.isNotLoggedIn();
});
