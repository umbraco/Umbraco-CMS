import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from '@playwright/test';
import * as fs from 'fs';

test.describe('Created packages tests', () => {
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

  test.skip('can create a empty package @smoke', async ({umbracoUi}) => {
    // Act
    await umbracoUi.package.clickCreatePackageButton();
    await umbracoUi.package.enterPackageName(packageName);
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.isPackageNameVisible(packageName);
  });

  test.skip('can update package name', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const wrongPackageName = 'WrongPackageName';
    await umbracoApi.package.ensureNameNotExists(wrongPackageName);
    await umbracoApi.package.createEmptyPackage(wrongPackageName);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickExistingPackageName(wrongPackageName);
    await umbracoUi.package.enterPackageName(packageName);
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.isPackageNameVisible(packageName);
    expect(umbracoApi.package.doesNameExist(packageName)).toBeTruthy();
  });

  test.skip('can delete a package', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    await umbracoApi.package.createEmptyPackage(packageName);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickDeleteButtonForPackageName(packageName);
    await umbracoUi.package.clickDeleteExactLabel();

    // Assert
    await umbracoUi.package.isPackageNameVisible(packageName, false);
    expect(await umbracoApi.package.doesNameExist(packageName)).toBeFalsy();
  });

  // TODO: Update the locators for the choose button. If it is updated or not
  // TODO: Remove .skip when the test is able to run. Currently it is not possible to add content to a package
  test.skip('can create a package with content', async ({page, umbracoApi, umbracoUi}) => {
    // Arrange
    const documentTypeName = 'TestDocumentType';
    const documentName = 'TestDocument';
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.package.createEmptyPackage(packageName);
    const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
    const documentId = await umbracoApi.document.createDefaultDocument(documentName, documentTypeId);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickExistingPackageName(packageName);
    // The frontend has updated the button name to "Choose" of "Add". But they are a bit unsure if they want to change it to select instead.
    // So for the moment I have used the page instead of our UiHelper. Because it is easier to change the locator.
    // await umbracoUi.package.clickAddContentToPackageButton();
    await page.locator('[label="Content"] >> [label="Choose"]').click();
    await umbracoUi.package.clickLabelWithName(documentName);
    await umbracoUi.package.clickChooseBtn();
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.contentNodeId == documentId).toBeTruthy();
    await umbracoUi.package.clickExistingPackageName(packageName);
    expect(umbracoUi.package.isButtonWithNameVisible(documentName + ' ' + documentId)).toBeTruthy();

    // Clean
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  // Currently unable to run this test. Because you are not able to save a mediaId
  test.skip('can create a package with media', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const mediaTypeName = 'TestMediaType';
    const mediaName = 'TestMedia';
    await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
    await umbracoApi.package.createEmptyPackage(packageName);
    const mediaTypeId = await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);
    const mediaId = await umbracoApi.media.createDefaultMedia(mediaName, mediaTypeId);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickExistingPackageName(packageName);
    await umbracoUi.package.clickAddMediaToPackageButton();
    await umbracoUi.package.clickCaretButton();
    await umbracoUi.package.clickLabelWithName(mediaName);
    await umbracoUi.package.clickSubmitButton();
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.clickExistingPackageName(packageName);
    expect(umbracoUi.package.isButtonWithNameVisible(mediaTypeName + ' ' + mediaId)).toBeTruthy();
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.mediaIds[0] == mediaId).toBeTruthy();

    // Clean
    await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  });

  test.skip('can create a package with document types', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const documentTypeName = 'TestDocumentType';
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
    await umbracoApi.package.createEmptyPackage(packageName);
    const documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickExistingPackageName(packageName);
    await umbracoUi.package.clickAddDocumentTypeToPackageButton();
    await umbracoUi.package.clickCaretButton();
    await umbracoUi.package.clickLabelWithName(documentTypeName);
    await umbracoUi.package.clickSubmitButton();
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.clickExistingPackageName(packageName);
    expect(umbracoUi.package.isButtonWithNameVisible(documentTypeName + ' ' + documentTypeId)).toBeTruthy();
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.documentTypes[0] == documentTypeId).toBeTruthy();

    // Clean
    await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  });

  // TODO: Remove .skip when the test is able to run. Currently waiting for button
  test.skip('can create a package with media types', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const mediaTypeName = 'TestMediaType';
    await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
    await umbracoApi.package.createEmptyPackage(packageName);
    const mediaTypeId = await umbracoApi.mediaType.createDefaultMediaType(mediaTypeName);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickExistingPackageName(packageName);
    await umbracoUi.package.clickAddMediaTypeToPackageButton();
    await umbracoUi.package.clickCaretButton();
    await umbracoUi.package.clickLabelWithName(mediaTypeName);
    await umbracoUi.package.clickSubmitButton()
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.clickExistingPackageName(packageName);
    expect(umbracoUi.package.isButtonWithNameVisible(mediaTypeName + ' ' + mediaTypeId)).toBeTruthy();
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.mediaTypes[0] == mediaTypeId).toBeTruthy();

    // Clean
    await umbracoApi.mediaType.ensureNameNotExists(mediaTypeName);
  });

  // TODO: Remove .skip when the test is able to run. After adding a language to a package and saving. The language is not saved or anything.
  test.skip('can create a package with languages', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const languageId = await umbracoApi.language.createDefaultDanishLanguage();
    await umbracoApi.package.createEmptyPackage(packageName);
    await umbracoUi.reloadPage();
    const languageData = await umbracoApi.language.get(languageId);
    const languageName = languageData.name;

    // Act
    await umbracoUi.package.clickExistingPackageName(packageName);
    await umbracoUi.package.clickAddLanguageToPackageButton();
    await umbracoUi.package.clickCaretButton();
    await umbracoUi.package.clickLabelWithName(languageName);
    await umbracoUi.package.clickSubmitButton()
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.clickExistingPackageName(packageName);
    expect(umbracoUi.package.isButtonWithNameVisible(languageName + ' ' + languageId)).toBeTruthy();
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.languages[0] == languageId).toBeTruthy();

    // Clean
    await umbracoApi.language.ensureNameNotExists(languageName);
  });

  // TODO: Remove .skip when the test is able to run. Currently waiting for button
  test.skip('can create a package with dictionary', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dictionaryName = 'TestDictionary';
    await umbracoApi.dictionary.createDefaultDictionary(dictionaryName);
    await umbracoApi.package.createEmptyPackage(packageName);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickExistingPackageName(packageName);
    await umbracoUi.package.clickAddDictionaryToPackageButton();
    await umbracoUi.package.clickCaretButton();
    await umbracoUi.package.clickLabelWithName(dictionaryName);
    await umbracoUi.package.clickSubmitButton()
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.clickExistingPackageName(packageName);
    expect(umbracoUi.package.isButtonWithNameVisible(dictionaryName)).toBeTruthy();
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.dictionaryItems[0] == dictionaryName).toBeTruthy();

    // Clean
    await umbracoApi.dictionary.ensureNameNotExists(dictionaryName);
  });

  // TODO: Remove .skip when the test is able to run. After adding a dataType to a package and saving. The datatype is not saved or anything.
  test.skip('can create a package with data types', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const dataTypeName = 'TestDataType';
    const dataTypeId = await umbracoApi.dataType.createDateTypeDataType(dataTypeName);
    await umbracoApi.package.createEmptyPackage(packageName);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickExistingPackageName(packageName);
    await umbracoUi.package.clickAddDataTypesToPackageButton();
    await umbracoUi.package.clickCaretButton();
    await umbracoUi.package.clickLabelWithName(dataTypeName);
    await umbracoUi.package.clickSubmitButton()
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.clickExistingPackageName(packageName);
    expect(umbracoUi.package.isButtonWithNameVisible(dataTypeName)).toBeTruthy();
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.dataTypes[0] == dataTypeId).toBeTruthy();

    // Clean
    await umbracoApi.dictionary.ensureNameNotExists(dataTypeName);
  });

  // TODO: Remove .skip when the test is able to run. Currently waiting for button
  test.skip('can create a package with templates', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const templateName = 'TestTemplate';
    const templateId = await umbracoApi.template.createDefaultTemplate(templateName);
    await umbracoApi.package.createEmptyPackage(packageName);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickExistingPackageName(packageName);
    await umbracoUi.package.clickAddTemplatesToPackageButton();
    await umbracoUi.package.clickCaretButton();
    await umbracoUi.package.clickLabelWithName(templateName);
    await umbracoUi.package.clickSubmitButton()
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.clickExistingPackageName(packageName);
    expect(umbracoUi.package.isButtonWithNameVisible(templateName)).toBeTruthy();
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.templates[0] == templateId).toBeTruthy();

    // Clean
    await umbracoApi.template.ensureNameNotExists(templateName);
  });

  // TODO: Remove .skip when the test is able to run. Currently waiting for button
  test.skip('can create a package with stylesheets', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const stylesheetName = 'TestStylesheet';
    const stylesheetId = await umbracoApi.stylesheet.createDefaultStylesheet(stylesheetName);
    await umbracoApi.package.createEmptyPackage(packageName);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickExistingPackageName(packageName);
    await umbracoUi.package.clickAddStylesheetToPackageButton();
    await umbracoUi.package.clickCaretButton();
    await umbracoUi.package.clickLabelWithName(stylesheetName);
    await umbracoUi.package.clickSubmitButton()
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.clickExistingPackageName(packageName);
    expect(umbracoUi.package.isButtonWithNameVisible(stylesheetName)).toBeTruthy();
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.stylesheets[0] == stylesheetId).toBeTruthy();

    // Clean
    await umbracoApi.stylesheet.ensureNameNotExists(stylesheetName);
  });

  // TODO: Remove .skip when the test is able to run. Currently waiting for button
  test.skip('can create a package with scripts', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const scriptName = 'TestScripts';
    const scriptId = await umbracoApi.script.createDefaultScript(scriptName);
    await umbracoApi.package.createEmptyPackage(packageName);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickExistingPackageName(packageName);
    await umbracoUi.package.clickAddScriptToPackageButton();
    await umbracoUi.package.clickCaretButton();
    await umbracoUi.package.clickLabelWithName(scriptName);
    await umbracoUi.package.clickSubmitButton()
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.clickExistingPackageName(packageName);
    expect(umbracoUi.package.isButtonWithNameVisible(scriptName)).toBeTruthy();
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.scripts[0] == scriptId).toBeTruthy();

    // Clean
    await umbracoApi.script.ensureNameNotExists(scriptName);
  });

  // TODO: Remove .skip when the test is able to run. Currently waiting for button
  test.skip('can create a package with partial views', async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const partialViewName = 'TestPartialView';
    const partialViewId = await umbracoApi.partialView.createDefaultPartialView(partialViewName);
    await umbracoApi.package.createEmptyPackage(packageName);
    await umbracoUi.reloadPage();

    // Act
    await umbracoUi.package.clickExistingPackageName(packageName);
    await umbracoUi.package.clickAddPartialViewToPackageButton();
    await umbracoUi.package.clickCaretButton();
    await umbracoUi.package.clickLabelWithName(partialViewName);
    await umbracoUi.package.clickSubmitButton()
    await umbracoUi.package.clickSaveChangesToPackageButton();

    // Assert
    await umbracoUi.package.clickExistingPackageName(packageName);
    expect(umbracoUi.package.isButtonWithNameVisible(partialViewName)).toBeTruthy();
    const packageData = await umbracoApi.package.getByName(packageName);
    expect(packageData.partialViews[0] == partialViewId).toBeTruthy();

    // Clean
    await umbracoApi.package.ensureNameNotExists(packageName);
  });

  // Currently you are not able to download a package
  //TODO: Remove skip when the frontend is ready
  test.skip('can download a package', async ({umbracoApi, umbracoUi}) => {
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
});
