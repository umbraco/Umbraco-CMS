import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const memberTypeName = 'Member Auth Profile Type';
const memberName = 'Member Auth Profile Test';
const updatedMemberName = 'Updated Member Name';
const username = 'memberauthprofile';
const email = 'memberauthprofile@acceptance.test';
const password = 'Umbraco9Rocks!';
const documentTypeName = 'Member Auth Profile Page Type';
const documentName = 'Member Auth Profile Page';
const templateName = 'Member Auth Profile Template';

test.describe('Member profile', () => {
  test.beforeEach(async ({umbracoApi}) => {
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.template.ensureNameNotExists(templateName);
    await umbracoApi.member.ensureNameNotExists(memberName);
    await umbracoApi.member.ensureNameNotExists(updatedMemberName);
    await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
  });

  test.afterEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.template.ensureNameNotExists(templateName);
    await umbracoApi.member.ensureNameNotExists(memberName);
    await umbracoApi.member.ensureNameNotExists(updatedMemberName);
    await umbracoApi.memberType.ensureNameNotExists(memberTypeName);
    await umbracoUi.memberAuthentication.clearMemberAuthCookie();
  });

  test('logged-in member can update their display name', async ({umbracoApi, umbracoUi}) => {
    // Arrange — log in first; createMemberProfileTemplate bundles the login form for this purpose.
    const memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
    const memberId = await umbracoApi.member.createApprovedMember(memberName, memberTypeId, email, username, password);
    const templateId = await umbracoApi.template.createMemberProfileTemplate(templateName);
    const url = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
    await umbracoUi.memberAuthentication.navigateToPage(url);
    await umbracoUi.memberAuthentication.fillLoginForm(username, password);
    await umbracoUi.memberAuthentication.submitLoginForm();
    await umbracoUi.memberAuthentication.expectAuthenticated(username);

    // Act
    const updatedEmail = 'memberauthprofile-updated@acceptance.test';
    await umbracoUi.memberAuthentication.fillProfileForm(updatedMemberName, updatedEmail);
    await umbracoUi.memberAuthentication.submitProfileForm();

    // Assert
    await umbracoUi.memberAuthentication.expectProfileSuccess();
    const memberData = await umbracoApi.member.get(memberId);
    expect(memberData.email).toBe(updatedEmail);
    expect(memberData.variants[0].name).toBe(updatedMemberName);
  });

  test('anonymous user cannot see the profile form', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
    await umbracoApi.member.createApprovedMember(memberName, memberTypeId, email, username, password);
    const templateId = await umbracoApi.template.createMemberProfileTemplate(templateName);
    const url = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
    await umbracoUi.memberAuthentication.navigateToPage(url);
    await umbracoUi.memberAuthentication.expectAnonymous();

    // Assert
    await umbracoUi.memberAuthentication.expectProfileFormHidden();
  });
});
