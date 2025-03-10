import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let dataTypeId = '';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const childContentName = 'ChildContent';
const childDocumentTypeName = 'ChildDocumentTypeForContent';
const dataTypeName = 'Textstring';
const contentText = 'This is test content text';
const scheduleWaitTime = 150000;

test.beforeEach(async ({umbracoApi}) => {
  // Need to increase the timeout of the tests due to the time to wait for publishing
  test.setTimeout(200000);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeId = dataTypeData.id;
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can schedule the publishing of invariant unpublish content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  console.log(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before publishing is Unpublished
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
  // verify the value of "Publish At"
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
  // verify the status of content after the publish time is Published
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  console.log(await umbracoApi.getCurrentTimePlusMinute(0));
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Published');
  // verify the value of "Last published"
  await umbracoUi.content.doesLastPublishedContainText(publishedTime.substring(0, publishedTime.length - 5));
});

test('can schedule the publishing of invariant published content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publish(contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterTextstring('Updated text');
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before publishing is Published (pending changes)
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  // verify the value of "Publish At"
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
  // verify the status of content after the publish time is Published
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Published');
  // verify the value of "Last published"
  await umbracoUi.content.doesLastPublishedContainText(publishedTime.substring(0, publishedTime.length - 5));
});

test('can schedule the publishing of variant unpublish content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before publishing is Unpublished
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
  // verify the value of "Publish At"
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
  // verify the status of content after the publish time is Published
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Published');
  // verify the value of "Last published"
  await umbracoUi.content.doesLastPublishedContainText(publishedTime.substring(0, publishedTime.length - 5));
});

test('can schedule the publishing of variant published content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publishDocumentWithCulture(contentId, 'en-US');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.enterTextstring('Updated text');
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before publishing is Published (pending changes)
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  // verify the value of "Publish At"
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
  // verify the status of content after the publish time is Published
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Published');
  // verify the value of "Last published"
  await umbracoUi.content.doesLastPublishedContainText(publishedTime.substring(0, publishedTime.length - 5));
});

test('can schedule the publishing of invariant unpublish child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.publish(contentId);
  await umbracoApi.document.createDocumentWithTextContentAndParent(childContentName, childDocumentTypeId, contentText, dataTypeName, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before publishing is Unpublished
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
  // verify the value of "Publish At"
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
  // verify the status of content after the publish time is Published
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe('Published');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Published');
  // verify the value of "Last published"
  await umbracoUi.content.doesLastPublishedContainText(publishedTime.substring(0, publishedTime.length - 5));
});

test('can schedule the publishing of variant unpublish child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.publish(contentId);
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContentAndParent(childContentName, childDocumentTypeId, contentText, dataTypeName, contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before publishing is Unpublished
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
  // verify the value of "Publish At"
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
  // verify the status of content after the publish time is Published
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe('Published');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Published');
  // verify the value of "Last published"
  await umbracoUi.content.doesLastPublishedContainText(publishedTime.substring(0, publishedTime.length - 5));
});

test('can schedule the publishing of invariant published child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.publish(contentId);
  const childContentId = await umbracoApi.document.createDocumentWithTextContentAndParent(childContentName, childDocumentTypeId, contentText, dataTypeName, contentId);
  await umbracoApi.document.publish(childContentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before publishing is Published (pending changes)
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  // verify the value of "Publish At"
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
  // verify the status of content after the publish time is Published
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe('Published');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Published');
  // verify the value of "Last published"
  await umbracoUi.content.doesLastPublishedContainText(publishedTime.substring(0, publishedTime.length - 5));
});

test('can schedule the publishing of variant published child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.publish(contentId);
  const childContentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContentAndParent(childContentName, childDocumentTypeId, contentText, dataTypeName, contentId);
  await umbracoApi.document.publish(childContentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before publishing is Published (pending changes)
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  // verify the value of "Publish At"
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
  // verify the status of content after the publish time is Published
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  const childContentData = await umbracoApi.document.getByName(childContentName);
  expect(childContentData.variants[0].state).toBe('Published');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Published');
  // verify the value of "Last published"
  await umbracoUi.content.doesLastPublishedContainText(publishedTime.substring(0, publishedTime.length - 5));
});

// Remove .fixme when the issue is fixed: https://github.com/umbraco/Umbraco-CMS/issues/18554
test.fixme('cannot schedule the publishing of child content if parent not published', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  const childContentId = await umbracoApi.document.createDocumentWithTextContentAndParent(childContentName, childDocumentTypeId, contentText, dataTypeName, contentId);
  await umbracoApi.document.publish(childContentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.isErrorNotificationVisible();
});

