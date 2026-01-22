import {ConstantHelper, NotificationConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

let elementTypeId = '';
let elementId = '';
const elementName = 'TestElement';
const elementTypeName = 'TestElementTypeForElement3';
const dataTypeName = 'Textstring';
const elementText = 'This is test element text';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataTypeData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can create empty element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Draft';
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickActionsMenuAtRoot();
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementButton();
  await umbracoUi.library.clickModalMenuItemWithName(elementTypeName);
  await umbracoUi.library.clickChooseModalButton();
  await umbracoUi.library.enterElementName(elementName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.variants[0].state).toBe(expectedState);
});

test('can save and publish empty element', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const expectedState = 'Published';
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickActionsMenuAtRoot();
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementButton();
  await umbracoUi.library.clickModalMenuItemWithName(elementTypeName);
  await umbracoUi.library.clickChooseModalButton();
  await umbracoUi.library.enterElementName(elementName);
  await umbracoUi.library.clickSaveAndPublishButtonAndWaitForElementToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.variants[0].state).toBe(expectedState);
});

test('can create element', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickActionsMenuAtRoot();
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementButton();
  await umbracoUi.library.clickModalMenuItemWithName(elementTypeName);
  await umbracoUi.library.clickChooseModalButton();
  await umbracoUi.library.enterElementName(elementName);
  await umbracoUi.library.enterTextstring(elementText);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeCreated();

  // Assert
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.values[0].value).toBe(elementText);
});

test('can rename element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongElementName = 'Wrong Element Name';
  elementId = await umbracoApi.element.createDefaultElement(wrongElementName, elementTypeId);
  expect(await umbracoApi.element.doesNameExist(wrongElementName)).toBeTruthy();
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(wrongElementName);
  await umbracoUi.library.enterElementName(elementName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();

  // Assert
  const updatedelementData = await umbracoApi.element.get(elementId);
  expect(updatedelementData.variants[0].name).toEqual(elementName);
});

test('can update element', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const wrongElementText = 'This is wrong test element text';
  elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, wrongElementText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementName);
  await umbracoUi.library.enterTextstring(elementText);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();

  // Assert
  const updatedelementData = await umbracoApi.element.getByName(elementName);
  expect(updatedelementData.values[0].value).toBe(elementText);
});

test('can unpublish element', {tag: '@smoke'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoApi.element.publish(elementId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickUnpublishActionMenuOption();
  await umbracoUi.library.clickConfirmToUnpublishButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.unpublished);
  const elementData = await umbracoApi.element.getByName(elementName);
  expect(elementData.variants[0].state).toBe('Draft');
});

test('can duplicate a element node to root', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const duplicatedElementName = elementName + ' (1)';
  elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  // Duplicate to root
  await umbracoUi.library.clickDuplicateToActionMenuOption();
  await umbracoUi.library.clickLabelWithName('Elements');
  await umbracoUi.library.clickDuplicateButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);
  expect(await umbracoApi.element.doesNameExist(elementName)).toBeTruthy();
  expect(await umbracoApi.element.doesNameExist(duplicatedElementName)).toBeTruthy();
  await umbracoUi.library.isElementInTreeVisible(elementName);
  await umbracoUi.library.isElementInTreeVisible(duplicatedElementName);
  const elementData = await umbracoApi.element.getByName(elementName);
  const duplicatedElementData = await umbracoApi.element.getByName(duplicatedElementName);
  expect(elementData.values[0].value).toEqual(duplicatedElementData.values[0].value);

  // Clean
  await umbracoApi.element.ensureNameNotExists(duplicatedElementName);
});

test('can duplicate a element node to other parent', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementFolderName = 'TestElementFolder';
  await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickDuplicateToActionMenuOption();
  await umbracoUi.library.clickModalMenuItemWithName(elementFolderName);
  await umbracoUi.library.clickDuplicateButton();

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.duplicated);
  await umbracoUi.library.isElementInTreeVisible(elementName);
  await umbracoUi.library.isElementInTreeVisible(elementFolderName);
  await umbracoUi.library.goToElementWithName(elementFolderName);

  // Clean
  await umbracoApi.element.ensureNameNotExists(elementFolderName);
});

test('can move a element node to other parent', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const elementFolderName = 'TestElementFolder';
  const elementFolderId = await umbracoApi.element.createDefaultElementFolder(elementFolderName);
  elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, elementText, dataTypeName);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act;
  await umbracoUi.library.clickActionsMenuForElement(elementName);
  await umbracoUi.library.clickMoveToActionMenuOption();
  await umbracoUi.library.moveToElementWithName([], elementFolderName);

  // Assert
  await umbracoUi.library.doesSuccessNotificationHaveText(NotificationConstantHelper.success.moved);
  await umbracoUi.library.clickActionsMenuForElement(elementFolderName);
  await umbracoUi.library.clickReloadChildrenActionMenuOption();
  await umbracoUi.library.isCaretButtonVisibleForElementName(elementFolderName, true);
  await umbracoUi.library.openElementCaretButtonForName(elementFolderName);
  await umbracoUi.library.isChildElementInTreeVisible(elementFolderName, elementName, true);
  await umbracoUi.library.isCaretButtonVisibleForElementName(elementName, false);
  expect(await umbracoApi.element.getChildrenAmount(elementFolderId)).toEqual(1);
});
