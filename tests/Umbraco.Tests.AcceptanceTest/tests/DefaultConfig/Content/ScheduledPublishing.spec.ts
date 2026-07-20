import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';

let dataTypeId = '';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const childContentName = 'ChildContent';
const childDocumentTypeName = 'ChildDocumentTypeForContent';
const dataTypeName = 'Textstring';
const contentText = 'This is test content text';

test.beforeEach(async ({umbracoApi}) => {
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeId = dataTypeData.id;
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test('can schedule the publishing of invariant unpublished content', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
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
  await umbracoUi.content.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
});

test('can schedule the publishing of variant unpublished content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
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
  await umbracoUi.content.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
});

test('can schedule the publishing of invariant unpublished child content', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
});

test('can schedule the publishing of variant unpublished child content', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
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
  await umbracoUi.content.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
});

test('can schedule the publishing of variant published child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithAllowedChildNodeAndInvariantPropertyEditor(documentTypeName, childDocumentTypeId, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publishDocumentWithCulture(contentId, 'en-US');
  const childContentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContentAndParent(childContentName, childDocumentTypeId, contentText, dataTypeName, contentId);
  await umbracoApi.document.publishDocumentWithCulture(childContentId, 'en-US');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const publishedTime = await umbracoApi.convertDateFormat(publishDateTime);
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  await umbracoUi.content.doesPublishAtContainText(publishedTime);
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
  await umbracoUi.content.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.isErrorNotificationVisible();
});

test('can schedule the publishing of multiple culture variants content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstCulture = 'en-US';
  const secondCulture = 'da';
  await umbracoApi.language.createDanishLanguage();
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithTwoCulturesAndTextContent(contentName, documentTypeId, contentText, dataTypeName, firstCulture, secondCulture);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickSchedulePublishButton();
  await umbracoUi.content.clickSelectAllCheckbox();
  const firstPublishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const firstPublishedTime = await umbracoApi.convertDateFormat(firstPublishDateTime);
  await umbracoUi.content.enterPublishTime(firstPublishDateTime);
  const secondPublishDateTime = await umbracoApi.getCurrentTimePlusMinute(2);
  await umbracoUi.content.enterPublishTime(secondPublishDateTime, 1);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Unpublished');
  await umbracoUi.content.doesPublishAtContainText(firstPublishedTime);

  // Clean
  await umbracoApi.language.ensureIsoCodeNotExists(secondCulture);
});

test('cannot schedule publishing with a publish time in the past', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.clickSchedulePublishButton();
  await umbracoUi.content.enterPublishTime(pastDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesPublishAtValidationMessageContainText(warningMessage);
});

test('cannot schedule unpublishing with an unpublish time in the past', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.clickSchedulePublishButton();
  await umbracoUi.content.enterUnpublishTime(pastDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesUnpublishAtValidationMessageContainText(warningMessage);
});

test('cannot schedule unpublishing with an unpublish time before the publish time', async ({umbracoApi, umbracoUi}) => {
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
  await umbracoUi.content.clickSchedulePublishButton();
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

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
  await umbracoUi.content.clickSchedulePublishButton();
  const unpublishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const unpublishedTime = await umbracoApi.convertDateFormat(unpublishDateTime);
  await umbracoUi.content.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  await umbracoUi.content.doesRemoveAtContainText(unpublishedTime);
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
  await umbracoUi.content.clickSchedulePublishButton();
  const unpublishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const unpublishedTime = await umbracoApi.convertDateFormat(unpublishDateTime);
  await umbracoUi.content.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  await umbracoUi.content.doesRemoveAtContainText(unpublishedTime);
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
  await umbracoUi.content.clickSchedulePublishButton();
  const unpublishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const unpublishedTime = await umbracoApi.convertDateFormat(unpublishDateTime);
  await umbracoUi.content.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  await umbracoUi.content.doesRemoveAtContainText(unpublishedTime);
});

test('can schedule the unpublishing of variant published child content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const childDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(childDocumentTypeName, dataTypeName, dataTypeId);
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithAllowedChildNodeAndInvariantPropertyEditor(documentTypeName, childDocumentTypeId, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publishDocumentWithCulture(contentId, 'en-US');
  const childContentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContentAndParent(childContentName, childDocumentTypeId, contentText, dataTypeName, contentId);
  await umbracoApi.document.publishDocumentWithCulture(childContentId, 'en-US');
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickCaretButtonForContentName(contentName);
  await umbracoUi.content.goToContentWithName(childContentName);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickSchedulePublishButton();
  const unpublishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  const unpublishedTime = await umbracoApi.convertDateFormat(unpublishDateTime);
  await umbracoUi.content.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  await umbracoUi.content.clickInfoTab();
  await umbracoUi.content.doesDocumentStateHaveText('Published (pending changes)');
  await umbracoUi.content.doesRemoveAtContainText(unpublishedTime);
});