test('can schedule the publishing of multiple culture variants content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstCulture = 'en-US';
  const secondCulture = 'da';
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithTwoCulturesAndTextContent(contentName, documentTypeId, contentText, dataTypeName, firstCulture, secondCulture);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  await umbracoUi.content.clickSelectAllCheckbox();
  const firstPublishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const firstPublishedTime = await umbracoApi.convertDateFormat(firstPublishDateTime);
  await umbracoUi.content.enterPublishTime(firstPublishDateTime);
  const secondPublishDateTime = await umbracoApi.getCurrentTimePlusMinute(2);
  await umbracoUi.content.enterPublishTime(secondPublishDateTime, 1);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before publishing is Unpublished
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
  // verify the value of "Publish At"
  await umbracoUi.content.doesPublishAtContainText(firstPublishedTime);
  // verify the status of first culture after the publish time is Published
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  let contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  expect(contentData.variants[1].state).toBe('Draft');
// verify the status of both culture after the second publish time are Published
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Published');
  expect(contentData.variants[1].state).toBe('Published');
});

test('publish time cannot be in the past', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const warningMessage = 'The release date cannot be in the past';
  const pastDateTime = '2024-03-09T10:00';
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  await umbracoUi.content.enterPublishTime(pastDateTime);
  await umbracoUi.content.clickScheduleModalButton();

  // Assert
  await umbracoUi.content.doesPublishAtValidationMessageContainText(warningMessage);
});

test('unpublish time cannot be in the past', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const warningMessage = 'The expire date cannot be in the past';
  const pastDateTime = '2024-03-09T10:00';
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  await umbracoUi.content.enterUnpublishTime(pastDateTime);
  await umbracoUi.content.clickScheduleModalButton();

  // Assert
  await umbracoUi.content.doesUnpublishAtValidationMessageContainText(warningMessage);
});

test('unpublish time cannot be before publish time', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const warningMessage = 'The expire date cannot be before the release date';
  const publishDateTime = '2040-03-09T10:00';
  const unpublishDateTime = '2024-03-09T10:00';
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.content.clickScheduleModalButton();

  // Assert
  await umbracoUi.content.doesUnpublishAtValidationMessageContainText(warningMessage);
});

test('can schedule the unpublishing of invariant published content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publish(contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const unpublishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const unpublishedTime = await umbracoApi.convertDateFormat(unpublishDateTime);
  await umbracoUi.content.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before unpublishing is Published (pending changes)
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  // verify the value of "Remove At"
  await umbracoUi.content.doesRemoveAtContainText(unpublishedTime);
  // verify the status of content after the unpublish time is Unpublished
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Draft');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
});

test('can schedule the unpublishing of variant published content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publishDocumentWithCulture(contentId, 'en-US');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  // Change content
  await umbracoUi.content.enterTextstring('Updated text');
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const unpublishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const unpublishedTime = await umbracoApi.convertDateFormat(unpublishDateTime);
  await umbracoUi.content.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before unpublishing is Published (pending changes)
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  // verify the value of "Remove At"
  await umbracoUi.content.doesRemoveAtContainText(unpublishedTime);
  // verify the status of content after the unpublish time is Unpublished
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.variants[0].state).toBe('Draft');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
});

test('can schedule the unpublishing of invariant published child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.publish(contentId);
  const childContentId = await umbracoApi.document.createDocumentWithTextContentAndParent(childContentName, childDocumentTypeId, contentText, dataTypeName, contentId);
  await umbracoApi.document.publish(childContentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const unpublishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const unpublishedTime = await umbracoApi.convertDateFormat(unpublishDateTime);
  await umbracoUi.content.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before unpublishing is Published (pending changes)
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  // verify the value of "Remove At"
  await umbracoUi.content.doesRemoveAtContainText(unpublishedTime);
  // verify the status of content after the unpublish time is Unpublished
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  const contentData = await umbracoApi.document.getByName(childContentName);
  expect(contentData.variants[0].state).toBe('Draft');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
});

test('can schedule the unpublishing of variant published child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithAllowedChildNode(documentTypeName, childDocumentTypeId);
  const contentId = await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoApi.document.publish(contentId);
  const childContentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContentAndParent(childContentName, childDocumentTypeId, contentText, dataTypeName, contentId);
  await umbracoApi.document.publish(childContentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickScheduleButton();
  const unpublishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const unpublishedTime = await umbracoApi.convertDateFormat(unpublishDateTime);
  await umbracoUi.content.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.content.clickScheduleModalButton();
  
  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  // verify the status of content before unpublishing is Published (pending changes)
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  // verify the value of "Remove At"
  await umbracoUi.content.doesRemoveAtContainText(unpublishedTime);
  // verify the status of content after the unpublish time is Unpublished
  await umbracoUi.waitForTimeout(scheduleWaitTime);
  const contentData = await umbracoApi.document.getByName(childContentName);
  expect(contentData.variants[0].state).toBe('Draft');
  await umbracoUi.reloadPage();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
});