import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// What a document editor can edit, per user language access and per variant tab:
//  - Culture-varying properties and the name are editable only on a variant tab the user has access to.
//  - Shared (non culture-varying) properties are editable on every variant tab, including tabs the user has no
//    language access to, when the user can edit shared data, and read-only otherwise.
// The tests are registered twice, by a spec where the user can edit shared data and a spec where the user cannot.

const testUser = ConstantHelper.testUserCredentials;
const userGroupName = 'LanguageAccessMatrixUserGroup';
const documentTypeName = 'LanguageAccessMatrixDocType';
const varyingPropertyName = 'VaryingText';
const sharedPropertyName = 'SharedText';
const textStringDataTypeName = 'Textstring';

type Language = {isoCode: string, name: string, documentName: string, isDefault: boolean};
const english: Language = {isoCode: 'en-US', name: 'English (United States)', documentName: 'LanguageAccessMatrixDoc EN', isDefault: true};
const danish: Language = {isoCode: 'da', name: 'Danish', documentName: 'LanguageAccessMatrixDoc DA', isDefault: false};
const vietnamese: Language = {isoCode: 'vi', name: 'Vietnamese', documentName: 'LanguageAccessMatrixDoc VI', isDefault: false};
const languages = [english, danish, vietnamese];

const users: {description: string, allowedIsoCodes: string[] | 'all'}[] = [
  {description: 'all languages', allowedIsoCodes: 'all'},
  {description: 'the default language only', allowedIsoCodes: [english.isoCode]},
  {description: 'Danish only', allowedIsoCodes: [danish.isoCode]},
  {description: 'the default language and Danish', allowedIsoCodes: [english.isoCode, danish.isoCode]},
];

function expectedEditability(allowedIsoCodes: string[] | 'all', tab: Language, userCanEditSharedData: boolean) {
  const hasAccess = allowedIsoCodes === 'all' || allowedIsoCodes.includes(tab.isoCode);
  return {
    varying: hasAccess,
    shared: userCanEditSharedData,
    name: hasAccess,
  };
}

export function registerDocumentEditabilityTests(userCanEditSharedData: boolean) {
  test.describe(`user ${userCanEditSharedData ? 'can' : 'cannot'} edit shared data`, () => {
    let textStringDataTypeId: string;

    test.beforeEach(async ({umbracoApi}) => {
      await umbracoApi.user.ensureNameNotExists(testUser.name);
      await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
      for (const language of languages.filter(x => !x.isDefault)) {
        await umbracoApi.language.ensureIsoCodeNotExists(language.isoCode);
      }
      await umbracoApi.language.createDanishLanguage();
      await umbracoApi.language.createVietnameseLanguage();
      const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
      textStringDataTypeId = textStringDataType.id;
    });

    test.afterEach(async ({umbracoApi}) => {
      await umbracoApi.loginToAdminUser();
      await umbracoApi.user.ensureNameNotExists(testUser.name);
      await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
      for (const language of languages) {
        await umbracoApi.document.ensureNameNotExists(language.documentName);
      }
      await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
      for (const language of languages.filter(x => !x.isDefault)) {
        await umbracoApi.language.ensureIsoCodeNotExists(language.isoCode);
      }
    });

    for (const user of users) {
      test.describe(`user with access to ${user.description}`, () => {
        for (const tab of languages) {
          const expected = expectedEditability(user.allowedIsoCodes, tab, userCanEditSharedData);
          const describe = (editable: boolean) => editable ? 'can edit' : 'cannot edit';

          test(`${describe(expected.varying)} the culture-varying property, ${describe(expected.shared)} the shared property and ${describe(expected.name)} the name in the ${tab.name} variant`, async ({umbracoApi, umbracoUi}) => {
            // Arrange
            const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithVaryingAndSharedProperties(
              documentTypeName, textStringDataTypeId, varyingPropertyName, sharedPropertyName);
            await umbracoApi.document.createDocumentWithMultipleVariantsAndVaryingAndSharedValues(
              english.documentName, documentTypeId, varyingPropertyName, sharedPropertyName,
              languages.map(x => ({isoCode: x.isoCode, name: x.documentName, value: `${x.isoCode}Value`})), 'SharedValue');
            const userGroupId = user.allowedIsoCodes === 'all'
              ? await umbracoApi.userGroup.createUserGroupWithAllLanguages(userGroupName, userCanEditSharedData)
              : await umbracoApi.userGroup.createUserGroupWithLanguages(userGroupName, user.allowedIsoCodes, userCanEditSharedData);
            await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
            await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
            await umbracoUi.goToBackOffice();
            await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
            await umbracoUi.content.changeDocumentSectionLanguage(tab.name);

            // Act
            await umbracoUi.content.goToContentWithName(tab.documentName);

            // Assert
            await umbracoUi.content.isDocumentNameInputEditable(expected.name);
            await umbracoUi.content.isDocumentPropertyEditable(varyingPropertyName, expected.varying);
            await umbracoUi.content.isDocumentPropertyEditable(sharedPropertyName, expected.shared);
          });
        }
      });
    }
  });
}
