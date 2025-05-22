import {AliasHelper, ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';

const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

const userGroupName = 'TestUserGroup';
let userGroupId = null;

const documentTypeName = 'TestDocumentType';
const documentName = 'TestDocument';
const englishDocumentName = 'EnglishDocument';
const danishDocumentName = 'DanishDocument';
const vietnameseDocumentName = 'VietnameseDocument';
let documentTypeId = null;

const dataTypeName = 'Textstring';
let dataTypeId = null;

const englishIsoCode = 'en-US';
const danishIsoCode = 'da';
const vietnameseIsoCode = 'vi';
const englishLanguageName = 'English (United States)';
const danishLanguageName = 'Danish';
const vietnameseLanguageName = 'Vietnamese';
const cultureVariants = [
  {
    isoCode: englishIsoCode,
    name: englishDocumentName,
    value: 'EnglishValue',
  },
  {
    isoCode: danishIsoCode,
    name: danishDocumentName,
    value: 'DanishValue',
  },
  {
    isoCode: vietnameseIsoCode,
    name: vietnameseDocumentName,
    value: 'VietnameseValue',
  }
];

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  await umbracoApi.language.ensureIsoCodeNotExists(vietnameseIsoCode);
  await umbracoApi.language.createDanishLanguage();
  await umbracoApi.language.createVietnameseLanguage();
  const dataType = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeId = dataType.id;
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId, 'TestGroup', true, true);
  await umbracoApi.document.createDocumentWithMultipleVariants(documentName, documentTypeId, AliasHelper.toAlias(dataTypeName), cultureVariants);
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  await umbracoApi.language.ensureIsoCodeNotExists(vietnameseIsoCode);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.fixme('can rename content with language set in userGroup', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedContentName = 'UpdatedContentName';
  userGroupId = await umbracoApi.userGroup.createUserGroupWithLanguageAndContentSection(userGroupName, englishIsoCode);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId, [], false, [], false, englishIsoCode);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(englishDocumentName);

  // Act
  await umbracoUi.content.isDocumentReadOnly(false);
  await umbracoUi.content.enterContentName(updatedContentName);
  // Fix this later. Currently the "Save" button changed to "Save..." button
  await umbracoUi.content.clickSaveButton();
  await umbracoUi.content.clickSaveAndCloseButton();

  // Assert
  //await umbracoUi.userGroup.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.userGroup.isErrorNotificationVisible(false);
  await umbracoUi.content.isContentInTreeVisible(updatedContentName);
});

test('can not rename content with language not set in userGroup', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithLanguageAndContentSection(userGroupName, englishIsoCode);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId, [], false, [], false, englishIsoCode);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.doesDocumentSectionHaveLanguageSelected(englishLanguageName);
  await umbracoUi.content.changeDocumentSectionLanguage(danishLanguageName);

  // Act
  await umbracoUi.content.goToContentWithName(danishDocumentName);

  // Assert
  await umbracoUi.content.isDocumentReadOnly();
  await umbracoUi.content.isDocumentNameInputEditable(false);
});

test('can update content property with language set in userGroup', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithLanguageAndContentSection(userGroupName, englishIsoCode);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.doesDocumentSectionHaveLanguageSelected(englishLanguageName);

  // Act
  await umbracoUi.content.goToContentWithName(englishDocumentName);

  // Assert
  await umbracoUi.content.isDocumentPropertyEditable(dataTypeName, true);
});

test('can not update content property with language not set in userGroup', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  userGroupId = await umbracoApi.userGroup.createUserGroupWithLanguageAndContentSection(userGroupName, englishIsoCode);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.userGroup.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.doesDocumentSectionHaveLanguageSelected(englishLanguageName);
  await umbracoUi.content.changeDocumentSectionLanguage(vietnameseLanguageName);

  // Act
  await umbracoUi.content.goToContentWithName(vietnameseDocumentName);

  // Assert
  await umbracoUi.content.isDocumentPropertyEditable(dataTypeName, false);
});
