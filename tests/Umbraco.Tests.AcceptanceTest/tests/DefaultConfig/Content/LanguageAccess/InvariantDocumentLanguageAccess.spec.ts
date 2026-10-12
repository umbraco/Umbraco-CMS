import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// A document type that does not vary by culture has no shared data to protect: a user with restricted language access
// can edit and publish it whether or not the user can edit shared data.
const testUser = ConstantHelper.testUserCredentials;
const userGroupName = 'InvariantDocumentLanguageAccessUserGroup';
const documentTypeName = 'InvariantDocumentLanguageAccessDocType';
const documentName = 'InvariantDocumentLanguageAccessDoc';
const textStringDataTypeName = 'Textstring';
const originalValue = 'OriginalValue';
const newValue = 'NewValue';
const danishIsoCode = 'da';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  await umbracoApi.language.createDanishLanguage();
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, textStringDataTypeName, textStringDataType.id);
  await umbracoApi.document.createDocumentWithTextContent(documentName, documentTypeId, originalValue, textStringDataTypeName);
  const userGroupId = await umbracoApi.userGroup.createUserGroupWithLanguages(userGroupName, [danishIsoCode], false);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.loginToAdminUser();
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('user with access to Danish only who cannot edit shared data can edit the name and properties', async ({umbracoUi}) => {
  // Act
  await umbracoUi.content.goToContentWithName(documentName);

  // Assert
  await umbracoUi.content.isDocumentNameInputEditable(true);
  await umbracoUi.content.isDocumentPropertyEditable(textStringDataTypeName, true);
});

test('user with access to Danish only who cannot edit shared data can save and publish', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.goToContentWithName(documentName);
  await umbracoUi.content.enterTextstring(newValue);

  // Act
  await umbracoUi.content.clickSaveAndPublishButtonAndWaitForContentToBePublished();

  // Assert
  await umbracoApi.loginToAdminUser();
  const documentId = (await umbracoApi.document.getByName(documentName)).id;
  expect(await umbracoApi.document.isDocumentPublished(documentId)).toBeTruthy();
  const published = await umbracoApi.document.getPublished(documentId);
  expect(published.values[0].value).toBe(newValue);
});
