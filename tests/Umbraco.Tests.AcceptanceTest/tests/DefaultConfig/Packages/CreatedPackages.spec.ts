// import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
// import {expect} from '@playwright/test';
// import * as fs from 'fs';
//
// const packageName = 'TestPackage';
// // UNCOMMENT WHEN FIXED
// test.beforeEach(async ({umbracoApi, umbracoUi}) => {
//   await umbracoApi.package.ensureNameNotExists(packageName);
//   await umbracoUi.goToBackOffice();
//   await umbracoUi.package.goToSection(ConstantHelper.sections.packages);
//   await umbracoUi.package.clickCreatedTab();
// });
//
// test.afterEach(async ({umbracoApi}) => {
//   await umbracoApi.package.ensureNameNotExists(packageName);
// });
//
// test('can create a empty package', {tag: '@smoke'}, async ({ umbracoUi}) => {
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   await umbracoUi.package.clickCreatedTab();
//   await umbracoUi.package.isPackageNameVisible(packageName);
// });
//
// test('can update package name', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const wrongPackageName = 'WrongPackageName';
//   await umbracoApi.package.ensureNameNotExists(wrongPackageName);
//   await umbracoApi.package.createEmptyPackage(wrongPackageName);
//   await umbracoUi.reloadPage();
//   await umbracoUi.package.goToSection(ConstantHelper.sections.packages);
//   await umbracoUi.package.clickCreatedTab();
//
//   // Act
//   await umbracoUi.package.clickExistingPackageName(wrongPackageName);
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickUpdateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   await umbracoUi.package.clickCreatedTab();
//   await umbracoUi.package.isPackageNameVisible(packageName);
//   expect(umbracoApi.package.doesNameExist(packageName)).toBeTruthy();
// });
//
// test('can delete a package', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   await umbracoApi.package.createEmptyPackage(packageName);
//   await umbracoUi.reloadPage();
//   await umbracoUi.package.clickCreatedTab();
//
//   // Act
//   await umbracoUi.package.clickDeleteButtonForPackageName(packageName);
//   await umbracoUi.package.clickConfirmToDeleteButton();
//
//   // Assert
//   await umbracoUi.package.clickCreatedTab();
//   await umbracoUi.package.isPackageNameVisible(packageName, false);
//   expect(await umbracoApi.package.doesNameExist(packageName)).toBeFalsy();
// });
//
// test('can create a package with content', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const documentTypeName = 'TestDocumentType';
//   const documentName = 'TestDocument';
//   await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
//   const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
//   const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
//
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickAddContentToPackageButton();
//   await umbracoUi.package.clickLabelWithName(documentName);
//   await umbracoUi.package.clickChooseContainerButton();
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   const packageData = await umbracoApi.package.getByName(packageName);
//   expect(packageData.contentNodeId == documentId).toBeTruthy();
//   expect(umbracoUi.package.isButtonWithNameVisible(documentName)).toBeTruthy();
//
//   // Clean
//   await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
// });
//
// test('can create a package with media', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const mediaName = 'TestMedia';
//   await umbracoApi.media.ensureNameNotExists(mediaName);
//   const mediaId = await umbracoApi.media.createDefaultMediaFile(mediaName);
//
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickAddMediaToPackageButton();
//   await umbracoUi.media.selectMediaWithName(mediaName);
//   await umbracoUi.package.clickSubmitButton();
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   expect(umbracoUi.package.isTextWithExactNameVisible(mediaName)).toBeTruthy();
//   const packageData = await umbracoApi.package.getByName(packageName);
//   expect(packageData.mediaIds[0] == mediaId).toBeTruthy();
//
//   // Clean
//   await umbracoApi.media.ensureNameNotExists(mediaName);
// });
//
// test('can create a package with document types', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const documentTypeName = 'TestDocumentType';
//   await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
//   const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
//
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickAddDocumentTypeToPackageButton();
//   await umbracoUi.package.clickLabelWithName(documentTypeName);
//   await umbracoUi.package.clickChooseContainerButton();
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   expect(umbracoUi.package.isButtonWithNameVisible(documentTypeName)).toBeTruthy();
//   const packageData = await umbracoApi.package.getByName(packageName);
//   expect(packageData.documentTypes[0] == documentTypeId).toBeTruthy();
//
//   // Clean
//   await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
// });
//
// test('can create a package with media types', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const mediaTypeName = 'TestMediaType';
//   await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
//   const mediaTypeId = await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);
//
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickAddMediaTypeToPackageButton();
//   await umbracoUi.package.clickButtonWithName(mediaTypeName, true);
//   await umbracoUi.package.clickChooseContainerButton();
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   expect(umbracoUi.package.isButtonWithNameVisible(mediaTypeName)).toBeTruthy();
//   const packageData = await umbracoApi.package.getByName(packageName);
//   expect(packageData.mediaTypes[0] == mediaTypeId).toBeTruthy();
//
//   // Clean
//   await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
// });
//
// test('can create a package with languages', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   await umbracoApi.language.ensureNameNotExists('Danish');
//   const languageId = await umbracoApi.language.createDanishLanguage();
//   const languageData = await umbracoApi.language.get(languageId);
//   const languageName = languageData.name;
//
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickAddLanguageToPackageButton();
//   await umbracoUi.package.clickButtonWithName(languageName);
//   await umbracoUi.package.clickSubmitButton();
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   expect(umbracoUi.package.isButtonWithNameVisible(languageName + ' ' + languageId)).toBeTruthy();
//   const packageData = await umbracoApi.package.getByName(packageName);
//   expect(packageData.languages[0] == languageId).toBeTruthy();
//
//   // Clean
//   await umbracoApi.language.ensureNameNotExists(languageName);
// });
//
// test('can create a package with dictionary', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const dictionaryName = 'TestDictionary';
//   const dictionaryId = await umbracoApi.dictionary.createDefaultDictionary(dictionaryName);
//
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickAddDictionaryToPackageButton();
//   await umbracoUi.package.clickButtonWithName(dictionaryName);
//   await umbracoUi.package.clickChooseContainerButton();
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   expect(umbracoUi.package.isButtonWithNameVisible(dictionaryName)).toBeTruthy();
//   const packageData = await umbracoApi.package.getByName(packageName);
//   expect(packageData.dictionaryItems[0] == dictionaryId).toBeTruthy();
//
//   // Clean
//   await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
// });
//
// test('can create a package with data types', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const dataTypeName = 'TestDataType';
//   await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
//   const dataTypeId = await umbracoApi.dataType.createDateTypeDataType(dataTypeName);
//
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickAddDataTypesToPackageButton();
//   await umbracoUi.package.clickLabelWithName(dataTypeName);
//   await umbracoUi.package.clickChooseContainerButton();
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   expect(umbracoUi.package.isButtonWithNameVisible(dataTypeName)).toBeTruthy();
//   const packageData = await umbracoApi.package.getByName(packageName);
//   expect(packageData.dataTypes[0] == dataTypeId).toBeTruthy();
//
//   // Clean
//   await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
// });
//
// test('can create a package with templates', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const templateName = 'TestTemplate';
//   await umbracoApi.template.ensureNameNotExists(templateName);
//   const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
//
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickAddTemplatesToPackageButton();
//   await umbracoUi.package.clickLabelWithName(templateName);
//   await umbracoUi.package.clickChooseContainerButton();
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   expect(umbracoUi.package.isButtonWithNameVisible(templateName)).toBeTruthy();
//   const packageData = await umbracoApi.package.getByName(packageName);
//   expect(packageData.templates[0] == templateId).toBeTruthy();
//
//   // Clean
//   await umbracoApi.template.ensureNameNotExists(templateName);
// });
//
// test('can create a package with stylesheets', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const stylesheetName = 'TestStylesheet.css';
//   await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
//   const stylesheetId = await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);
//
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickAddStylesheetToPackageButton();
//   await umbracoUi.package.clickLabelWithName(stylesheetName);
//   await umbracoUi.package.clickChooseContainerButton();
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   expect(umbracoUi.package.isButtonWithNameVisible(stylesheetName)).toBeTruthy();
//   const packageData = await umbracoApi.package.getByName(packageName);
//   expect(packageData.stylesheets[0] == stylesheetId).toBeTruthy();
//
//   // Clean
//   await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
// });
//
// test('can create a package with scripts', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const scriptName = 'TestScripts.js';
//   await umbracoApi.script.ensureNameNotExists(scriptName);
//   const scriptId = await umbracoApi.script.createDefaultScript(scriptName);
//
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickAddScriptToPackageButton();
//   await umbracoUi.package.clickLabelWithName(scriptName);
//   await umbracoUi.package.clickChooseContainerButton();
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   expect(umbracoUi.package.isButtonWithNameVisible(scriptName)).toBeTruthy();
//   const packageData = await umbracoApi.package.getByName(packageName);
//   expect(packageData.scripts[0] == scriptId).toBeTruthy();
//
//   // Clean
//   await umbracoApi.script.ensureNameNotExists(scriptName);
// });
//
// test('can create a package with partial views', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const partialViewName = 'TestPartialView.cshtml';
//   const partialViewId = await umbracoApi.partialView.createDefaultPartialView(partialViewName);
//
//   // Act
//   await umbracoUi.package.clickCreatePackageButton();
//   await umbracoUi.package.enterPackageName(packageName);
//   await umbracoUi.package.clickAddPartialViewToPackageButton();
//   await umbracoUi.package.clickLabelWithName(partialViewName);
//   await umbracoUi.package.clickChooseContainerButton();
//   await umbracoUi.package.clickCreateButton();
//
//   // Assert
//   await umbracoUi.package.isSuccessNotificationVisible();
//   expect(umbracoUi.package.isButtonWithNameVisible(partialViewName)).toBeTruthy();
//   const packageData = await umbracoApi.package.getByName(packageName);
//   expect(packageData.partialViews[0] == partialViewId).toBeTruthy();
//
//   // Clean
//   await umbracoApi.partialView.ensureNameNotExists(partialViewName);
// });
//
// test('can download a package', async ({umbracoApi, umbracoUi}) => {
//   // Arrange
//   const packageId = await umbracoApi.package.createEmptyPackage(packageName);
//   await umbracoUi.reloadPage();
//
//   // Act
//   await umbracoUi.package.clickExistingPackageName(packageName);
//   const packageData = await umbracoUi.package.downloadPackage(packageId);
//   // Reads the packageFixture we have in the fixture library
//   const path = require('path');
//   const filePath = path.resolve('./fixtures/packageLibrary/package.xml');
//   const packageFixture = fs.readFileSync(filePath);
//
//   // Assert
//   expect(packageData).toMatch(packageFixture.toString().trim());
// });
