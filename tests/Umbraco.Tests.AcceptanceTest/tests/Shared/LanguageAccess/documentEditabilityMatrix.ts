import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// What a document editor can edit, per user language access and per variant, on a document type that varies by
// culture and segment:
//  - Culture-varying properties, with or without segments, and the name are editable only on a variant the user has
//    access to.
//  - Shared properties (not culture-varying, with or without segments) are editable on every variant, including
//    variants the user has no language access to, when the user can edit shared data, and read-only otherwise.
// The tests are registered twice, by a spec where the user can edit shared data and a spec where the user cannot.

const testUser = ConstantHelper.testUserCredentials;
const userGroupName = 'LanguageAccessMatrixUserGroup';
const documentTypeName = 'LanguageAccessMatrixDocType';
const textStringDataTypeName = 'Textstring';
const properties = [
  {name: 'VaryingText', variesByCulture: true, variesBySegment: false},
  {name: 'SharedText', variesByCulture: false, variesBySegment: false},
  {name: 'SegmentOnlyText', variesByCulture: false, variesBySegment: true},
  {name: 'CultureAndSegmentText', variesByCulture: true, variesBySegment: true},
];

type Language = {isoCode: string, name: string, documentName: string, isDefault: boolean};
const english: Language = {isoCode: 'en-US', name: 'English (United States)', documentName: 'LanguageAccessMatrixDoc EN', isDefault: true};
const danish: Language = {isoCode: 'da', name: 'Danish', documentName: 'LanguageAccessMatrixDoc DA', isDefault: false};
const vietnamese: Language = {isoCode: 'vi', name: 'Vietnamese', documentName: 'LanguageAccessMatrixDoc VI', isDefault: false};
const languages = [english, danish, vietnamese];

// isSmoke: the user whose test also runs in the pull request smoke tests.
const users: {description: string, allowedIsoCodes: string[] | 'all', isSmoke?: boolean}[] = [
  {description: 'all languages', allowedIsoCodes: 'all'},
  {description: 'the default language only', allowedIsoCodes: [english.isoCode]},
  {description: 'Danish only', allowedIsoCodes: [danish.isoCode], isSmoke: true},
  {description: 'the default language and Danish', allowedIsoCodes: [english.isoCode, danish.isoCode]},
  {description: 'no languages', allowedIsoCodes: []},
];

const hasAccess = (allowedIsoCodes: string[] | 'all', language: Language) =>
  allowedIsoCodes === 'all' || allowedIsoCodes.includes(language.isoCode);

function describeCultureAccess(allowedIsoCodes: string[] | 'all') {
  const accessible = languages.filter(x => hasAccess(allowedIsoCodes, x));
  if (accessible.length === languages.length) {
    return 'every variant';
  }
  return accessible.length === 0 ? 'no variant' : `only the ${accessible.map(x => x.name).join(' and ')} variant`;
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
      const sharedLabel = userCanEditSharedData ? 'can edit' : 'cannot edit';

      test(`user with access to ${user.description} can edit culture-varying properties and the name in ${describeCultureAccess(user.allowedIsoCodes)}, and ${sharedLabel} shared properties in every variant`, {tag: user.isSmoke ? ['@smoke'] : []}, async ({umbracoApi, umbracoUi}) => {
        // Arrange
        const documentTypeId = await umbracoApi.documentType.createCultureAndSegmentVariantDocumentTypeWithProperties(
          documentTypeName, textStringDataTypeId, properties);
        const values = properties.flatMap(property => property.variesByCulture
          ? languages.map(language => ({propertyName: property.name, culture: language.isoCode, value: `${language.isoCode}Value`}))
          : [{propertyName: property.name, culture: null, value: 'SharedValue'}]);
        await umbracoApi.document.createDocumentWithMultipleVariantsAndTextValues(
          english.documentName, documentTypeId, languages.map(x => ({isoCode: x.isoCode, name: x.documentName})), values);
        const userGroupId = user.allowedIsoCodes === 'all'
          ? await umbracoApi.userGroup.createUserGroupWithAllLanguages(userGroupName, userCanEditSharedData)
          : await umbracoApi.userGroup.createUserGroupWithLanguages(userGroupName, user.allowedIsoCodes, userCanEditSharedData);
        await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
        await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
        await umbracoUi.goToBackOffice();
        await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);

        for (const language of languages) {
          // Act
          await umbracoUi.content.changeDocumentSectionLanguage(language.name);
          await umbracoUi.content.goToContentWithName(language.documentName);
          await umbracoUi.content.doesDocumentNameInputHaveValue(language.documentName);

          // Assert
          const canEditCulture = hasAccess(user.allowedIsoCodes, language);
          await umbracoUi.content.isDocumentNameInputEditable(canEditCulture);
          for (const property of properties) {
            await umbracoUi.content.isDocumentPropertyEditable(property.name, property.variesByCulture ? canEditCulture : userCanEditSharedData);
          }
        }
      });
    }
  });
}
