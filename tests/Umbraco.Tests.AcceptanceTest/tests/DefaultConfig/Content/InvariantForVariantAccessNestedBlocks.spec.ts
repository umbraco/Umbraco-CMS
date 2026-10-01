import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const testUser = ConstantHelper.testUserCredentials;
const userGroupName = 'InvariantForVariantUserGroup';
// Content
const documentName = 'NestedBlockMatrixContent';
const danishVariantName = 'NestedBlockMatrixContent DA';
const englishTextValue = 'English nested block text';
// Document type
const documentTypeName = 'NestedBlockMatrixDocType';
// Nested block structure
const outerBlockListName = 'OuterBlockList';
const outerElementTypeName = 'OuterElement';
const innerBlockListName = 'InnerBlockList';
const innerElementTypeName = 'InnerElement';
const textPropertyName = 'BlockText';
const textStringDataTypeName = 'Textstring';
// Cultures
const firstCulture = 'en-US';
const secondCulture = 'da';
const cultures = [
  {isoCode: firstCulture, name: documentName},
  {isoCode: secondCulture, name: danishVariantName},
];
// Data Type
let textStringDataTypeId: string;

// 18 variance combinations of the nested block editor.
// Variance flags: true = Varies By Culture, false = Shared/Invariant.
// Constraint: when innerElement = false, text must also be false.
// editableWithoutAccess: whether the text property can be edited by a user without invariant-for-variant access.
// A user with the access can edit the text property in all combinations.
const matrixCases: ReadonlyArray<{
  outerBlockList: boolean;
  outerElement: boolean;
  innerBlockList: boolean;
  innerElement: boolean;
  text: boolean;
  editableWithoutAccess: boolean;
}> = [
  {outerBlockList: true,  outerElement: true,  innerBlockList: true,  innerElement: true,  text: true,  editableWithoutAccess: true},
  {outerBlockList: true,  outerElement: true,  innerBlockList: true,  innerElement: true,  text: false, editableWithoutAccess: true},
  {outerBlockList: true,  outerElement: true,  innerBlockList: true,  innerElement: false, text: false, editableWithoutAccess: true},
  {outerBlockList: true,  outerElement: true,  innerBlockList: false, innerElement: true,  text: true,  editableWithoutAccess: true},
  {outerBlockList: true,  outerElement: true,  innerBlockList: false, innerElement: true,  text: false, editableWithoutAccess: true},
  {outerBlockList: true,  outerElement: true,  innerBlockList: false, innerElement: false, text: false, editableWithoutAccess: true},
  {outerBlockList: true,  outerElement: false, innerBlockList: false, innerElement: true,  text: true,  editableWithoutAccess: true},
  {outerBlockList: true,  outerElement: false, innerBlockList: false, innerElement: true,  text: false, editableWithoutAccess: true},
  {outerBlockList: true,  outerElement: false, innerBlockList: false, innerElement: false, text: false, editableWithoutAccess: true},
  {outerBlockList: false, outerElement: true,  innerBlockList: true,  innerElement: true,  text: true,  editableWithoutAccess: true},
  {outerBlockList: false, outerElement: true,  innerBlockList: true,  innerElement: true,  text: false, editableWithoutAccess: true},
  {outerBlockList: false, outerElement: true,  innerBlockList: true,  innerElement: false, text: false, editableWithoutAccess: true},
  {outerBlockList: false, outerElement: true,  innerBlockList: false, innerElement: true,  text: true,  editableWithoutAccess: true},
  {outerBlockList: false, outerElement: true,  innerBlockList: false, innerElement: true,  text: false, editableWithoutAccess: false},
  {outerBlockList: false, outerElement: true,  innerBlockList: false, innerElement: false, text: false, editableWithoutAccess: false},
  {outerBlockList: false, outerElement: false, innerBlockList: false, innerElement: true,  text: true,  editableWithoutAccess: true},
  {outerBlockList: false, outerElement: false, innerBlockList: false, innerElement: true,  text: false, editableWithoutAccess: false},
  {outerBlockList: false, outerElement: false, innerBlockList: false, innerElement: false, text: false, editableWithoutAccess: false},
];

type MatrixCase = typeof matrixCases[number];

const formatCombo = (tc: MatrixCase): string =>
  `outer block list=${tc.outerBlockList ? 'Varies by culture' : 'Shared'}` +
  `, outer element=${tc.outerElement ? 'Varies by culture' : 'Shared'}` +
  `, inner block list=${tc.innerBlockList ? 'Varies by culture' : 'Shared'}` +
  `, inner element=${tc.innerElement ? 'Varies by culture' : 'Shared'}` +
  `, text=${tc.text ? 'Varies by culture' : 'Shared'}`;

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
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
  await umbracoApi.language.ensureIsoCodeNotExists(secondCulture);
});

