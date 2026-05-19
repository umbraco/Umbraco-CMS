import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// User and User Group
const testUser = ConstantHelper.testUserCredentials;
const userGroupName = 'BackofficeSearchTestGroup';
// Element type
const elementTypeName = 'BackofficeSearchElementType';
const variantTypeName = 'BackofficeSearchVariantElementType';
// Element
const recycledElementName = 'BackofficeSearchElementRecycled';
const unpublishedElementName = 'BackofficeSearchElementUnpublished';
const folderName = 'BackofficeSearchElementFolder';
const subFolderName = 'BackofficeSearchElementSubFolder';
const folderedElementName = 'BackofficeSearchElementInFolder';
const variantElementEN = 'BackofficeSearchElementVariantEN';
const variantElementDA = 'BackofficeSearchElementVariantDA';
// Data type
const dataTypeName = 'Textstring';
// Language
const danishIsoCode = 'da';

test.beforeEach(async ({umbracoUi}) => {
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(recycledElementName);  
  await umbracoApi.element.ensureNameNotExists(unpublishedElementName);
  await umbracoApi.element.ensureNameNotExists(folderedElementName);
  await umbracoApi.element.ensureNameNotExists(folderName);
  await umbracoApi.element.ensureNameNotExists(subFolderName);
  await umbracoApi.element.ensureNameNotExists(variantElementEN);
  await umbracoApi.element.ensureNameNotExists(variantElementDA);
  await umbracoApi.element.emptyRecycleBin();
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(variantTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.user.ensureNameNotExists(testUser.name);
});

test('can see Elements provider auto-selected when opening search from the Library section', {tag: '@smoke'}, async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();

  // Assert
  await umbracoUi.backofficeSearch.isSearchModalVisible();
  await umbracoUi.backofficeSearch.isSearchProviderActive('Elements');
});

const elementNameCases = [
  {label: 'a basic name', name: 'BackofficeSearchElementItem'},
  {label: 'a short name', name: 'Bse', searchKeyword: 'Bse'},
  {label: 'a long name', name: 'BackofficeSearchElementWithAVeryLongNameToTestBoundaryHandlingOfElementNameFieldsInSearchResults'},
  {label: 'a name with spaces', name: 'Backoffice Search Element With Spaces'},
  {label: 'a name with numbers', name: 'BackofficeSearchElement1234567890'},
  {label: 'a name with special characters', name: 'BackofficeSearchElement , . ! ? # $ % & * @ é ü ă đ 漢字'},
  {label: 'a name with Unicode characters', name: 'BackofficeSearchElement Æøå'},
];
for (const elementNameCase of elementNameCases) {
  test(`can find an element by ${elementNameCase.label}`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
    await umbracoApi.element.createDefaultElement(elementNameCase.name, elementTypeId);
    await umbracoUi.library.goToSection(ConstantHelper.sections.library);

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.searchForElement(elementNameCase.name);

    // Assert
    await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(elementNameCase.name);

    // Clean
    await umbracoApi.element.ensureNameNotExists(elementNameCase.name);
  });
}

const searchKeywordCases = [
  {type: 'a keyword containing an SQL injection', value: "' OR '1'='1'; --"},
  {type: 'a keyword containing a cross-site scripting', value: "<script>alert('XSS')</script>"},
];
for (const searchKeywordCase of searchKeywordCases) {
  test(`can search elements using ${searchKeywordCase.type}`, async ({umbracoUi}) => {
    // Arrange
    await umbracoUi.library.goToSection(ConstantHelper.sections.library);

    // Act
    await umbracoUi.backofficeSearch.clickSearchHeaderButton();
    await umbracoUi.backofficeSearch.searchForElement(searchKeywordCase.value);

    // Assert
    await umbracoUi.backofficeSearch.isSearchModalVisible();
    await umbracoUi.backofficeSearch.isNoResultsMessageVisible();
  });
}

test('can see a Trashed tag for a trashed element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const recycledElementId = await umbracoApi.element.createDefaultElement(recycledElementName, elementTypeId);
  await umbracoApi.element.publish(recycledElementId);
  await umbracoApi.element.moveToRecycleBin(recycledElementId);
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForElement(recycledElementName);

  // Assert
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(recycledElementName);
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible('Trashed');
});

test('can see a Draft tag for an element with unpublished changes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  await umbracoApi.element.createDefaultElement(unpublishedElementName, elementTypeId);
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForElement(unpublishedElementName);

  // Assert
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(unpublishedElementName);
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible('Draft');
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible('Trashed', false);
});

test('can see the folder path as breadcrumb for an element inside folders', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const folderId = await umbracoApi.element.createDefaultElementFolder(folderName);
  const subFolderId = await umbracoApi.element.createDefaultElementFolder(subFolderName, folderId);
  await umbracoApi.element.createDefaultElementWithParent(folderedElementName, elementTypeId, subFolderId);
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForElement(folderedElementName);

  // Assert
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(folderedElementName);
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(`${folderName} / ${subFolderName}`);
});

test('can see the variant name matching the current app language', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.createDanishLanguage();
  const textStringDataType = await umbracoApi.dataType.getByName(dataTypeName);
  const variantTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(
    variantTypeName,
    'TestGroup',
    dataTypeName,
    textStringDataType.id,
    true,
    true,
  );
  await umbracoApi.element.createDefaultElementWithEnglishAndDanishVariants(
    variantTypeId,
    variantElementEN,
    variantElementDA,
    dataTypeName,
    'EN text',
    'DA text',
  );
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();
  await umbracoUi.backofficeSearch.searchForElement('BackofficeSearchElementVariant');

  // Assert
  // Default app language is English, so the EN variant name is the displayed label
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(variantElementEN);
  await umbracoUi.backofficeSearch.isSearchResultWithNameVisible(variantElementDA, false);
});

test('cannot see Elements provider as a user without Library section access', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const userGroupId = await umbracoApi.userGroup.createSimpleUserGroupWithContentSection(userGroupName);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();

  // Act
  await umbracoUi.backofficeSearch.clickSearchHeaderButton();

  // Assert
  await umbracoUi.backofficeSearch.isSearchModalVisible();
  await umbracoUi.backofficeSearch.isSearchProviderVisible('Documents');
  await umbracoUi.backofficeSearch.isSearchProviderVisible('Elements', false);

  // Clean
  await umbracoApi.loginToAdminUser();
});
