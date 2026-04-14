import {test} from '@umbraco/acceptance-test-helpers';

const memberTypeName = 'Test Login Member Type';
const memberName = 'Test Login Member';
const username = 'testloginmember';
const email = 'testloginmember@acceptance.test';
const password = '0123456789';
const documentTypeName = 'Test Login Page Type';
const documentName = 'Test Login Page';
const templateName = 'Test Login Template';
let memberTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.member.ensureNameNotExists(memberName);
  await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
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

test('member can log in with valid credentials', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.member.createApprovedMember(memberName, memberTypeId, email, username, password);
  const templateId = await umbracoApi.template.createMemberLoginTemplate(templateName);
  const documentId = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
  const url = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(url);
  await umbracoUi.memberAuthentication.isNotLoggedIn();

  // Act
  await umbracoUi.memberAuthentication.fillLoginForm(username, password);
  await umbracoUi.memberAuthentication.submitLoginForm();

  // Assert
  await umbracoUi.memberAuthentication.isAuthenticated(username);
});

test('member cannot log in with invalid password', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.member.createApprovedMember(memberName, memberTypeId, email, username, password);
  const templateId = await umbracoApi.template.createMemberLoginTemplate(templateName);
  const documentId = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
  const url = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(url);

  // Act
  await umbracoUi.memberAuthentication.fillLoginForm(username, 'wrongPassword!1');
  await umbracoUi.memberAuthentication.submitLoginForm();

  // Assert
  await umbracoUi.memberAuthentication.isNotLoggedIn();
  await umbracoUi.memberAuthentication.doesValidationErrorShow('Invalid username or password');
});

test('member cannot log in when not approved', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
  const templateId = await umbracoApi.template.createMemberLoginTemplate(templateName);
  const documentId = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
  const url = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(url);

  // Act
  await umbracoUi.memberAuthentication.fillLoginForm(username, password);
  await umbracoUi.memberAuthentication.submitLoginForm();

  // Assert
  await umbracoUi.memberAuthentication.isNotLoggedIn();
  await umbracoUi.memberAuthentication.doesValidationErrorShow('Member is not allowed');
});

test('member cannot log in when locked out', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const memberId = await umbracoApi.member.createApprovedMember(memberName, memberTypeId, email, username, password);
  await umbracoApi.member.setLockedOut(memberId, true);
  const templateId = await umbracoApi.template.createMemberLoginTemplate(templateName);
  const documentId = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
  const url = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(url);

  // Act
  await umbracoUi.memberAuthentication.fillLoginForm(username, password);
  await umbracoUi.memberAuthentication.submitLoginForm();

  // Assert
  await umbracoUi.memberAuthentication.isNotLoggedIn();
  await umbracoUi.memberAuthentication.doesValidationErrorShow('Member is locked out');
});
