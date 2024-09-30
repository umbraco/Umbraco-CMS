import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';

const testUser = {
  name: 'Test User',
  email: 'verySecureEmail@123.test',
  password: 'verySecurePassword123',
}

const rootDocumentTypeName = 'RootDocumentType';
const childDocumentTypeOneName = 'ChildDocumentTypeOne';
const childDocumentTypeTwoName = 'ChildDocumentTypeTwo';
let childDocumentTypeOneId = null;
let childDocumentTypeTwoId = null;
let rootDocumentTypeId = null

const rootDocumentName = 'RootDocument';
const childDocumentOneName = 'ChildDocumentOne';
const childDocumentTwoName = 'ChildDocumentTwo';

let userGroupId = null;

test.beforeEach(async ({umbracoUi, umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeOneName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeTwoName);

  childDocumentTypeOneId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeOneName);
  childDocumentTypeTwoId = await umbracoApi.documentType.createDefaultDocumentType(childDocumentTypeTwoName);
  rootDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedTwoChildNodes(rootDocumentTypeName, childDocumentTypeOneId, childDocumentTypeTwoId);

  var rootDocumentId = await umbracoApi.document.createDefaultDocument(rootDocumentName, rootDocumentTypeId);
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentOneName, childDocumentTypeOneId, rootDocumentId);
  await umbracoApi.document.createDefaultDocumentWithParent(childDocumentTwoName, childDocumentTypeTwoId, rootDocumentId);

  // Should be empty, is supposed to be tested in userGroups
  userGroupId = await umbracoApi.userGroup.createUserGroupWithDocumentAccessAndStartNode('Content Editor', rootDocumentId );

  await umbracoApi.user.setUserSettingsToDefault(testUser.name, testUser.email, testUser.password, userGroupId);

  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeOneName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeTwoName);
});

test('test permissions', {tag: '@smoke'}, async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.user.goToSection(ConstantHelper.sections.users);

  await page.pause()
  // Act
  await umbracoUi.user.clickCreateButton();

});

// SETUP:
// RootDocumentType
// ChildDocumentTypeOne
// ChildDocumentTypeTwo
