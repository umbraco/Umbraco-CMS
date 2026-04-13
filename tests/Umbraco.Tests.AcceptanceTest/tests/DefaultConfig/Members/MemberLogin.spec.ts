import {test} from '@umbraco/acceptance-test-helpers';

const memberTypeName = 'Member Auth Test Type';
const memberName = 'Member Auth Login Test';
const username = 'memberauthlogin';
const email = 'memberauthlogin@acceptance.test';
const password = 'Umbraco9Rocks!';
const documentTypeName = 'Member Auth Login Page Type';
const documentName = 'Member Auth Login Page';
const templateName = 'Member Auth Login Template';

test.describe('Member login', () => {
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

  test('member can log in with valid credentials', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
    await umbracoApi.member.createApprovedMember(memberName, memberTypeId, email, username, password);
    const templateId = await umbracoApi.template.createMemberLoginTemplate(templateName);
    const url = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
    await umbracoUi.memberAuthentication.navigateToPage(url);
    await umbracoUi.memberAuthentication.expectAnonymous();

    // Act
    await umbracoUi.memberAuthentication.fillLoginForm(username, password);
    await umbracoUi.memberAuthentication.submitLoginForm();

    // Assert
    await umbracoUi.memberAuthentication.expectAuthenticated(username);
  });

  test('member cannot log in with invalid password', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
    await umbracoApi.member.createApprovedMember(memberName, memberTypeId, email, username, password);
    const templateId = await umbracoApi.template.createMemberLoginTemplate(templateName);
    const url = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
    await umbracoUi.memberAuthentication.navigateToPage(url);

    // Act
    await umbracoUi.memberAuthentication.fillLoginForm(username, 'wrongPassword!1');
    await umbracoUi.memberAuthentication.submitLoginForm();

    // Assert
    await umbracoUi.memberAuthentication.expectAnonymous();
    await umbracoUi.memberAuthentication.expectValidationError('Invalid username or password');
  });

  test('member cannot log in when not approved', async ({umbracoApi, umbracoUi}) => {
    // Arrange — createDefaultMember produces an unapproved member (isApproved defaults to false).
    const memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
    await umbracoApi.member.createDefaultMember(memberName, memberTypeId, email, username, password);
    const templateId = await umbracoApi.template.createMemberLoginTemplate(templateName);
    const url = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
    await umbracoUi.memberAuthentication.navigateToPage(url);

    // Act
    await umbracoUi.memberAuthentication.fillLoginForm(username, password);
    await umbracoUi.memberAuthentication.submitLoginForm();

    // Assert
    await umbracoUi.memberAuthentication.expectAnonymous();
    await umbracoUi.memberAuthentication.expectValidationError('Member is not allowed');
  });

  test('member cannot log in when locked out', async ({umbracoApi, umbracoUi}) => {
    // Arrange — MemberBuilder doesn't expose isLockedOut, so flip it post-create.
    const memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
    const memberId = await umbracoApi.member.createApprovedMember(memberName, memberTypeId, email, username, password);
    await umbracoApi.member.setLockedOut(memberId, true);
    const templateId = await umbracoApi.template.createMemberLoginTemplate(templateName);
    const url = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
    await umbracoUi.memberAuthentication.navigateToPage(url);

    // Act
    await umbracoUi.memberAuthentication.fillLoginForm(username, password);
    await umbracoUi.memberAuthentication.submitLoginForm();

    // Assert
    await umbracoUi.memberAuthentication.expectAnonymous();
    await umbracoUi.memberAuthentication.expectValidationError('Member is locked out');
  });
});
