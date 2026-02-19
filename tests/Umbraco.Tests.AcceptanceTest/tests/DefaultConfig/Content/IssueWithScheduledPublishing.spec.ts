import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';

const documentTypeName = "DocumentType";
const contentName = "Content";
const languageName = 'Danish';
let documentTypeId = null;

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowVaryByCulture(documentTypeName);
  await umbracoApi.document.createDefaultDocumentWithEnglishCulture(contentName, documentTypeId);
  await umbracoApi.language.ensureNameNotExists(languageName);
  await umbracoApi.language.createDanishLanguage();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureNameNotExists(languageName);
});

test('can schedule publish after unselecting all languages', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  // Open schedule modal and click schedule
  await umbracoUi.content.changeDocumentSectionLanguage(languageName);
  await umbracoUi.content.goToContentWithName('(' + contentName + ')');
  await umbracoUi.content.enterContentName('Tester');
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickSchedulePublishButton();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.content.clickSelectAllCheckbox();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  await umbracoUi.content.clickSelectAllCheckbox();
  await umbracoUi.content.clickSchedulePublishLanguageButton(languageName);

  // Assert
  await umbracoUi.content.doesSchedulePublishModalButtonContainDisabledTag(false);
  await umbracoUi.content.clickSchedulePublishModalButton();
});
