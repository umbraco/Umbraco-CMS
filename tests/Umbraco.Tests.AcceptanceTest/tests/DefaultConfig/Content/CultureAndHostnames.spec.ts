import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let documentTypeId = '';
let contentId = '';
const contentName = 'TestCultureAndHostnames';
const documentTypeName = 'TestDocumentTypeForContent';
const languageName = 'Danish';
const isoCode = 'da';
const domainName = 'testdomain';
const domainValue = [
    {
      "domainName": domainName,
      "isoCode": isoCode
    }
];

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.language.ensureNameNotExists(languageName);
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.language.create(languageName, false, false, isoCode);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureNameNotExists(languageName);
});

test('can add a culture', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCultureAndHostnamesActionMenuOption();
  await umbracoUi.content.selectCultureLanguageOption(languageName);
  await umbracoUi.content.clickSaveModalButtonAndWaitForDomainToBeCreated();

  // Assert
  const domainsData = await umbracoApi.document.getDomains(contentId);
  expect(domainsData.defaultIsoCode).toEqual(isoCode);
});

test('can add a domain', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCultureAndHostnamesActionMenuOption();
  await umbracoUi.content.clickAddNewHostnameButton();
  await umbracoUi.content.enterDomain(domainName, 0);
  await umbracoUi.content.selectHostnameLanguageOption(languageName, 0);
  await umbracoUi.content.clickSaveModalButtonAndWaitForDomainToBeCreated();

  // Assert
  const domainsData = await umbracoApi.document.getDomains(contentId);
  expect(domainsData.domains.length).toBe(1);
  expect(domainsData.domains[0].domainName).toEqual(domainName);
  expect(domainsData.domains[0].isoCode).toEqual(isoCode);
});

test('can update culture and hostname', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedDomainName = 'updateddomain';
  let domainsData = await umbracoApi.document.getDomains(contentId);
  domainsData.domains = domainValue;
  await umbracoApi.document.updateDomains(contentId, domainsData);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCultureAndHostnamesActionMenuOption();
  await umbracoUi.content.enterDomain(updatedDomainName);
  await umbracoUi.content.clickSaveModalButtonAndWaitForDomainToBeCreated();

  // Assert
  domainsData = await umbracoApi.document.getDomains(contentId);
  expect(domainsData.domains[0].domainName).toEqual(updatedDomainName);
  expect(domainsData.domains[0].isoCode).toEqual(isoCode);
});

test('can delete culture and hostname', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  let domainsData = await umbracoApi.document.getDomains(contentId);
  domainsData.domains = domainValue;
  await umbracoApi.document.updateDomains(contentId, domainsData);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCultureAndHostnamesActionMenuOption();
  await umbracoUi.content.clickDeleteHostnameButton();
  await umbracoUi.content.clickSaveModalButtonAndWaitForDomainToBeCreated();

  // Assert
  domainsData = await umbracoApi.document.getDomains(contentId);
  expect(domainsData.domains.length).toBe(0);
});

test('can add culture and hostname for multiple languages', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const secondDomainName = 'testdomain2';
  const secondLanguageName = 'Vietnamese';
  const secondIsoCode = 'vi';
  await umbracoApi.language.ensureNameNotExists(secondLanguageName);
  await umbracoApi.language.create(secondLanguageName, false, false, secondIsoCode);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickCultureAndHostnamesActionMenuOption();
  await umbracoUi.content.clickAddNewHostnameButton();
  await umbracoUi.content.enterDomain(domainName, 0);
  await umbracoUi.content.selectHostnameLanguageOption(languageName, 0);
  await umbracoUi.content.clickAddNewHostnameButton();
  await umbracoUi.content.enterDomain(secondDomainName, 1);
  await umbracoUi.content.selectHostnameLanguageOption(secondLanguageName, 1);
  await umbracoUi.content.clickSaveModalButtonAndWaitForDomainToBeCreated();

  // Assert
  const domainsData = await umbracoApi.document.getDomains(contentId);
  expect(domainsData.domains.length).toBe(2);
  expect(domainsData.domains[0].domainName).toEqual(domainName);
  expect(domainsData.domains[0].isoCode).toEqual(isoCode);
  expect(domainsData.domains[1].domainName).toEqual(secondDomainName);
  expect(domainsData.domains[1].isoCode).toEqual(secondIsoCode);

  // Clean
  await umbracoApi.language.ensureNameNotExists(secondLanguageName);
});
