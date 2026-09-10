import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';
import * as fs from 'fs';

const packageName = 'TestPackage';
test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.package.ensureNameNotExists(packageName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.package.goToSection(ConstantHelper.sections.packages);
  await umbracoUi.package.clickCreatedTab();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.package.ensureNameNotExists(packageName);
});

test.fixme('can create a empty package', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}, tag: '@smoke'}, async ({ umbracoUi}) => {
  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.clickCreatedTab();
  await umbracoUi.package.isPackageNameVisible(packageName);
});

test.fixme('can update package name', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongPackageName = 'WrongPackageName';
  await umbracoApi.package.ensureNameNotExists(wrongPackageName);
  await umbracoApi.package.createEmptyPackage(wrongPackageName);
  await umbracoUi.reloadPage();
  await umbracoUi.package.goToSection(ConstantHelper.sections.packages);
  await umbracoUi.package.clickCreatedTab();

  // Act
  await umbracoUi.package.clickExistingPackageName(wrongPackageName);
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickUpdateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.clickCreatedTab();
  await umbracoUi.package.isPackageNameVisible(packageName);
  expect(await umbracoApi.package.doesNameExist(packageName)).toBeTruthy();
});

test.fixme('can delete a package', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.package.createEmptyPackage(packageName);
  await umbracoUi.reloadPage();
  await umbracoUi.package.clickCreatedTab();

  // Act
  await umbracoUi.package.clickDeleteButtonForPackageName(packageName);
  await umbracoUi.package.clickConfirmToDeleteButton();

  // Assert
  await umbracoUi.package.clickCreatedTab();
  await umbracoUi.package.isPackageNameVisible(packageName, false);
  expect(await umbracoApi.package.doesNameExist(packageName)).toBeFalsy();
});

test.fixme('can create a package with content', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeName = 'TestDocumentType';
  const documentName = 'TestDocument';
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);

  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickAddContentToPackageButton();
  await umbracoUi.package.clickLabelWithName(documentName);
  await umbracoUi.package.clickChooseContainerButton();
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  const packageData = await umbracoApi.package.getByName(packageName);
  expect(packageData.contentNodeId == documentId).toBeTruthy();
  await umbracoUi.package.isButtonWithNameVisible(documentName);

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.fixme('can create a package with media', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaName = 'TestMedia';
  await umbracoApi.media.ensureNameNotExists(mediaName);
  const mediaId = await umbracoApi.media.createDefaultMediaFile(mediaName);

  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickAddMediaToPackageButton();
  await umbracoUi.media.selectMediaWithName(mediaName);
  await umbracoUi.package.clickSubmitButton();
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.isTextWithExactNameVisible(mediaName);
  const packageData = await umbracoApi.package.getByName(packageName);
  expect(packageData.mediaIds[0] == mediaId).toBeTruthy();

  // Clean
  await umbracoApi.media.ensureNameNotExists(mediaName);
});

test.fixme('can create a package with document types', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeName = 'TestDocumentType';
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);

  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickAddDocumentTypeToPackageButton();
  await umbracoUi.package.clickLabelWithName(documentTypeName);
  await umbracoUi.package.clickChooseContainerButton();
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.isButtonWithNameVisible(documentTypeName);
  const packageData = await umbracoApi.package.getByName(packageName);
  expect(packageData.documentTypes[0] == documentTypeId).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.fixme('can create a package with media types', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const mediaTypeName = 'TestMediaType';
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  const mediaTypeId = await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);

  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickAddMediaTypeToPackageButton();
  await umbracoUi.package.clickButtonWithName(mediaTypeName, true);
  await umbracoUi.package.clickChooseContainerButton();
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.isButtonWithNameVisible(mediaTypeName);
  const packageData = await umbracoApi.package.getByName(packageName);
  expect(packageData.mediaTypes[0] == mediaTypeId).toBeTruthy();

  // Clean
  await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
});

test.fixme('can create a package with languages', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.ensureNameNotExists('Danish');
  const languageId = await umbracoApi.language.createDanishLanguage();
  const languageData = await umbracoApi.language.get(languageId);
  const languageName = languageData.name;

  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickAddLanguageToPackageButton();
  await umbracoUi.package.clickButtonWithName(languageName);
  await umbracoUi.package.clickSubmitButton();
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.isButtonWithNameVisible(languageName + ' ' + languageId);
  const packageData = await umbracoApi.package.getByName(packageName);
  expect(packageData.languages[0] == languageId).toBeTruthy();

  // Clean
  await umbracoApi.language.ensureNameNotExists(languageName);
});

