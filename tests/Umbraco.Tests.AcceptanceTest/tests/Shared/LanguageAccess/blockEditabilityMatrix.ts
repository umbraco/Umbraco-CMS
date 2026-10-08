import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// What a language-restricted user can edit inside nested blocks, over the 18 combinations of which levels of a
// nested block editor vary by culture. The block content is created as an administrator in the default language.
//  - Block data that belongs to a culture is editable only by users with access to that culture.
//  - Shared data is editable on every variant when the user can edit shared data, including variants the user has
//    no language access to. Text is shared data when it is shared and neither block list varies by culture, whether
//    or not the blocks themselves vary by culture.
//  - When the user cannot edit shared data, shared data is read-only unless the block content belongs to a
//    culture-varying block list.
// The tests are shared by the two config projects, which differ only in whether users can edit shared data.

const testUser = ConstantHelper.testUserCredentials;
const userGroupName = 'LanguageAccessBlocksMatrixUserGroup';
// Content
const documentName = 'LanguageAccessBlocksMatrixContent';
const danishVariantName = 'LanguageAccessBlocksMatrixContent DA';
const englishTextValue = 'English nested block text';
// Document type
const documentTypeName = 'LanguageAccessBlocksMatrixDocType';
// Nested block structure
const outerBlockListName = 'LanguageAccessOuterBlockList';
const outerElementTypeName = 'LanguageAccessOuterElement';
const innerBlockListName = 'LanguageAccessInnerBlockList';
const innerElementTypeName = 'LanguageAccessInnerElement';
const textPropertyName = 'BlockText';
const textStringDataTypeName = 'Textstring';
// Cultures
const englishIsoCode = 'en-US';
const danishIsoCode = 'da';
const cultures = [
  {isoCode: englishIsoCode, name: documentName},
  {isoCode: danishIsoCode, name: danishVariantName},
];

// Variance flags: true = varies by culture, false = shared. When innerElement is false, text must also be false.
// editableWithoutSharedDataAccess: whether the text can be edited in the Danish variant by a user with Danish
// access who cannot edit shared data. A user who can edit shared data can always edit it.
const matrixCases: ReadonlyArray<{
  outerBlockList: boolean;
  outerElement: boolean;
  innerBlockList: boolean;
  innerElement: boolean;
  text: boolean;
  editableWithoutSharedDataAccess: boolean;
}> = [
  {outerBlockList: true,  outerElement: true,  innerBlockList: true,  innerElement: true,  text: true,  editableWithoutSharedDataAccess: true},
  {outerBlockList: true,  outerElement: true,  innerBlockList: true,  innerElement: true,  text: false, editableWithoutSharedDataAccess: true},
  {outerBlockList: true,  outerElement: true,  innerBlockList: true,  innerElement: false, text: false, editableWithoutSharedDataAccess: true},
  {outerBlockList: true,  outerElement: true,  innerBlockList: false, innerElement: true,  text: true,  editableWithoutSharedDataAccess: true},
  {outerBlockList: true,  outerElement: true,  innerBlockList: false, innerElement: true,  text: false, editableWithoutSharedDataAccess: true},
  {outerBlockList: true,  outerElement: true,  innerBlockList: false, innerElement: false, text: false, editableWithoutSharedDataAccess: true},
  {outerBlockList: true,  outerElement: false, innerBlockList: false, innerElement: true,  text: true,  editableWithoutSharedDataAccess: true},
  {outerBlockList: true,  outerElement: false, innerBlockList: false, innerElement: true,  text: false, editableWithoutSharedDataAccess: true},
  {outerBlockList: true,  outerElement: false, innerBlockList: false, innerElement: false, text: false, editableWithoutSharedDataAccess: true},
  {outerBlockList: false, outerElement: true,  innerBlockList: true,  innerElement: true,  text: true,  editableWithoutSharedDataAccess: true},
  {outerBlockList: false, outerElement: true,  innerBlockList: true,  innerElement: true,  text: false, editableWithoutSharedDataAccess: true},
  {outerBlockList: false, outerElement: true,  innerBlockList: true,  innerElement: false, text: false, editableWithoutSharedDataAccess: true},
  {outerBlockList: false, outerElement: true,  innerBlockList: false, innerElement: true,  text: true,  editableWithoutSharedDataAccess: true},
  {outerBlockList: false, outerElement: true,  innerBlockList: false, innerElement: true,  text: false, editableWithoutSharedDataAccess: false},
  {outerBlockList: false, outerElement: true,  innerBlockList: false, innerElement: false, text: false, editableWithoutSharedDataAccess: false},
  {outerBlockList: false, outerElement: false, innerBlockList: false, innerElement: true,  text: true,  editableWithoutSharedDataAccess: true},
  {outerBlockList: false, outerElement: false, innerBlockList: false, innerElement: true,  text: false, editableWithoutSharedDataAccess: false},
  {outerBlockList: false, outerElement: false, innerBlockList: false, innerElement: false, text: false, editableWithoutSharedDataAccess: false},
];

