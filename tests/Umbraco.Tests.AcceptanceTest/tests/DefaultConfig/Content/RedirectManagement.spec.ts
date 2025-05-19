import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const disableStatus = 'Disabled';
const enableStatus = 'Enabled';
let documentTypeId = '';
let contentId = '';
const contentName = 'TestContentRedirectURL';
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
  await umbracoApi.document.publish(contentId);
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
  // Rename the published content
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterContentName(updatedContentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  // Verify that there is no redirects have been made
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.isTextWithExactNameVisible(contentUrl, false);
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
  // Rename the published content
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterContentName(updatedContentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  // Verify that there is one redirects have been made
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.isTextWithExactNameVisible(contentUrl);
  // Verify that the status is Enable
  const statusData = await umbracoApi.redirectManagement.getStatus();
  expect(statusData.status).toBe(enableStatus);
});

test('can search for original URL', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const searchKeyword = 'redirect';
  // Rename the published content
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterContentName(updatedContentName);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.enterOriginalUrl(searchKeyword);
  await umbracoUi.redirectManagement.clickSearchButton();

  // Assert
  await umbracoUi.redirectManagement.doesRedirectManagementRowsHaveCount(1);
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);
  await umbracoUi.redirectManagement.isTextWithExactNameVisible(contentUrl);
});

test('can delete a redirect', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Rename the published content
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterContentName(updatedContentName);
  await umbracoUi.content.clickSaveAndPublishButton();
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);

  // Act
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.redirectManagement.clickRedirectManagementTab();
  await umbracoUi.redirectManagement.isTextWithExactNameVisible(contentUrl);
  await umbracoUi.redirectManagement.deleteFirstRedirectURL();

  // Assert
  await umbracoUi.redirectManagement.isTextWithExactNameVisible(contentUrl, false);
});
