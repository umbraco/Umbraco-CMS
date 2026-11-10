import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// What a language-restricted user can edit, save and publish on a culture-varying element:
//  - Culture-varying properties and the name are editable only on a variant the user has access to.
//  - Shared (non culture-varying) properties are editable on every variant when the user can edit shared data,
//    and read-only otherwise.
//  - Saving and publishing only ever offer, and only ever change, the variants the user has access to.
const testUser = ConstantHelper.testUserCredentials;
const userGroupName = 'ElementLanguageAccessUserGroup';
const elementTypeName = 'ElementLanguageAccessElementType';
const varyingPropertyName = 'VaryingText';
const sharedPropertyName = 'SharedText';
const textStringDataTypeName = 'Textstring';
const originalSharedValue = 'SharedValue';
// Cultures
const english = {isoCode: 'en-US', name: 'English (United States)', variantName: 'English', elementName: 'ElementLanguageAccess EN'};
const danish = {isoCode: 'da', name: 'Danish', variantName: 'Danish', elementName: 'ElementLanguageAccess DA'};
const languages = [english, danish];

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.element.ensureNameNotExists(english.elementName);
  await umbracoApi.language.ensureIsoCodeNotExists(danish.isoCode);
  await umbracoApi.language.createDanishLanguage();
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const elementTypeId = await umbracoApi.documentType.createVariantElementTypeWithVariantAndInvariantProperty(
    elementTypeName, 'TestGroup', varyingPropertyName, sharedPropertyName, textStringDataType.id);
  await umbracoApi.element.createElementWithMultipleVariantsAndVaryingAndSharedValues(
    english.elementName, elementTypeId, varyingPropertyName, sharedPropertyName,
    languages.map(x => ({isoCode: x.isoCode, name: x.elementName, value: `${x.isoCode}Value`})), originalSharedValue);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.loginToAdminUser();
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.element.ensureNameNotExists(english.elementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danish.isoCode);
});

async function openElementAs(umbracoApi, umbracoUi, isoCodes: string[], canEditSharedData: boolean, variant: typeof languages[number]) {
  const userGroupId = await umbracoApi.userGroup.createUserGroupWithLanguagesAndElementAccess(userGroupName, isoCodes, canEditSharedData);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library, false);
  await umbracoUi.library.goToElementWithName(english.elementName);
  if (variant !== english) {
    await umbracoUi.library.switchLanguage(variant.variantName);
  }
}

const propertyInput = (page, propertyName: string) =>
  page.locator('umb-element-workspace-editor umb-property').filter({hasText: propertyName}).locator('#input');

const valueOf = (element, propertyName: string, culture: string | null) =>
  element.values.find((x) => x.alias === AliasHelper.toAlias(propertyName) && x.culture === culture)?.value;

const stateOf = (element, culture: string) => element.variants.find((x) => x.culture === culture)?.state;

// The save and publish dialogs use different picker elements.
async function expectOnlyDanishToBeSelectable(page) {
  const items = page.locator('umb-element-variant-language-picker, umb-content-variant-language-picker').locator('uui-menu-item');
  await expect(items).toHaveCount(languages.length);
  await expect(items.filter({hasText: danish.name})).not.toHaveAttribute('disabled');
  await expect(items.filter({hasText: english.name})).toHaveAttribute('disabled');
}

for (const userCanEditSharedData of [true, false]) {
  const sharedLabel = userCanEditSharedData ? 'can edit' : 'cannot edit';

  test(`user with access to Danish only can edit the culture-varying property and the name only in the Danish variant, and ${sharedLabel} the shared property in every variant`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await openElementAs(umbracoApi, umbracoUi, [danish.isoCode], userCanEditSharedData, english);

    for (const variant of languages) {
      // Act
      if (variant !== english) {
        await umbracoUi.library.switchLanguage(variant.variantName);
      }
      await umbracoUi.library.doesElementNameInputHaveValue(variant.elementName);

      // Assert
      const hasAccess = variant === danish;
      await umbracoUi.library.isElementNameInputEditable(hasAccess);
      await umbracoUi.library.isElementPropertyEditable(varyingPropertyName, hasAccess);
      await umbracoUi.library.isElementPropertyEditable(sharedPropertyName, userCanEditSharedData);
    }
  });
}

test('user with access to Danish only who cannot edit shared data only saves Danish', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const newDanishValue = 'NewDanishValue';
  await openElementAs(umbracoApi, umbracoUi, [danish.isoCode], false, danish);
  await propertyInput(page, varyingPropertyName).fill(newDanishValue);

  // Act
  await umbracoUi.library.clickSaveButtonForElement();
  await expectOnlyDanishToBeSelectable(page);
  await umbracoUi.library.clickSaveModalButtonAndWaitForElementToBeUpdated();

  // Assert
  await umbracoApi.loginToAdminUser();
  const element = await umbracoApi.element.getByName(english.elementName);
  expect(valueOf(element, varyingPropertyName, danish.isoCode)).toBe(newDanishValue);
  expect(valueOf(element, varyingPropertyName, english.isoCode)).toBe(`${english.isoCode}Value`);
  expect(valueOf(element, sharedPropertyName, null)).toBe(originalSharedValue);
});

test('user with access to Danish only who can edit shared data publishes Danish with the changed shared data and leaves English unpublished', async ({page, umbracoApi, umbracoUi}) => {
  // Arrange
  const newSharedValue = 'NewSharedValue';
  await openElementAs(umbracoApi, umbracoUi, [danish.isoCode], true, danish);
  await propertyInput(page, sharedPropertyName).fill(newSharedValue);

  // Act
  await umbracoUi.library.clickSaveAndPublishButton();
  await expectOnlyDanishToBeSelectable(page);
  await umbracoUi.library.clickConfirmToPublishButtonAndWaitForElementToBePublished();

  // Assert
  await umbracoApi.loginToAdminUser();
  const element = await umbracoApi.element.getByName(english.elementName);
  expect(stateOf(element, danish.isoCode)).toBe('Published');
  expect(stateOf(element, english.isoCode)).not.toBe('Published');
  expect(valueOf(element, sharedPropertyName, null)).toBe(newSharedValue);
});
