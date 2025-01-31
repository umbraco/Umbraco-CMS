import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const testUser = ConstantHelper.testUserCredentials;
let testUserCookieAndToken = {cookie: "", accessToken: "", refreshToken: ""};

let userGroupId = null;

const documentTypeName = 'TestDocumentType';
const documentName = 'TestDocument';
const richTextEditorName = 'TestRichTextEditor';
const stylesheetName = 'TestStylesheet.css';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  const stylesheetPath = await umbracoApi.stylesheet.createStylesheetWithHeaderContent(stylesheetName);
  const dataTypeId = await umbracoApi.dataType.createTinyMCEDataTypeWithStylesheet(richTextEditorName, stylesheetPath);
  await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, richTextEditorName, dataTypeId);
  const userGroup = await umbracoApi.userGroup.getByName('Editors');
  userGroupId = userGroup.id;
});

test.afterEach(async ({umbracoApi}) => {
  // Ensure we are logged in to admin
  await umbracoApi.loginToAdminUser(testUserCookieAndToken.cookie, testUserCookieAndToken.accessToken, testUserCookieAndToken.refreshToken);
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  await umbracoApi.dataType.ensureNameNotExists(richTextEditorName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can create content with a rich text editor that has a stylesheet', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  testUserCookieAndToken = await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

  // Act
  await umbracoUi.content.clickActionsMenuAtRoot();
  await umbracoUi.content.clickCreateButton();
  await umbracoUi.content.chooseDocumentType(documentTypeName);
  await umbracoUi.content.enterContentName(documentName);
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.noAccessToResource, false);
  await umbracoUi.content.clickSaveButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
  expect(await umbracoApi.document.doesNameExist(documentName)).toBeTruthy();
});
