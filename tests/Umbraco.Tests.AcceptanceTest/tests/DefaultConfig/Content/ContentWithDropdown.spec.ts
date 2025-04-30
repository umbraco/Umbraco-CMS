import {ConstantHelper, test, AliasHelper, NotificationConstantHelper} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

const contentName = 'TestContent';
const documentTypeName = 'TestDocumentTypeForContent';
const dataTypeNames = ['Dropdown', 'Dropdown multiple'];
const customDataTypeName = 'CustomDropdown';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName); 
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.dataType.ensureNameNotExists(customDataTypeName);
});

for (const dataTypeName of dataTypeNames) {
  test(`can create content with the ${dataTypeName} data type`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedState = 'Draft';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  
    // Act
    await umbracoUi.content.clickActionsMenuAtRoot();
    await umbracoUi.content.clickCreateButton();
    await umbracoUi.content.chooseDocumentType(documentTypeName);
    await umbracoUi.content.enterContentName(contentName);
    await umbracoUi.content.clickSaveButton();
  
    // Assert
    await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.created);
    expect(await umbracoApi.document.doesNameExist(contentName)).toBeTruthy();
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.variants[0].state).toBe(expectedState);
    expect(contentData.values).toEqual([]);
  });
  
  test(`can publish content with the ${dataTypeName} data type`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const expectedState = 'Published';
    const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, dataTypeName, dataTypeData.id);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  
    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.clickSaveAndPublishButton();
  
    // Assert
    await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
    await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.variants[0].state).toBe(expectedState);
    expect(contentData.values).toEqual([]);
  });
  
  test(`can create content with the custom ${dataTypeName} data type`, async ({umbracoApi, umbracoUi}) => {
    // Arrange
    const optionValues = ['testOption1', 'testOption2', 'testOption3'];
    const selectedOptions = dataTypeName === 'Dropdown' ? [optionValues[0]] : optionValues;
    const isMultiple = dataTypeName === 'Dropdown' ? false : true;
    const customDataTypeId = await umbracoApi.dataType.createDropdownDataType(customDataTypeName, isMultiple, optionValues);
    const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId);
    await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
    await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  
    // Act
    await umbracoUi.content.goToContentWithName(contentName);
    await umbracoUi.content.chooseDropdownOption(selectedOptions);
    await umbracoUi.content.clickSaveAndPublishButton();
  
    // Assert
    await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
    await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
    const contentData = await umbracoApi.document.getByName(contentName);
    expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(customDataTypeName));
    expect(contentData.values[0].value).toEqual(selectedOptions);
  });
}

test('can not publish a mandatory dropdown with an empty value', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const optionValues = ['testOption1', 'testOption2', 'testOption3'];
  const customDataTypeId = await umbracoApi.dataType.createDropdownDataType(customDataTypeName, false, optionValues);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(documentTypeName, customDataTypeName, customDataTypeId, 'Test Group', false, false, true);
  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);

  // Act
  await umbracoUi.content.goToContentWithName(contentName);
  // Do not select any dropdown values and the validation error appears
  await umbracoUi.content.clickSaveAndPublishButton();
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesErrorNotificationHaveText(NotificationConstantHelper.error.documentCouldNotBePublished);
  // Select a dropdown value and the validation error disappears
  await umbracoUi.content.chooseDropdownOption([optionValues[0]]);
  await umbracoUi.content.isValidationMessageVisible(ConstantHelper.validationMessages.emptyValue, false);
  await umbracoUi.content.clickSaveAndPublishButton();

  // Assert
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.saved);
  await umbracoUi.content.doesSuccessNotificationHaveText(NotificationConstantHelper.success.published);
  const contentData = await umbracoApi.document.getByName(contentName);
  expect(contentData.values[0].alias).toEqual(AliasHelper.toAlias(customDataTypeName));
  expect(contentData.values[0].value).toEqual([optionValues[0]]);
});