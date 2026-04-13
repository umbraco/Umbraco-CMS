import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const memberName = 'Member Auth Register Test';
const otherMemberName = 'Other Member';
const email = 'memberauthregister@acceptance.test';
const password = 'Umbraco9Rocks!';
const documentTypeName = 'Member Auth Register Page Type';
const documentName = 'Member Auth Register Page';
const templateName = 'Member Auth Register Template';

test.describe('Member registration', () => {
  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.template.ensureNameNotExists(templateName);
    await umbracoApi.member.ensureNameNotExists(memberName);
    await umbracoApi.member.ensureNameNotExists(otherMemberName);
  });

  test.afterEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.template.ensureNameNotExists(templateName);
    await umbracoApi.member.ensureNameNotExists(memberName);
    await umbracoApi.member.ensureNameNotExists(otherMemberName);
    await umbracoUi.memberAuthentication.clearMemberAuthCookie();
  });

  test('anonymous user can register and is automatically logged in', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const templateId = await umbracoApi.template.createMemberRegistrationTemplate(templateName);
    const url = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
    await umbracoUi.memberAuthentication.navigateToPage(url);
    await umbracoUi.memberAuthentication.expectAnonymous();

    // Act
    await umbracoUi.memberAuthentication.fillRegisterForm(memberName, email, password);
    await umbracoUi.memberAuthentication.submitRegisterForm();

    // Assert — expectAuthenticated matches the email because UsernameIsEmail = true, so
    // UserName (and therefore the principal's Name) is the submitted email.
    await umbracoUi.memberAuthentication.expectRegisterSuccess();
    expect(await umbracoApi.member.doesNameExist(memberName)).toBeTruthy();
    await umbracoUi.memberAuthentication.expectAuthenticated(email);
  });

  test('cannot register when password and confirm password do not match', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const templateId = await umbracoApi.template.createMemberRegistrationTemplate(templateName);
    const url = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
    await umbracoUi.memberAuthentication.navigateToPage(url);

    // Act
    await umbracoUi.memberAuthentication.fillRegisterForm(memberName, email, password, 'somethingDifferent!1');
    await umbracoUi.memberAuthentication.submitRegisterForm();

    // Assert
    await umbracoUi.memberAuthentication.expectAnonymous();
    await umbracoUi.memberAuthentication.expectValidationError('do not match');
    expect(await umbracoApi.member.doesNameExist(memberName)).toBeFalsy();
  });

  test('cannot register a duplicate email', async ({umbracoApi, umbracoUi}) => {
    // Arrange — first registration succeeds
    const templateId = await umbracoApi.template.createMemberRegistrationTemplate(templateName);
    const url = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
    await umbracoUi.memberAuthentication.navigateToPage(url);
    await umbracoUi.memberAuthentication.fillRegisterForm(memberName, email, password);
    await umbracoUi.memberAuthentication.submitRegisterForm();
    await umbracoUi.memberAuthentication.expectAuthenticated(email);

    // Sign the auto-logged-in member out so the second registration runs anonymously.
    await umbracoUi.memberAuthentication.clearMemberAuthCookie();
    await umbracoUi.memberAuthentication.navigateToPage(url);
    await umbracoUi.memberAuthentication.expectAnonymous();

    // Act — second registration with same email should fail
    await umbracoUi.memberAuthentication.fillRegisterForm(otherMemberName, email, password);
    await umbracoUi.memberAuthentication.submitRegisterForm();

    // Assert — ASP.NET Identity's DuplicateUserName error ("Username 'xxx' is already taken").
    await umbracoUi.memberAuthentication.expectAnonymous();
    await umbracoUi.memberAuthentication.expectValidationError('is already taken');
    expect(await umbracoApi.member.doesNameExist(otherMemberName)).toBeFalsy();
  });
});
