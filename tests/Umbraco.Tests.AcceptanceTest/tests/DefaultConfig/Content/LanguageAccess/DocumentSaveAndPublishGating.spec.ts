import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// What the variant selector, the Save action and the save and publish dialogs allow, and what publishing publishes,
// based on the languages the user may edit and on whether the user can edit shared data.
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
    languages.map(x => ({isoCode: x.isoCode, name: x.documentName, value: `${x.isoCode}Value`})), originalSharedValue);
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

const originalSharedValue = 'SharedValue';
const pendingSharedValue = 'PendingSharedValue';
const newDanishValue = 'NewDanishValue';

// The save, publish, schedule and publish with descendants dialogs use different picker elements.
const dialogItems = (page) =>
  page.locator('umb-document-variant-language-picker, umb-content-variant-language-picker, umb-document-schedule-modal')
    .locator('uui-menu-item');

const propertyInput = (page, propertyName: string) =>
  page.locator('umb-document-workspace-editor umb-property').filter({hasText: propertyName}).locator('#input');

const valueOf = (document, propertyName: string, culture: string | null) =>
  document.values.find((x) => x.alias === AliasHelper.toAlias(propertyName) && x.culture === culture)?.value;

const stateOf = (document, culture: string) => document.variants.find((x) => x.culture === culture)?.state;

// As the administrator: publishes every language, then leaves a change to the shared data unpublished.
async function publishEveryLanguageAndChangeSharedData(umbracoApi) {
  const documentId = (await umbracoApi.document.getByName(english.documentName)).id;
  await umbracoApi.document.publishDocumentWithCultures(documentId, languages.map((x) => x.isoCode));
  const document = await umbracoApi.document.get(documentId);
  document.values.find((x) => x.alias === AliasHelper.toAlias(sharedPropertyName) && x.culture === null).value = pendingSharedValue;
  await umbracoApi.document.update(documentId, document);
  return documentId;
}

async function openDanish(umbracoUi) {
  await umbracoUi.content.changeDocumentSectionLanguage(danish.name);
  await umbracoUi.content.goToContentWithName(danish.documentName);
}