type MatrixCase = typeof matrixCases[number];

const formatCombo = (tc: MatrixCase): string =>
  `outer block list=${tc.outerBlockList ? 'Varies by culture' : 'Shared'}` +
  `, outer element=${tc.outerElement ? 'Varies by culture' : 'Shared'}` +
  `, inner block list=${tc.innerBlockList ? 'Varies by culture' : 'Shared'}` +
  `, inner element=${tc.innerElement ? 'Varies by culture' : 'Shared'}` +
  `, text=${tc.text ? 'Varies by culture' : 'Shared'}`;

const isSharedData = (tc: MatrixCase): boolean => !tc.outerBlockList && !tc.innerBlockList && !tc.text;

// Creates the document and, as the admin user, publishes a nested block with text in the default language.
async function createPublishedNestedBlockInEnglish(umbracoApi, umbracoUi, textStringDataTypeId: string, tc: MatrixCase) {
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithNestedBlockList(
    documentTypeName,
    outerBlockListName,
    outerElementTypeName,
    innerBlockListName,
    innerElementTypeName,
    textPropertyName,
    textStringDataTypeId,
    {
      outerBlockList: tc.outerBlockList,
      outerElement: tc.outerElement,
      innerBlockList: tc.innerBlockList,
      innerElement: tc.innerElement,
      text: tc.text,
    }
  );
  await umbracoApi.document.createDocumentWithMultipleVariantsAndNoValues(documentName, documentTypeId, cultures);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(documentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockCardWithName(outerElementTypeName, true);
  await umbracoUi.content.clickAddBlockWithNameButton(innerElementTypeName);
  await umbracoUi.content.clickBlockCardWithName(innerElementTypeName, true);
  await umbracoUi.content.isBlockWorkspacePropertyEditable(innerElementTypeName, textPropertyName, true);
  await umbracoUi.content.enterTextstring(englishTextValue);
  await umbracoUi.content.clickCreateInModal(innerElementTypeName);
  await umbracoUi.content.clickCreateInModal(outerElementTypeName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();
}

async function loginAsUserWithLanguages(umbracoApi, umbracoUi, isoCodes: string[], userCanEditSharedData: boolean) {
  const userGroupId = await umbracoApi.userGroup.createUserGroupWithLanguages(userGroupName, isoCodes, userCanEditSharedData);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);
}

// Opens the existing English block, which is what the user sees on the variant that has the block.
async function openExistingInnerBlock(umbracoUi) {
  await umbracoUi.content.clickEditBlockListEntryWithName(outerElementTypeName);
  await umbracoUi.content.clickEditNestedBlockListEntry(outerElementTypeName, innerElementTypeName);
}

// Opens the inner block in the Danish variant, adding a Danish block where the block lists vary by culture.
async function openInnerBlockInDanish(umbracoUi, tc: MatrixCase) {
  await umbracoUi.content.switchLanguage(danishIsoCode);
  if (tc.outerBlockList) {
    await umbracoUi.content.clickAddBlockElementButton();
    await umbracoUi.content.clickBlockCardWithName(outerElementTypeName, true);
    await umbracoUi.content.clickAddBlockWithNameButton(innerElementTypeName);
    await umbracoUi.content.clickBlockCardWithName(innerElementTypeName, true);
  } else {
    await umbracoUi.content.clickEditBlockListEntryWithName(outerElementTypeName);
    if (tc.innerBlockList) {
      await umbracoUi.content.clickAddBlockWithNameButton(innerElementTypeName);
      await umbracoUi.content.clickBlockCardWithName(innerElementTypeName, true);
    } else {
      await umbracoUi.content.clickEditNestedBlockListEntry(outerElementTypeName, innerElementTypeName);
    }
  }
}

export function registerBlockEditabilityTests(userCanEditSharedData: boolean) {
  let textStringDataTypeId: string;

  test.beforeEach(async ({umbracoApi, umbracoUi}) => {
    await umbracoApi.user.ensureNameNotExists(testUser.name);
    await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
    await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
    await umbracoApi.language.createDanishLanguage();
    const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
    textStringDataTypeId = textStringDataType.id;
    await umbracoUi.goToBackOffice();
  });

  test.afterEach(async ({umbracoApi}) => {
    await umbracoApi.loginToAdminUser();
    await umbracoApi.user.ensureNameNotExists(testUser.name);
    await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
    await umbracoApi.document.ensureNameNotExists(documentName);
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.documentType.ensureNameNotExists(outerElementTypeName);
    await umbracoApi.documentType.ensureNameNotExists(innerElementTypeName);
    await umbracoApi.dataType.ensureNameNotExists(outerBlockListName);
    await umbracoApi.dataType.ensureNameNotExists(innerBlockListName);
    await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  });

  test.describe('user with access to Danish only', () => {
    for (const tc of matrixCases) {
      const danishEditable = userCanEditSharedData || tc.editableWithoutSharedDataAccess;
      const englishEditable = userCanEditSharedData && isSharedData(tc);
      const expectedLabel = danishEditable ? 'can edit' : 'cannot edit';
      const englishLabel = englishEditable ? 'can edit' : 'cannot edit';

      test(`${englishLabel} the English block text (${formatCombo(tc)})`, async ({umbracoApi, umbracoUi}) => {
        test.slow();
        // Arrange
        await createPublishedNestedBlockInEnglish(umbracoApi, umbracoUi, textStringDataTypeId, tc);
        await loginAsUserWithLanguages(umbracoApi, umbracoUi, [danishIsoCode], userCanEditSharedData);

        // Act - the English variant, which the user has no language access to
        await openExistingInnerBlock(umbracoUi);

        // Assert
        await umbracoUi.content.isBlockWorkspacePropertyEditable(innerElementTypeName, textPropertyName, englishEditable);
      });

      test(`${expectedLabel} the Danish block text (${formatCombo(tc)})`, async ({umbracoApi, umbracoUi}) => {
        test.slow();
        // Arrange
        await createPublishedNestedBlockInEnglish(umbracoApi, umbracoUi, textStringDataTypeId, tc);
        await loginAsUserWithLanguages(umbracoApi, umbracoUi, [danishIsoCode], userCanEditSharedData);

        // Act
        await openInnerBlockInDanish(umbracoUi, tc);

        // Assert
        await umbracoUi.content.isBlockWorkspacePropertyEditable(innerElementTypeName, textPropertyName, danishEditable);
      });
    }
  });

  test.describe('user with access to the default language only', () => {
    // Block lists that are shared show the English block in the Danish variant, which this user has no language access to.
    const sharedCase = matrixCases[matrixCases.length - 1];

    const danishEditable = userCanEditSharedData && isSharedData(sharedCase);

    test(`${danishEditable ? 'can edit' : 'cannot edit'} the block text of the Danish variant (${formatCombo(sharedCase)})`, async ({umbracoApi, umbracoUi}) => {
      test.slow();
      // Arrange
      await createPublishedNestedBlockInEnglish(umbracoApi, umbracoUi, textStringDataTypeId, sharedCase);
      await loginAsUserWithLanguages(umbracoApi, umbracoUi, [englishIsoCode], userCanEditSharedData);

      // Act
      await openInnerBlockInDanish(umbracoUi, sharedCase);

      // Assert
      await umbracoUi.content.isBlockWorkspacePropertyEditable(innerElementTypeName, textPropertyName, danishEditable);
    });
  });
}
