import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from "@playwright/test";

// Content
const contentName = 'TestSegmentScheduleContent';
// Element
const elementName = 'TestSegmentScheduleElement';
// DocumentType
const documentTypeName = 'TestSegmentScheduleDocType';
// Element Type
const elementTypeName = 'TestSegmentScheduleElementType';
// DataType
const dataTypeName = 'Textstring';
const editorAlias = 'Umbraco.TextBox';
// Segments
const vipMemberSegment = 'VIP members';
const vipMemberSegmentAlias = 'vip-members';
// Test Values
const defaultSegmentValue = 'Default segment value';
const vipSegmentValue = 'VIP segment value';
const updatedVipSegmentValue = 'Updated VIP segment value';

// The acceptance-test site suspends the scheduled publishing job, so these tests assert the schedule is stored and
// the published values are untouched, not that the job later runs.
test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can schedule the publishing of an updated VIP segment value on segment-only content', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id, 'TestGroup', false, false, false, true, true);
  const documentId = await umbracoApi.document.createDocumentWithMultipleSegmentValues(contentName, documentTypeId, dataTypeName, editorAlias, [
    {value: defaultSegmentValue, segment: null},
    {value: vipSegmentValue, segment: vipMemberSegmentAlias},
  ]);
  await umbracoApi.document.publish(documentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.goToContentWithName(contentName);
  await umbracoUi.content.clickSelectVariantButton();
  await umbracoUi.content.clickSegmentVariantButton(vipMemberSegment);

  // Act
  await umbracoUi.content.enterTextstring(updatedVipSegmentValue);
  await umbracoUi.content.clickViewMoreOptionsButton();
  await umbracoUi.content.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  await umbracoUi.content.enterPublishTime(publishDateTime);
  await umbracoUi.content.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  const contentData = await umbracoApi.document.get(documentId);
  expect(contentData.variants[0].state).toBe('PublishedPendingChanges');
  expect(contentData.variants[0].scheduledPublishDate).toBeTruthy();
  const draftVip = contentData.values.find(value => value.culture === null && value.segment === vipMemberSegmentAlias);
  expect(draftVip).toBeTruthy();
  expect(draftVip.value).toBe(updatedVipSegmentValue);
  const [publishedDefault, publishedVip] = await umbracoApi.document.getPublishedValuesByCultureAndSegmentForDocument(documentId, [
    {culture: null, segment: null},
    {culture: null, segment: vipMemberSegmentAlias},
  ]);
  expect(publishedDefault).toBeTruthy();
  expect(publishedDefault.value).toBe(defaultSegmentValue);
  expect(publishedVip).toBeTruthy();
  expect(publishedVip.value).toBe(vipSegmentValue);
});

test('can schedule the publishing of an updated VIP segment value on a Library element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryBySegment(elementTypeName, 'TestGroup', dataTypeName, dataTypeData.id);
  const elementId = await umbracoApi.element.createElementWithMultipleSegmentValues(elementName, elementTypeId, dataTypeName, [
    {value: defaultSegmentValue, segment: null},
    {value: vipSegmentValue, segment: vipMemberSegmentAlias},
  ]);
  await umbracoApi.element.publish(elementId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.clickSelectVariantButton();
  await umbracoUi.library.clickSegmentVariantButton(vipMemberSegment);

  // Act
  await umbracoUi.library.enterTextstring(updatedVipSegmentValue);
  await umbracoUi.library.clickViewMoreOptionsButton();
  await umbracoUi.library.clickSchedulePublishButton();
  const publishDateTime = await umbracoApi.getCurrentTimePlusMinute();
  await umbracoUi.library.enterPublishTime(publishDateTime);
  await umbracoUi.library.clickSchedulePublishModalButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.schedulePublishingUpdated);
  const elementData = await umbracoApi.element.get(elementId);
  expect(elementData.variants[0].state).toBe('PublishedPendingChanges');
  expect(elementData.variants[0].scheduledPublishDate).toBeTruthy();
  const draftVip = elementData.values.find(value => value.culture === null && value.segment === vipMemberSegmentAlias);
  expect(draftVip).toBeTruthy();
  expect(draftVip.value).toBe(updatedVipSegmentValue);
  const [publishedDefault, publishedVip] = await umbracoApi.element.getPublishedValuesByCultureAndSegmentForElement(elementId, [
    {culture: null, segment: null},
    {culture: null, segment: vipMemberSegmentAlias},
  ]);
  expect(publishedDefault).toBeTruthy();
  expect(publishedDefault.value).toBe(defaultSegmentValue);
  expect(publishedVip).toBeTruthy();
  expect(publishedVip.value).toBe(vipSegmentValue);
});
