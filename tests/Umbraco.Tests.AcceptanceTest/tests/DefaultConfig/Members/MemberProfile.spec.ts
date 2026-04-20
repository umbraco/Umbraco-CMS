import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const memberTypeName = 'Test Profile Member Type';
const memberName = 'Test Profile Member';
const updatedMemberName = 'Test Profile Member Updated';
const username = 'testprofilemember';
const email = 'testprofilemember@acceptance.test';
const updatedEmail = 'testprofilemember-updated@acceptance.test';
const password = '0123456789';
const documentTypeName = 'Test Profile Page Type';
const documentName = 'Test Profile Page';
const templateName = 'Test Profile Template';
let memberTypeId = '';
let memberId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  memberTypeId = await umbracoApi.memberType.createDefaultMemberType(memberTypeName);
  memberId = await umbracoApi.member.createApprovedMember(memberName, memberTypeId, email, username, password);
  const templateId = await umbracoApi.template.createMemberProfileTemplate(templateName);
  const documentId = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
  const url = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(url);
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
  // Arrange
  await umbracoUi.memberAuthentication.fillLoginForm(username, password);
  await umbracoUi.memberAuthentication.submitLoginForm();
  await umbracoUi.memberAuthentication.isAuthenticated(username);

  // Act
  await umbracoUi.memberAuthentication.fillProfileForm(updatedMemberName, updatedEmail);
  await umbracoUi.memberAuthentication.submitProfileForm();

  // Assert
  await umbracoUi.memberAuthentication.doesProfileSuccessShow();
  const memberData = await umbracoApi.member.get(memberId);
  expect(memberData.email).toBe(updatedEmail);
  expect(memberData.variants[0].name).toBe(updatedMemberName);
});

test('anonymous user cannot see the profile form', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.memberAuthentication.isNotLoggedIn();

  // Assert
  await umbracoUi.memberAuthentication.isProfileFormHidden();
});
