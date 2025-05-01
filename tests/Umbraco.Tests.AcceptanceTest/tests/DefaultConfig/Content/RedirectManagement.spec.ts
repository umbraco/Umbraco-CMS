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
  await umbracoUi.goToBackOffice();
  // Create a content
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  documentTypeId = await umbracoApi.documentType.createDefaultDocumentTypeWithAllowAsRoot(documentTypeName);
  contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  // Publish the content
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickPublishButton();
  await umbracoUi.content.clickConfirmToPublishButton();
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
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.isTextWithExactNameVisible(contentUrl, false);
  // Verify that the status is Disable
  const statusData = await umbracoApi.redirectManagement.getStatus();
  expect(statusData.status).toBe(disableStatus);
});

// TODO: Remove skip when the frond-end is ready. Currently there is no redirect have been made after renaming a published page
test.skip('can re-enable URL tracker', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.redirectManagement.setStatus(disableStatus);

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.clickEnableURLTrackerButton();

  // Assert
  // Verify that if renaming a published page, there are one redirects have been made
  // rename the published content
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterContentName(updatedContentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  // verify that there is one redirects have been made
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.isTextWithExactNameVisible(contentUrl);
  // Verify that the status is Enable
  const statusData = await umbracoApi.redirectManagement.getStatus();
  expect(statusData.status).toBe(enableStatus);
});

// TODO: Remove skip and update this when the front-end is ready. Currently it always return "No redirects matching this search criteria" when searching.
test.skip('can search for original URL', async ({umbracoUi}) => {
  // Arrange
  const searchKeyword = '/test-content/';
  // TODO: rename content to add an item in the redirect url management

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.enterOriginalUrl(searchKeyword);
  await umbracoUi.redirectManagement.clickSearchButton();

  // Assert
  // TODO: verify the search result
});

// TODO: Remove skip when the frond-end is ready. Currently there is no redirect have been made after renaming a published page
test.skip('can delete a redirect', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Rename the published content
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterContentName(updatedContentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.deleteFirstRedirectURL();

  // Assert
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);
  await umbracoUi.redirectManagement.isTextWithExactNameVisible(contentUrl, false);
});
