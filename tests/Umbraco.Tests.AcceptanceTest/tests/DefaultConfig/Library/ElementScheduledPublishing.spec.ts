import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

let elementTypeId = '';
const elementName = 'TestElement';
const elementTypeName = 'TestElementTypeForElementScheduling';
const dataTypeName = 'Textstring';
const elementText = 'This is test element text';

// The element workspace Info tab does not yet render the scheduled "Publish at"/"Remove at" dates
// (unlike the document Info tab), so scheduling is asserted against the API instead of the UI.

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataTypeData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can schedule the publishing of an unpublished element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);

  // Act
  await umbracoUi.library.clickViewMoreOptionsButton();
  await umbracoUi.library.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  await umbracoUi.library.enterPublishTime(publishDateTime);
  await umbracoUi.library.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  const elementData = await umbracoApi.element.get(elementId);
  expect(elementData.variants[0].scheduledPublishDate).toBeTruthy();
});

test('can schedule the publishing of a published element with pending changes', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoApi.element.publish(elementId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);

  // Act
  await umbracoUi.library.enterTextstring('Updated text');
  await umbracoUi.library.clickViewMoreOptionsButton();
  await umbracoUi.library.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  await umbracoUi.library.enterPublishTime(publishDateTime);
  await umbracoUi.library.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  const elementData = await umbracoApi.element.get(elementId);
  expect(elementData.variants[0].scheduledPublishDate).toBeTruthy();
});

test('can schedule the unpublishing of a published element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoApi.element.publish(elementId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);

  // Act
  await umbracoUi.library.clickViewMoreOptionsButton();
  await umbracoUi.library.clickSchedulePublishButton();
  const unpublishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  await umbracoUi.library.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.library.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  const elementData = await umbracoApi.element.get(elementId);
  expect(elementData.variants[0].scheduledUnpublishDate).toBeTruthy();
});

test('cannot schedule publishing with a publish time in the past', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const warningMessage = 'The release date cannot be in the past';
  const pastDateTime = '2024-03-09T10:00';
  await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);

  // Act
  await umbracoUi.library.clickViewMoreOptionsButton();
  await umbracoUi.library.clickSchedulePublishButton();
  await umbracoUi.library.enterPublishTime(pastDateTime);
  await umbracoUi.library.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.library.doesPublishAtValidationMessageContainText(warningMessage);
});

test('cannot schedule unpublishing with an unpublish time before the publish time', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const warningMessage = 'The expire date cannot be before the release date';
  const publishDateTime = '2040-03-09T10:00';
  const unpublishDateTime = '2024-03-09T10:00';
  await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);

  // Act
  await umbracoUi.library.clickViewMoreOptionsButton();
  await umbracoUi.library.clickSchedulePublishButton();
  await umbracoUi.library.enterPublishTime(publishDateTime);
  await umbracoUi.library.enterUnpublishTime(unpublishDateTime);
  await umbracoUi.library.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.library.doesUnpublishAtValidationMessageContainText(warningMessage);
});
