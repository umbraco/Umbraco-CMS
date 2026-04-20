import {test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const memberName = 'Test Register Member';
const otherMemberName = 'Test Register Member Other';
const email = 'testregistermember@acceptance.test';
const password = '0123456789';
const documentTypeName = 'Test Register Page Type';
const documentName = 'Test Register Page';
const templateName = 'Test Register Template';
let url = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  const templateId = await umbracoApi.template.createMemberRegistrationTemplate(templateName);
  const documentId = await umbracoApi.document.createPublishedDocumentForTemplate(documentName, documentTypeName, templateId);
  url = await umbracoApi.document.getDocumentUrl(documentId);
  await umbracoUi.contentRender.navigateToRenderedContentPage(url);
  await umbracoUi.memberAuthentication.isNotLoggedIn();
});

test.afterEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.member.ensureNameNotExists(memberName);
  await umbracoApi.member.ensureNameNotExists(otherMemberName);
  await umbracoUi.memberAuthentication.clearMemberAuthCookie();
});

test('anonymous user can register and is automatically logged in', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.memberAuthentication.fillRegisterForm(memberName, email, password);
  await umbracoUi.memberAuthentication.submitRegisterForm();

  // Assert
  await umbracoUi.memberAuthentication.doesRegisterSuccessShow();
  expect(await umbracoApi.member.doesNameExist(memberName)).toBeTruthy();
  await umbracoUi.memberAuthentication.isAuthenticated(email);
});

test('cannot register when password and confirm password do not match', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.memberAuthentication.fillRegisterForm(memberName, email, password, 'somethingDifferent!1');
  await umbracoUi.memberAuthentication.submitRegisterForm();

  // Assert
  await umbracoUi.memberAuthentication.isNotLoggedIn();
  await umbracoUi.memberAuthentication.doesValidationErrorShow('do not match');
  expect(await umbracoApi.member.doesNameExist(memberName)).toBeFalsy();
});

test('cannot register a duplicate email', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.memberAuthentication.fillRegisterForm(memberName, email, password);
  await umbracoUi.memberAuthentication.submitRegisterForm();
  await umbracoUi.memberAuthentication.isAuthenticated(email);
  await umbracoUi.memberAuthentication.clearMemberAuthCookie();
  await umbracoUi.contentRender.navigateToRenderedContentPage(url);
  await umbracoUi.memberAuthentication.isNotLoggedIn();

  // Act
  await umbracoUi.memberAuthentication.fillRegisterForm(otherMemberName, email, password);
  await umbracoUi.memberAuthentication.submitRegisterForm();

  // Assert
  await umbracoUi.memberAuthentication.isNotLoggedIn();
  await umbracoUi.memberAuthentication.doesValidationErrorShow('is already taken');
  expect(await umbracoApi.member.doesNameExist(otherMemberName)).toBeFalsy();
});
