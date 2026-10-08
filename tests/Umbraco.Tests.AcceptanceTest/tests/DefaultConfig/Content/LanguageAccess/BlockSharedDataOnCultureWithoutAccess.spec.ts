import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// A shared (culture invariant) Block List on culture variant content, holding a culture variant element type with a
// shared property. The block and its shared property are shared data, so a user who can edit shared data can edit
// them from every culture, including cultures the user has no language access to.

const testUser = ConstantHelper.testUserCredentials;
const userGroupName = 'SharedBlockDataUserGroup';
// Content
const documentName = 'SharedBlockDataContent';
const danishVariantName = 'SharedBlockDataContent DA';
const sharedTextValue = 'Shared block text';
// Document type
const documentTypeName = 'SharedBlockDataDocType';
const blockListName = 'SharedBlockDataBlockList';
const elementTypeName = 'SharedBlockDataVariantElement';
const sharedTextPropertyName = 'SharedBlockText';
const textStringDataTypeName = 'Textstring';
// Cultures
const englishIsoCode = 'en-US';
const danishIsoCode = 'da';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  await umbracoApi.language.createDanishLanguage();

  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  const elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'ContentTab', 'TestGroup', sharedTextPropertyName, textStringDataType.id, false, true, false) as string;
  const blockListId = await umbracoApi.dataType.createBlockListDataTypeWithABlock(blockListName, elementTypeId) as string;
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, blockListName, blockListId);
  await umbracoApi.document.createDocumentWithMultipleVariantsAndNoValues(documentName, documentTypeId, [
    {isoCode: englishIsoCode, name: documentName},
    {isoCode: danishIsoCode, name: danishVariantName},
  ]);

  // As the administrator, add a block with a shared value from the default culture and publish it.
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(documentName);
  await umbracoUi.content.clickAddBlockElementButton();
  await umbracoUi.content.clickBlockCardWithName(elementTypeName, true);
  await umbracoUi.content.enterTextstring(sharedTextValue);
  await umbracoUi.content.clickCreateInModal(elementTypeName);
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.clickContainerSaveAndPublishButton();
  await umbracoUi.content.isSuccessNotificationVisible();

  // Log in as a user with access to Danish only, who can edit shared data.
  const userGroupId = await umbracoApi.userGroup.createUserGroupWithLanguages(userGroupName, [danishIsoCode], true);
  await umbracoApi.user.setUserPermissions(testUser.name, testUser.email, testUser.password, userGroupId);
  await umbracoApi.user.loginToUser(testUser.name, testUser.email, testUser.password);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content, false);
  await umbracoUi.content.goToContentWithName(documentName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.loginToAdminUser();
  await umbracoApi.user.ensureNameNotExists(testUser.name);
  await umbracoApi.userGroup.ensureNameNotExists(userGroupName);
  await umbracoApi.document.ensureNameNotExists(documentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.dataType.ensureNameNotExists(blockListName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('can edit the shared block property on a culture with language access', async ({umbracoUi}) => {
  test.slow();
  // Act
  await umbracoUi.content.switchLanguage('Danish');
  await umbracoUi.content.clickEditBlockListEntryWithName(elementTypeName);

  // Assert
  await umbracoUi.content.isBlockWorkspacePropertyEditable(elementTypeName, sharedTextPropertyName, true);
});

test('can edit the shared block property on a culture without language access', async ({umbracoUi}) => {
  test.slow();
  // Act - the document opens in English, which the user has no language access to
  await umbracoUi.content.clickEditBlockListEntryWithName(elementTypeName);

  // Assert
  await umbracoUi.content.isBlockWorkspacePropertyEditable(elementTypeName, sharedTextPropertyName, true);
});
