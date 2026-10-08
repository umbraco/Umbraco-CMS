import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// What the variant selector, the Save action and the save and publish dialogs allow, based on the languages the user
// may edit and on whether the user can edit shared data.
const testUser = ConstantHelper.testUserCredentials;
const userGroupName = 'DocumentSaveAndPublishGatingUserGroup';
const documentTypeName = 'DocumentSaveAndPublishGatingDocType';
const varyingPropertyName = 'VaryingText';
const sharedPropertyName = 'SharedText';
const textStringDataTypeName = 'Textstring';
// Cultures
const english = {isoCode: 'en-US', name: 'English (United States)', documentName: 'DocumentSaveAndPublishGatingDoc EN'};
const danish = {isoCode: 'da', name: 'Danish', documentName: 'DocumentSaveAndPublishGatingDoc DA'};
const vietnamese = {isoCode: 'vi', name: 'Vietnamese', documentName: 'DocumentSaveAndPublishGatingDoc VI'};
const languages = [english, danish, vietnamese];

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.language.ensureIsoCodeNotExists(danish.isoCode);
  await umbracoApi.language.ensureIsoCodeNotExists(vietnamese.isoCode);
  await umbracoApi.language.createDanishLanguage();
  await umbracoApi.language.createVietnameseLanguage();
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithVaryingAndSharedProperties(
    documentTypeName, textStringDataType.id, varyingPropertyName, sharedPropertyName);
  await umbracoApi.document.createDocumentWithMultipleVariantsAndVaryingAndSharedValues(
    english.documentName, documentTypeId, varyingPropertyName, sharedPropertyName,
    languages.map(x => ({isoCode: x.isoCode, name: x.documentName, value: `${x.isoCode}Value`})), 'SharedValue');
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.loginToAdminUser();
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  for (const language of languages) {
    await umbracoApi.document.ensureNameNotExists(language.documentName);
  }
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danish.isoCode);
  await umbracoApi.language.ensureIsoCodeNotExists(vietnamese.isoCode);
});

async function loginToContentSectionAs(umbracoApi, umbracoUi, isoCodes: string[], canEditSharedData: boolean) {
  const userGroupId = await umbracoApi.userGroup.createUserGroupWithLanguages(userGroupName, isoCodes, canEditSharedData);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
}

// The save dialog and the publish dialog use different picker elements.
const dialogItems = (page) =>
  page.locator('umb-document-variant-language-picker, umb-content-variant-language-picker').locator('uui-menu-item');

async function expectOnlyDanishToBeSelectable(page) {
  const items = dialogItems(page);
  await expect(items).toHaveCount(languages.length);
  await expect(items.filter({hasText: danish.name})).not.toHaveAttribute('disabled');
  await expect(items.filter({hasText: danish.name})).toHaveAttribute('selected');
  for (const language of [english, vietnamese]) {
    await expect(items.filter({hasText: language.name})).toHaveAttribute('disabled');
    await expect(items.filter({hasText: language.name})).not.toHaveAttribute('selected');
  }
}

test.describe('user who can edit Danish and can edit shared data', () => {
  test('only offers Danish in the save dialog', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await loginToContentSectionAs(umbracoApi, umbracoUi, [danish.isoCode], true);
    await umbracoUi.content.changeDocumentSectionLanguage(danish.name);
    await umbracoUi.content.goToContentWithName(danish.documentName);

    // Act
    await umbracoUi.content.clickSaveButtonForContent();

    // Assert
    await expectOnlyDanishToBeSelectable(page);
  });

  test('only offers Danish in the publish dialog', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await loginToContentSectionAs(umbracoApi, umbracoUi, [danish.isoCode], true);
    await umbracoUi.content.changeDocumentSectionLanguage(danish.name);
    await umbracoUi.content.goToContentWithName(danish.documentName);

    // Act
    await umbracoUi.content.clickSaveAndPublishButton();

    // Assert
    await expectOnlyDanishToBeSelectable(page);
  });

  test('tags the English variant as read-only although its shared data can be edited', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await loginToContentSectionAs(umbracoApi, umbracoUi, [danish.isoCode], true);

    // Act
    await umbracoUi.content.goToContentWithName(english.documentName);

    // Assert
    await umbracoUi.content.isDocumentReadOnly(true);
    await expect(page.locator('umb-document-workspace-editor umb-property').filter({hasText: sharedPropertyName}).locator('#input')).toBeEditable();
  });

  test('does not tag the Danish variant as read-only', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await loginToContentSectionAs(umbracoApi, umbracoUi, [danish.isoCode], true);
    await umbracoUi.content.changeDocumentSectionLanguage(danish.name);

    // Act
    await umbracoUi.content.goToContentWithName(danish.documentName);

    // Assert
    await umbracoUi.content.isDocumentReadOnly(false);
  });
});

test.describe('user who cannot edit any language and can edit shared data', () => {
  test('can save the shared data on its own and leaves every language untouched', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const newSharedValue = 'UpdatedSharedValue';
    await loginToContentSectionAs(umbracoApi, umbracoUi, [], true);
    await umbracoUi.content.goToContentWithName(english.documentName);
    await page.locator('umb-document-workspace-editor umb-property').filter({hasText: sharedPropertyName}).locator('#input').fill(newSharedValue);

    // Act
    const documentUpdated = page.waitForResponse((response) =>
      response.request().method() === 'PUT' && /\/document\/[0-9a-f-]{36}$/.test(response.url()));
    await umbracoUi.content.clickSaveButtonForContent();

    // Assert
    expect((await documentUpdated).ok()).toBeTruthy();
    await umbracoApi.loginToAdminUser();
    const document = await umbracoApi.document.getByName(english.documentName);
    const valueOf = (alias: string, culture: string | null) =>
      document.values.find((x) => x.alias === AliasHelper.toAlias(alias) && x.culture === culture)?.value;
    expect(valueOf(sharedPropertyName, null)).toBe(newSharedValue);
    for (const language of languages) {
      expect(valueOf(varyingPropertyName, language.isoCode)).toBe(`${language.isoCode}Value`);
    }
  });

  test('cannot select any language in the publish dialog', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await loginToContentSectionAs(umbracoApi, umbracoUi, [], true);
    await umbracoUi.content.goToContentWithName(english.documentName);

    // Act
    await umbracoUi.content.clickSaveAndPublishButton();

    // Assert
    const items = dialogItems(page);
    await expect(items).toHaveCount(languages.length);
    for (const language of languages) {
      await expect(items.filter({hasText: language.name})).toHaveAttribute('disabled');
    }
  });
});

test.describe('user who cannot edit any language and cannot edit shared data', () => {
  test('cannot save', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await loginToContentSectionAs(umbracoApi, umbracoUi, [], false);

    // Act
    await umbracoUi.content.goToContentWithName(english.documentName);

    // Assert
    await expect(page.getByTestId('workspace-action:Umb.WorkspaceAction.Document.Save').locator('button')).toBeDisabled();
  });
});
