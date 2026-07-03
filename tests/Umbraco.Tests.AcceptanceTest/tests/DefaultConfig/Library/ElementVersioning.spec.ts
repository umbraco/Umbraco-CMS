import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

let elementTypeId = '';
let elementId = '';
const elementName = 'TestElement';
const elementTypeName = 'TestElementTypeForElementVersioning';
const dataTypeName = 'Textstring';
const originalText = 'Original version text';
const updatedText = 'Updated version text';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  elementTypeId = await umbracoApi.documentType.createElementTypeWithPropertyInTab(elementTypeName, 'TestTab', 'TestGroup', dataTypeName, dataTypeData.id);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
});

test('can rollback an element to a previous published version', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, originalText, dataTypeName);
  await umbracoApi.element.publish(elementId);
  const elementData = await umbracoApi.element.get(elementId);
  elementData.values[0].value = updatedText;
  await umbracoApi.element.update(elementId, elementData);
  await umbracoApi.element.publish(elementId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);

  // Act
  await umbracoUi.library.clickInfoTab();
  await umbracoUi.library.clickRollbackButton();
  await umbracoUi.library.waitForRollbackItems();
  await umbracoUi.library.clickLatestRollBackItem();
  await umbracoUi.library.clickRollbackContainerButton();

  // Assert
  await umbracoUi.library.isSuccessNotificationVisible();
  await umbracoUi.library.clickContentTab();
  await umbracoUi.library.doesElementPropertyHaveValue(dataTypeName, originalText);
  const updatedElementData = await umbracoApi.element.get(elementId);
  expect(updatedElementData.values[0].value).toBe(originalText);
  // Verify audit trail
  await umbracoUi.library.clickInfoTab();
  await umbracoUi.library.doesHistoryItemHaveTag(ConstantHelper.auditTrailTypes.rollback);
  await umbracoUi.library.doesHistoryItemHaveDescription(ConstantHelper.auditTrailMessages.elementRolledBack);
  const currentUser = await umbracoApi.user.getCurrentUser();
  await umbracoUi.library.doesHistoryItemHaveUsername(currentUser.name);
});

test('rollback restores the element name to the previous version', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const renamedElementName = 'TestElementRenamed';
  await umbracoApi.element.ensureNameNotExists(renamedElementName);
  elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, originalText, dataTypeName);
  await umbracoApi.element.publish(elementId);
  const elementData = await umbracoApi.element.get(elementId);
  elementData.variants[0].name = renamedElementName;
  elementData.values[0].value = updatedText;
  await umbracoApi.element.update(elementId, elementData);
  await umbracoApi.element.publish(elementId);
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(renamedElementName);

  // Act
  await umbracoUi.library.clickInfoTab();
  await umbracoUi.library.clickRollbackButton();
  await umbracoUi.library.waitForRollbackItems();
  await umbracoUi.library.clickLatestRollBackItem();
  await umbracoUi.library.clickRollbackContainerButton();

  // Assert
  await umbracoUi.library.isSuccessNotificationVisible();
  const rolledBackElementData = await umbracoApi.element.get(elementId);
  expect(rolledBackElementData.variants[0].name).toBe(elementName);
  expect(rolledBackElementData.values[0].value).toBe(originalText);

  // Clean
  await umbracoApi.element.ensureNameNotExists(renamedElementName);
});

test('can rollback a variant element to a previous published version', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const variantElementTypeName = 'VariantElementTypeForVersioning';
  const variantPropertyName = 'TestVariantText';
  const danishElementName = 'TestElementDanish';
  const danishText = 'Original Danish text';
  await umbracoApi.language.createDanishLanguage();
  await umbracoApi.element.ensureNameNotExists(danishElementName);
  await umbracoApi.documentType.ensureNameNotExists(variantElementTypeName);
  const dataTypeData = await umbracoApi.dataType.getByName(dataTypeName);
  const variantElementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(variantElementTypeName, 'Content', variantPropertyName, dataTypeData.id, true, true);
  elementId = await umbracoApi.element.createDefaultElementWithEnglishAndDanishVariants(variantElementTypeId, elementName, danishElementName, variantPropertyName, originalText, danishText);
  await umbracoApi.element.publish(elementId, {publishSchedules: [{culture: 'en-US'}, {culture: 'da'}]});
  const elementData = await umbracoApi.element.get(elementId);
  const englishValue = elementData.values.find((v) => v.culture === 'en-US');
  if (englishValue) {
    englishValue.value = updatedText;
  }
  await umbracoApi.element.update(elementId, elementData);
  await umbracoApi.element.publish(elementId, {publishSchedules: [{culture: 'en-US'}, {culture: 'da'}]});
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);
  await umbracoUi.library.goToElementWithName(elementName);

  // Act
  await umbracoUi.library.clickInfoTab();
  await umbracoUi.library.clickRollbackButton();
  await umbracoUi.library.waitForRollbackItems();
  await umbracoUi.library.clickLatestRollBackItem();
  await umbracoUi.library.clickRollbackContainerButton();

  // Assert
  await umbracoUi.library.isSuccessNotificationVisible();
  const rolledBackElementData = await umbracoApi.element.get(elementId);
  const rolledBackEnglishValue = rolledBackElementData.values.find((v) => v.culture === 'en-US');
  expect(rolledBackEnglishValue.value).toBe(originalText);

  // Clean
  await umbracoApi.element.ensureNameNotExists(danishElementName);
  await umbracoApi.documentType.ensureNameNotExists(variantElementTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists('da');
});
