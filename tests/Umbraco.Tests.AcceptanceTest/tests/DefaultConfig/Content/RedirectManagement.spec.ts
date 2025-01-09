import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const disableStatus = 'Disabled';
const enableStatus = 'Enabled';
let documentTypeId = '';
let contentId = '';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentType';
const updatedContentName = 'UpdatedContentName';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.redirectManagement.setStatus(enableStatus);
  // Create a published content
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.publish(contentId);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.redirectManagement.setStatus(enableStatus);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can disable URL tracker', async ({umbracoApi, umbracoUi}) => {
  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.clickDisableURLTrackerButton();
  await umbracoUi.redirectManagement.clickDisableButton();

  // Assert
  // Verify that if renaming a published page, there are no redirects have been made
  // rename the published content
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterContentName(updatedContentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  // verify that there is no redirects have been made
  const contentData = await umbracoApi.document.get(contentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.isTextWithExactNameVisible(contentData.urls[0].url, false);
  // Verify that the status is Disable
  const statusData = await umbracoApi.redirectManagement.getStatus();
  expect(statusData.status).toBe(disableStatus);
});

test('can re-enable URL tracker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.redirectManagement.setStatus(disableStatus);

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.clickEnableURLTrackerButton();
  // rename the published content
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterContentName(updatedContentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  // Verify that if renaming a published page, there are one redirects have been made
  // verify that there is one redirects have been made
  const contentData = await umbracoApi.document.get(contentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.isTextWithExactNameVisible(contentData.urls[0].url + '/');
  // Verify that the status is Enable
  const statusData = await umbracoApi.redirectManagement.getStatus();
  expect(statusData.status).toBe(enableStatus);
});

test('can search for original URL', async ({umbracoUi, umbracoApi}) => {
  // Arrange
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterContentName(updatedContentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Act
  const contentData = await umbracoApi.document.get(contentId);
  const searchKeyword = contentData.urls[0].url;
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.enterOriginalUrl(searchKeyword);
  await umbracoUi.redirectManagement.clickSearchButton();

  // Assert
  const resultData = await umbracoApi.redirectManagement.filterByText(searchKeyword);
  await umbracoUi.redirectManagement.doesRedirectManagementRowsHaveCount(resultData.total);
});

test('can delete a redirect', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Rename the published content
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterContentName(updatedContentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Act
  await umbracoUi.waitForTimeout(1000);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.deleteFirstRedirectURL();

  // Assert
  const contentData = await umbracoApi.document.get(contentId);
  await umbracoUi.redirectManagement.isTextWithExactNameVisible(contentData.urls[0].url, false);
});
