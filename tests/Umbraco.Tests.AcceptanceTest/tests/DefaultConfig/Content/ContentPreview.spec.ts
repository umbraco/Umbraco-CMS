import {AliasHelper, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

const contentName = 'TestPreviewContent';
const documentTypeName = 'TestDocumentTypeForPreview';
const dataTypeName = 'Textstring';
const templateName = 'TestTemplateForPreview';
const propertyName = 'Test Textstring';
const contentText = 'Preview test content';
let contentId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const templateId = await umbracoApi.template.createTemplateWithDisplayingStringValue(templateName, AliasHelper.toAlias(propertyName));
  contentId = await umbracoApi.document.createPublishedDocumentWithValue(contentName, contentText, dataTypeData.id, templateId, propertyName, documentTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.language.ensureNameNotExists('Danish');
});

test('can preview published content', async ({umbracoUi}) => {
  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPreviewButton();
  await umbracoUi.preview.waitForPreviewPage();

  // Assert
  await umbracoUi.preview.isExitButtonVisible();
  await umbracoUi.preview.isPreviewWebsiteButtonVisible();
  await umbracoUi.preview.isDeviceButtonVisible();
  await umbracoUi.preview.isIframeAttached();
  await umbracoUi.preview.doesIframeContainText(contentText);
});

test('can switch device in preview', async ({umbracoUi}) => {
  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPreviewButton();
  await umbracoUi.preview.waitForPreviewPage();
  await umbracoUi.preview.clickDeviceButton();
  await umbracoUi.preview.clickDeviceByName('Smartphone portrait');

  // Assert
  await umbracoUi.preview.isDeviceActive('Smartphone portrait');
});

test('can preview updated content before publishing', async ({umbracoUi}) => {
  // Arrange
  const updatedText = 'Updated preview content';

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterTextstring(updatedText);
  await umbracoUi.content.clickSaveAndPreviewButton();
  await umbracoUi.preview.waitForPreviewPage();

  // Assert
  await umbracoUi.preview.doesIframeContainText(updatedText);
});

test('can open published URL from preview using preview button', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const publishedUrl = await umbracoApi.document.getDocumentUrl(contentId);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPreviewButton();
  await umbracoUi.preview.waitForPreviewPage();
  await umbracoUi.preview.isPreviewWebsiteButtonVisible();
  const websitePage = await umbracoUi.preview.clickPreviewWebsiteButtonAndWaitForWebsite();

  // Assert
  const websiteUrl = websitePage.url();
  expect(websiteUrl).toContain(publishedUrl);
  expect(websiteUrl).not.toContain('/preview');
});

test('can switch culture in preview with multiple languages', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.createDanishLanguage();

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPreviewButton();
  await umbracoUi.preview.waitForPreviewPage();
  await umbracoUi.preview.clickCultureButton();
  await umbracoUi.preview.clickCultureByName('Danish');

  // Assert
  await umbracoUi.preview.isCultureActive('Danish');
});

test('can exit preview mode', async ({umbracoUi}) => {
  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSaveAndPreviewButton();
  await umbracoUi.preview.waitForPreviewPage();
  await umbracoUi.preview.isExitButtonVisible();
  await umbracoUi.preview.clickExitButton();

  // Assert
  await umbracoUi.preview.isPreviewPageClosed();
  await umbracoUi.content.doesTextStringHaveExpectedValue(contentText);
});