// Creates the document and, as the admin user, publishes a nested block with text in the default language.
async function createPublishedNestedBlockInDefaultLanguage(umbracoApi, umbracoUi, tc: MatrixCase) {
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

async function loginAsUserWithInvariantForVariantAccess(umbracoApi, umbracoUi, hasAccessToInvariantForVariant: boolean) {
  const userGroupId = await umbracoApi.userGroup.createUserGroupWithInvariantForVariantAccess(userGroupName, hasAccessToInvariantForVariant);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);
}

// Opens the inner block's text field in the non-default language.
async function openInnerBlockInNonDefaultLanguage(umbracoUi, tc: MatrixCase) {
  await umbracoUi.content.switchLanguage(secondCulture);
  if (tc.outerBlockList) {
    // Outer block list varies → DA gets its own list, EN's block is not visible.
    // Add a fresh outer + inner block in DA to expose the inner text field.
    await umbracoUi.content.clickAddBlockElementButton();
    await umbracoUi.content.clickBlockCardWithName(outerElementTypeName, true);
    await umbracoUi.content.clickAddBlockWithNameButton(innerElementTypeName);
    await umbracoUi.content.clickBlockCardWithName(innerElementTypeName, true);
  } else {
    // Outer block list is shared → the EN block is visible in DA. Open the
    // existing outer block, then the existing inner block within it.
    await umbracoUi.content.clickEditBlockListEntryWithName(outerElementTypeName);
    if (tc.innerBlockList) {
      // Inner block list varies → DA gets its own inner block, EN's inner block is not visible.
      // Add a fresh inner block in DA to expose the inner text field.
      await umbracoUi.content.clickAddBlockWithNameButton(innerElementTypeName);
      await umbracoUi.content.clickBlockCardWithName(innerElementTypeName, true);
    } else {
      // Inner block list is shared → the EN inner block is visible in DA. Open the existing inner block.
      await umbracoUi.content.clickEditNestedBlockListEntry(outerElementTypeName, innerElementTypeName);
    }
  }
}

test.describe('with invariant-for-variant access', () => {
  for (const tc of matrixCases) {
    test(`can edit text property in non-default language (${formatCombo(tc)})`, async ({umbracoApi, umbracoUi}) => {
      test.slow();
      // Arrange
      await createPublishedNestedBlockInDefaultLanguage(umbracoApi, umbracoUi, tc);
      await loginAsUserWithInvariantForVariantAccess(umbracoApi, umbracoUi, true);

      // Act
      await openInnerBlockInNonDefaultLanguage(umbracoUi, tc);

      // Assert
      await umbracoUi.content.isBlockWorkspacePropertyEditable(innerElementTypeName, textPropertyName, true);
    });
  }
});

test.describe('without invariant-for-variant access', () => {
  for (const tc of matrixCases) {
    const expectedLabel = tc.editableWithoutAccess ? 'can edit' : 'cannot edit';

    test(`${expectedLabel} text property in non-default language (${formatCombo(tc)})`, async ({umbracoApi, umbracoUi}) => {
      test.slow();
      // Arrange
      await createPublishedNestedBlockInDefaultLanguage(umbracoApi, umbracoUi, tc);
      await loginAsUserWithInvariantForVariantAccess(umbracoApi, umbracoUi, false);

      // Act
      await openInnerBlockInNonDefaultLanguage(umbracoUi, tc);

      // Assert
      await umbracoUi.content.isBlockWorkspacePropertyEditable(innerElementTypeName, textPropertyName, tc.editableWithoutAccess);
    });
  }

  // Shared data is read-only in every language without the access, the default language included.
  for (const tc of matrixCases.filter((x) => !x.editableWithoutAccess)) {
    test(`cannot edit text property in default language (${formatCombo(tc)})`, async ({umbracoApi, umbracoUi}) => {
      test.slow();
      // Arrange
      await createPublishedNestedBlockInDefaultLanguage(umbracoApi, umbracoUi, tc);
      await loginAsUserWithInvariantForVariantAccess(umbracoApi, umbracoUi, false);

      // Act
      await umbracoUi.content.clickEditBlockListEntryWithName(outerElementTypeName);
      await umbracoUi.content.clickEditNestedBlockListEntry(outerElementTypeName, innerElementTypeName);

      // Assert
      await umbracoUi.content.isBlockWorkspacePropertyEditable(innerElementTypeName, textPropertyName, false);
    });
  }
});