test.fixme('can create a package with dictionary', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dictionaryName = 'TestDictionary';
  const dictionaryId = await umbracoApi.dictionary.createDefaultDictionary(dictionaryName);

  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickAddDictionaryToPackageButton();
  await umbracoUi.package.clickButtonWithName(dictionaryName);
  await umbracoUi.package.clickChooseContainerButton();
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.isButtonWithNameVisible(dictionaryName);
  const packageData = await umbracoApi.package.getByName(packageName);
  expect(packageData.dictionaryItems[0] == dictionaryId).toBeTruthy();

  // Clean
  await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
});

test.fixme('can create a package with data types', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeName = 'TestDataType';
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
  const dataTypeId = await umbracoApi.dataType.createDefaultDateTimeDataType(dataTypeName);

  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickAddDataTypesToPackageButton();
  await umbracoUi.package.clickLabelWithName(dataTypeName);
  await umbracoUi.package.clickChooseContainerButton();
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.isButtonWithNameVisible(dataTypeName);
  const packageData = await umbracoApi.package.getByName(packageName);
  expect(packageData.dataTypes[0] == dataTypeId).toBeTruthy();

  // Clean
  await umbracoApi.dataType.ensureNameNotExists(dataTypeName);
});

test.fixme('can create a package with templates', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const templateName = 'TestTemplate';
  await umbracoApi.template.ensureNameNotExists(templateName);
  const templateId = await umbracoApi.template.createDefaultTemplate(templateName);

  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickAddTemplatesToPackageButton();
  await umbracoUi.package.clickLabelWithName(templateName);
  await umbracoUi.package.clickChooseContainerButton();
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.isButtonWithNameVisible(templateName);
  const packageData = await umbracoApi.package.getByName(packageName);
  expect(packageData.templates[0] == templateId).toBeTruthy();

  // Clean
  await umbracoApi.template.ensureNameNotExists(templateName);
});

test.fixme('can create a package with stylesheets', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const stylesheetName = 'TestStylesheet.css';
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  const stylesheetId = await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);

  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickAddStylesheetToPackageButton();
  await umbracoUi.package.clickLabelWithName(stylesheetName);
  await umbracoUi.package.clickChooseContainerButton();
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.isButtonWithNameVisible(stylesheetName);
  const packageData = await umbracoApi.package.getByName(packageName);
  expect(packageData.stylesheets[0] == stylesheetId).toBeTruthy();

  // Clean
  await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
});

test.fixme('can create a package with scripts', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const scriptName = 'TestScripts.js';
  await umbracoApi.script.ensureNameNotExists(scriptName);
  const scriptId = await umbracoApi.script.createDefaultScript(scriptName);

  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickAddScriptToPackageButton();
  await umbracoUi.package.clickLabelWithName(scriptName);
  await umbracoUi.package.clickChooseContainerButton();
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.isButtonWithNameVisible(scriptName);
  const packageData = await umbracoApi.package.getByName(packageName);
  expect(packageData.scripts[0] == scriptId).toBeTruthy();

  // Clean
  await umbracoApi.script.ensureNameNotExists(scriptName);
});

test.fixme('can create a package with partial views', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const partialViewName = 'TestPartialView.cshtml';
  const partialViewId = await umbracoApi.partialView.createDefaultPartialView(partialViewName);

  // Act
  await umbracoUi.package.clickCreatePackageButton();
  await umbracoUi.package.enterPackageName(packageName);
  await umbracoUi.package.clickAddPartialViewToPackageButton();
  await umbracoUi.package.clickLabelWithName(partialViewName);
  await umbracoUi.package.clickChooseContainerButton();
  await umbracoUi.package.clickCreateButton();

  // Assert
  await umbracoUi.package.isSuccessNotificationVisible();
  await umbracoUi.package.isButtonWithNameVisible(partialViewName);
  const packageData = await umbracoApi.package.getByName(packageName);
  expect(packageData.partialViews[0] == partialViewId).toBeTruthy();

  // Clean
  await umbracoApi.partialView.ensureNameNotExists(partialViewName);
});

test.fixme('can download a package', {annotation: {type: 'fixme', description: "Commented out wholesale since the v15 era and never verified since; uncommented and marked fixme so it is visible in reports instead of invisible. Enable one at a time against a running instance."}}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const packageId = await umbracoApi.package.createEmptyPackage(packageName);
  await umbracoUi.reloadPage();

  // Act
  await umbracoUi.package.clickExistingPackageName(packageName);
  const packageData = await umbracoUi.package.downloadPackage(packageId);
  // Reads the packageFixture we have in the fixture library
  const path = require('path');
  const filePath = path.resolve('./fixtures/packageLibrary/package.xml');
  const packageFixture = fs.readFileSync(filePath);

  // Assert
  expect(packageData).toMatch(packageFixture.toString().trim());
});
