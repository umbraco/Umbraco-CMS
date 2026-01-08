import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let dataTypeId = '';
const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeName = 'Textstring';
const contentText = 'This is test content text';
const referenceHeadline = ConstantHelper.trashDeleteDialogMessage.referenceHeadline;
const documentPickerName = ['TestPicker', 'DocumentTypeForPicker'];

test.beforeEach(async ({umbracoApi}) => {
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  dataTypeId = dataTypeData.id;
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.emptyRecycleBin();
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName[1]);
});

test('can trash an invariant content node', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // Verify the references list not displayed
  await umbracoUi.content.isReferenceHeadlineVisible(false);
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();
});

test('can trash a variant content node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // Verify the references list not displayed
  await umbracoUi.content.isReferenceHeadlineVisible(false);
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();
});

test('can trash a published content node', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publish(contentId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // Verify the references list not displayed
  await umbracoUi.content.isReferenceHeadlineVisible(false);
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();
});

test('can trash an invariant content node that references one item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create an invariant published content node
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publish(contentId);
  // Create a document link picker
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName[0], contentName, contentId, documentPickerName[1]);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // Verify the references list
  await umbracoUi.content.doesReferenceHeadlineHaveText(referenceHeadline);
  await umbracoUi.content.doesReferenceItemsHaveCount(1);
  await umbracoUi.content.isReferenceItemNameVisible(documentPickerName[0]);
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();
});

test('can trash a variant content node that references one item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  // Create a variant published content node
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publishDocumentWithCulture(contentId, 'en-US');
  // Create a document link picker
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName[0], contentName, contentId, documentPickerName[1]);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // Verify the references list
  await umbracoUi.content.doesReferenceHeadlineHaveText(referenceHeadline);
  await umbracoUi.content.doesReferenceItemsHaveCount(1);
  await umbracoUi.content.isReferenceItemNameVisible(documentPickerName[0]);
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();
});

test('can trash an invariant content node that references more than 3 items', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentPickerName2 = ['TestPicker2', 'DocumentTypeForPicker2'];
  const documentPickerName3 = ['TestPicker3', 'DocumentTypeForPicker3'];
  const documentPickerName4 = ['TestPicker4', 'DocumentTypeForPicker4'];
  // Create an invariant published content node
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publish(contentId);
  // Create 4 document link pickers
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName[0], contentName, contentId, documentPickerName[1]);
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName2[0], contentName, contentId, documentPickerName2[1]);
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName3[0], contentName, contentId, documentPickerName3[1]);
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName4[0], contentName, contentId, documentPickerName4[1]);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // Verify the references list has 3 items and has the text '...and one more item'
  await umbracoUi.content.doesReferenceHeadlineHaveText(referenceHeadline);
  await umbracoUi.content.doesReferenceItemsHaveCount(3);
  await umbracoUi.content.isReferenceItemNameVisible(documentPickerName[0]);
  await umbracoUi.content.isReferenceItemNameVisible(documentPickerName2[0]);
  await umbracoUi.content.isReferenceItemNameVisible(documentPickerName3[0]);
  await umbracoUi.content.doesReferencesContainText('...and one more item');
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName2[1]);
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName3[1]);
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName4[1]);
});

test('can trash a variant content node that references more than 3 items', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentPickerName2 = ['TestPicker2', 'DocumentTypeForPicker2'];
  const documentPickerName3 = ['TestPicker3', 'DocumentTypeForPicker3'];
  const documentPickerName4 = ['TestPicker4', 'DocumentTypeForPicker4'];
  // Create a variant published content node
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithEnglishCultureAndTextContent(contentName, documentTypeId, contentText, dataTypeName);
  await umbracoApi.document.publishDocumentWithCulture(contentId, 'en-US');
  // Create 4 document link pickers
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName[0], contentName, contentId, documentPickerName[1]);
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName2[0], contentName, contentId, documentPickerName2[1]);
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName3[0], contentName, contentId, documentPickerName3[1]);
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName4[0], contentName, contentId, documentPickerName4[1]);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // Verify the references list has 3 items and has the text '...and one more item'
  await umbracoUi.content.doesReferenceHeadlineHaveText(referenceHeadline);
  await umbracoUi.content.doesReferenceItemsHaveCount(3);
  await umbracoUi.content.isReferenceItemNameVisible(documentPickerName[0]);
  await umbracoUi.content.isReferenceItemNameVisible(documentPickerName2[0]);
  await umbracoUi.content.isReferenceItemNameVisible(documentPickerName3[0]);
  await umbracoUi.content.doesReferencesContainText('...and one more item');
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();

  // Clean
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName2[1]);
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName3[1]);
  await umbracoApi.documentType.ensureNameNotExists(documentPickerName4[1]);
});

test('can trash a content node with multiple cultures that references one item', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const firstCulture = 'en-US';
  const secondCulture = 'da';
  await umbracoApi.language.createDanishLanguage();
  // Create a content node with multiple cultures
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditor(documentTypeName, dataTypeName, dataTypeId);
  const contentId = await umbracoApi.document.createDocumentWithTwoCulturesAndTextContent(contentName, documentTypeId, contentText, dataTypeName, firstCulture, secondCulture);
  await umbracoApi.document.publishDocumentWithCulture(contentId, firstCulture);
  await umbracoApi.document.publishDocumentWithCulture(contentId, secondCulture);
  // Create a document link picker
  await umbracoApi.document.createDefaultDocumentWithOneDocumentLink(documentPickerName[0], contentName, contentId, documentPickerName[1]);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.clickActionsMenuForContent(contentName);
  await umbracoUi.content.clickTrashButton();
  // Verify the references list
  await umbracoUi.content.doesReferenceHeadlineHaveText(referenceHeadline);
  await umbracoUi.content.doesReferenceItemsHaveCount(1);
  await umbracoUi.content.isReferenceItemNameVisible(documentPickerName[0]);
  await umbracoUi.content.clickConfirmTrashButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.movedToRecycleBin);
  expect(await umbracoApi.document.doesNameExist(contentName)).toBeFalsy();
  await umbracoUi.content.isItemVisibleInRecycleBin(contentName);
  expect(await umbracoApi.document.doesItemExistInRecycleBin(contentName)).toBeTruthy();

  // Clean
  await umbracoApi.language.ensureIsoCodeNotExists(secondCulture);
});