async function editAndPublishDanish(page, umbracoUi) {
  await propertyInput(page, varyingPropertyName).fill(newDanishValue);
  await umbracoUi.content.clickSaveAndPublishButton();
  await expectOnlyDanishToBeSelectable(page);
  await umbracoUi.content.clickContainerSaveAndPublishButtonAndWaitForContentToBePublished();
  await umbracoUi.content.isSuccessNotificationVisible();
}

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

  test('only offers Danish in the schedule publish dialog', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await loginToContentSectionAs(umbracoApi, umbracoUi, [danish.isoCode], true);
    await umbracoUi.content.changeDocumentSectionLanguage(danish.name);
    await umbracoUi.content.goToContentWithName(danish.documentName);

    // Act
    await umbracoUi.content.clickViewMoreOptionsButton();
    await umbracoUi.content.clickSchedulePublishButton();

    // Assert
    await expectOnlyDanishToBeSelectable(page);
  });

  test('only offers Danish in the publish with descendants dialog', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await loginToContentSectionAs(umbracoApi, umbracoUi, [danish.isoCode], true);
    await umbracoUi.content.changeDocumentSectionLanguage(danish.name);
    await umbracoUi.content.goToContentWithName(danish.documentName);

    // Act
    await umbracoUi.content.clickViewMoreOptionsButton();
    await umbracoUi.content.clickPublishWithDescendantsButton();

    // Assert
    await expectOnlyDanishToBeSelectable(page);
  });

  test('publishes Danish with the changed shared data and leaves the other languages unpublished', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const newSharedValue = 'NewSharedValue';
    await loginToContentSectionAs(umbracoApi, umbracoUi, [danish.isoCode], true);
    await openDanish(umbracoUi);
    await propertyInput(page, sharedPropertyName).fill(newSharedValue);

    // Act
    await editAndPublishDanish(page, umbracoUi);

    // Assert
    await umbracoApi.loginToAdminUser();
    const documentId = (await umbracoApi.document.getByName(english.documentName)).id;
    const document = await umbracoApi.document.get(documentId);
    expect(stateOf(document, danish.isoCode)).toBe('Published');
    expect(stateOf(document, english.isoCode)).not.toBe('Published');
    expect(stateOf(document, vietnamese.isoCode)).not.toBe('Published');
    const published = await umbracoApi.document.getPublished(documentId);
    expect(valueOf(published, varyingPropertyName, danish.isoCode)).toBe(newDanishValue);
    expect(valueOf(published, sharedPropertyName, null)).toBe(newSharedValue);
  });

  test('publishes the pending shared data with Danish', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const documentId = await publishEveryLanguageAndChangeSharedData(umbracoApi);
    await loginToContentSectionAs(umbracoApi, umbracoUi, [danish.isoCode], true);
    await openDanish(umbracoUi);

    // Act
    await editAndPublishDanish(page, umbracoUi);

    // Assert
    await umbracoApi.loginToAdminUser();
    const published = await umbracoApi.document.getPublished(documentId);
    expect(valueOf(published, varyingPropertyName, danish.isoCode)).toBe(newDanishValue);
    expect(valueOf(published, sharedPropertyName, null)).toBe(pendingSharedValue);
  });

  test('tags the English variant as read-only although its shared data can be edited', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    await loginToContentSectionAs(umbracoApi, umbracoUi, [danish.isoCode], true);

    // Act
    await umbracoUi.content.goToContentWithName(english.documentName);

    // Assert
    await umbracoUi.content.isDocumentReadOnly(true);
    await expect(propertyInput(page, sharedPropertyName)).toBeEditable();
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

test.describe('user who can edit Danish and cannot edit shared data', () => {
  test('publishes Danish without the pending shared data', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const documentId = await publishEveryLanguageAndChangeSharedData(umbracoApi);
    await loginToContentSectionAs(umbracoApi, umbracoUi, [danish.isoCode], false);
    await openDanish(umbracoUi);

    // Act
    await editAndPublishDanish(page, umbracoUi);

    // Assert
    await umbracoApi.loginToAdminUser();
    const published = await umbracoApi.document.getPublished(documentId);
    expect(valueOf(published, varyingPropertyName, danish.isoCode)).toBe(newDanishValue);
    expect(valueOf(published, sharedPropertyName, null)).toBe(originalSharedValue);
    const document = await umbracoApi.document.get(documentId);
    expect(valueOf(document, sharedPropertyName, null)).toBe(pendingSharedValue);
  });
});

test.describe('user who cannot edit any language and can edit shared data', () => {
  test('can save the shared data on its own and leaves every language untouched', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const newSharedValue = 'UpdatedSharedValue';
    await loginToContentSectionAs(umbracoApi, umbracoUi, [], true);
    await umbracoUi.content.goToContentWithName(english.documentName);
    await propertyInput(page, sharedPropertyName).fill(newSharedValue);

    // Act
    const documentUpdated = page.waitForResponse((response) =>
      response.request().method() === 'PUT' && /\/document\/[0-9a-f-]{36}$/.test(response.url()));
    await umbracoUi.content.clickSaveButtonForContent();

    // Assert
    expect((await documentUpdated).ok()).toBeTruthy();
    await umbracoApi.loginToAdminUser();
    const document = await umbracoApi.document.getByName(english.documentName);
    expect(valueOf(document, sharedPropertyName, null)).toBe(newSharedValue);
    for (const language of languages) {
      expect(valueOf(document, varyingPropertyName, language.isoCode)).toBe(`${language.isoCode}Value`);
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
    await expect(page.locator('umb-content-publish-modal').getByLabel('Save and publish', {exact: true})).toBeDisabled();
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
