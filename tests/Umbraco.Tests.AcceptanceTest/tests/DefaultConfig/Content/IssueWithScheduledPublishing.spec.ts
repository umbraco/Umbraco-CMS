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

// https://github.com/umbraco/Umbraco-CMS/issues/18555
test.skip('Can schedule publish after unselecting all languages', async ({umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  // Open schedule modal and click schedule
  await umbracoUi.content.changeDocumentSectionLanguage(languageName);
  await umbracoUi.content.goToContentWithName('(' + contentName + ')');
  await umbracoUi.content.enterContentName('Tester');
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  await umbracoUi.waitForTimeout(500);
  await umbracoUi.content.clickSelectAllCheckbox();
  await umbracoUi.waitForTimeout(500);
  await umbracoUi.content.clickSelectAllCheckbox();
  await umbracoUi.content.clickButtonWithName(contentName);

  // Assert
  await umbracoUi.content.doesSchedulePublishModalButtonContainDisabledTag(false);
});
