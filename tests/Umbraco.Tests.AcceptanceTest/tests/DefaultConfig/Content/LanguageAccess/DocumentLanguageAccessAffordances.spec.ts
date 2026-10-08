import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// How the backoffice signals and applies a user's language access in the document editor: the read-only tags,
// the languages offered when saving or publishing, and that an allowed language can still be saved.
// The tests do not depend on whether the user can edit shared data.
const testUser = ConstantHelper.testUserCredentials;
const userGroupName = 'LanguageAccessAffordancesUserGroup';
const documentTypeName = 'LanguageAccessAffordancesDocType';
const varyingPropertyName = 'VaryingText';
const sharedPropertyName = 'SharedText';
const textStringDataTypeName = 'Textstring';
let textStringDataTypeId: string;
// Cultures
const english = {isoCode: 'en-US', name: 'English (United States)', documentName: 'LanguageAccessAffordancesDoc EN'};
const danish = {isoCode: 'da', name: 'Danish', documentName: 'LanguageAccessAffordancesDoc DA'};
const vietnamese = {isoCode: 'vi', name: 'Vietnamese', documentName: 'LanguageAccessAffordancesDoc VI'};
const languages = [english, danish, vietnamese];

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.language.ensureIsoCodeNotExists(danish.isoCode);
  await umbracoApi.language.ensureIsoCodeNotExists(vietnamese.isoCode);
  await umbracoApi.language.createDanishLanguage();
  await umbracoApi.language.createVietnameseLanguage();
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringDataTypeId = textStringDataType.id;
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithVaryingAndSharedProperties(
    documentTypeName, textStringDataTypeId, varyingPropertyName, sharedPropertyName);
  await umbracoApi.document.createDocumentWithMultipleVariantsAndVaryingAndSharedValues(
    english.documentName, documentTypeId, varyingPropertyName, sharedPropertyName,
    languages.map(x => ({isoCode: x.isoCode, name: x.documentName, value: `${x.isoCode}Value`})), 'SharedValue');
  // A user who can only edit Danish. The app language starts on the default language, which the user cannot edit.
  const userGroupId = await umbracoApi.userGroup.createUserGroupWithLanguages(userGroupName, [danish.isoCode]);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
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

// The save dialog and the publish dialog use different picker elements.
async function expectOnlyDanishToBeSelectable(page) {
  const items = page.locator('umb-document-variant-language-picker, umb-content-variant-language-picker').locator('uui-menu-item');
  await expect(items).toHaveCount(languages.length);
  await expect(items.filter({hasText: danish.name})).not.toHaveAttribute('disabled');
  await expect(items.filter({hasText: danish.name})).toHaveAttribute('selected');
  for (const language of [english, vietnamese]) {
    await expect(items.filter({hasText: language.name})).toHaveAttribute('disabled');
    await expect(items.filter({hasText: language.name})).not.toHaveAttribute('selected');
  }
}

test('tags the app language as read-only while it is a language the user cannot edit', async ({umbracoUi}) => {
  // Assert
  await umbracoUi.content.isAppLanguageReadOnly(true);

  // Act
  await umbracoUi.content.changeDocumentSectionLanguage(danish.name);

  // Assert
  await umbracoUi.content.doesDocumentSectionHaveLanguageSelected(danish.name);
  await umbracoUi.content.isAppLanguageReadOnly(false);
});

test('tags a variant the user cannot edit as read-only', async ({umbracoUi}) => {
  // Act
  await umbracoUi.content.goToContentWithName(english.documentName);

  // Assert
  await umbracoUi.content.isDocumentReadOnly(true);
});

test('does not tag a variant the user can edit as read-only', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.content.changeDocumentSectionLanguage(danish.name);

  // Act
  await umbracoUi.content.goToContentWithName(danish.documentName);

  // Assert
  await umbracoUi.content.isDocumentReadOnly(false);
});

test('only lets the user select the language they can edit when saving', async ({page, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.changeDocumentSectionLanguage(danish.name);
  await umbracoUi.content.goToContentWithName(danish.documentName);

  // Act
  await umbracoUi.content.clickSaveButtonForContent();

  // Assert
  await expectOnlyDanishToBeSelectable(page);
});

test('only lets the user select the language they can edit when saving and publishing', {tag: '@smoke'}, async ({page, umbracoUi}) => {
  // Arrange
  await umbracoUi.content.changeDocumentSectionLanguage(danish.name);
  await umbracoUi.content.goToContentWithName(danish.documentName);

  // Act
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await expectOnlyDanishToBeSelectable(page);
});

test('saves a change to the language the user can edit and leaves the other languages and the shared value untouched', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const newDanishValue = 'UpdatedDanishValue';
  await umbracoUi.content.changeDocumentSectionLanguage(danish.name);
  await umbracoUi.content.goToContentWithName(danish.documentName);
  await page.locator('umb-document-workspace-editor umb-property').filter({hasText: varyingPropertyName}).locator('#input').fill(newDanishValue);

  // Act
  await umbracoUi.content.clickSaveButtonForContent();
  await umbracoUi.content.clickContainerSaveButtonAndWaitForContentToBeUpdated();

  // Assert
  await umbracoApi.loginToAdminUser();
  const document = await umbracoApi.document.getByName(english.documentName);
  const valueOf = (alias: string, culture: string | null) =>
    document.values.find((x) => x.alias === AliasHelper.toAlias(alias) && x.culture === culture)?.value;
  expect(valueOf(varyingPropertyName, danish.isoCode)).toBe(newDanishValue);
  expect(valueOf(varyingPropertyName, english.isoCode)).toBe(`${english.isoCode}Value`);
  expect(valueOf(varyingPropertyName, vietnamese.isoCode)).toBe(`${vietnamese.isoCode}Value`);
  expect(valueOf(sharedPropertyName, null)).toBe('SharedValue');
});
