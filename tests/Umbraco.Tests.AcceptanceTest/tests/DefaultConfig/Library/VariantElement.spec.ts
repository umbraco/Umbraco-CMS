import {ConstantHelper, test} from '@umbraco/playwright-testhelpers';
import {expect} from "@playwright/test";

// Element Type
let variantElementTypeId = '';
const variantElementTypeName = 'VariantElementType';
const mixedElementTypeName = 'MixedPropertyElementType';

// Element
const elementName = 'TestVariantElement';
const elementNameEnglish = 'Test Variant Element EN';
const elementNameDanish = 'Test Variant Element DA';

// Data Types
const textStringDataTypeName = 'Textstring';
let textStringDataTypeId = '';

// Property Names
const variantPropertyName = 'TestVariantText';
const invariantPropertyName = 'TestInvariantText';

// Test Content
const englishText = 'This is English text';
const danishText = 'Dette er dansk tekst';
const updatedEnglishText = 'Updated English text';
const invariantText = 'This is shared invariant text';

// Languages
const danishLanguage = 'Danish';
const danishIsoCode = 'da';

// Group/Tab Names
const groupName = 'Content';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.element.ensureNameNotExists(elementNameEnglish);
  await umbracoApi.element.ensureNameNotExists(elementNameDanish);
  await umbracoApi.documentType.ensureNameNotExists(variantElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(mixedElementTypeName);
  await umbracoApi.language.createDanishLanguage();
  const textStringDataType = await umbracoApi.dataType.getByName(textStringDataTypeName);
  textStringDataTypeId = textStringDataType.id;
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.element.ensureNameNotExists(elementNameEnglish);
  await umbracoApi.element.ensureNameNotExists(elementNameDanish);
  await umbracoApi.documentType.ensureNameNotExists(variantElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(mixedElementTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('can create element with multiple culture variants', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  variantElementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(
    variantElementTypeName,
    groupName,
    variantPropertyName,
    textStringDataTypeId,
    true,
    true
  );
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  // Create element with English culture
  await umbracoUi.library.clickActionsMenuAtRoot();
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementButton();
  await umbracoUi.library.clickModalMenuItemWithName(variantElementTypeName);
  await umbracoUi.library.clickChooseModalButton();
  await umbracoUi.library.enterElementName(elementNameEnglish);
  await umbracoUi.library.enterTextstring(englishText);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeCreated();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  // Add Danish variant using variant selector
  await umbracoUi.library.clickSelectVariantButton();
  await umbracoUi.library.clickVariantAddModeButtonForLanguageName(danishLanguage);
  await umbracoUi.library.enterElementName(elementNameDanish);
  await umbracoUi.library.enterTextstring(danishText);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();

  // Assert
  const elementData = await umbracoApi.element.getByName(elementNameEnglish);
  expect(elementData.variants.length).toBeGreaterThanOrEqual(2);
  const englishVariant = elementData.variants.find((variant: any) => variant.culture === 'en-US');
  expect(englishVariant).toBeDefined();
  expect(englishVariant.name).toBe(elementNameEnglish);
  expect(englishVariant.state).toBe('Draft');
  const danishVariant = elementData.variants.find((variant: any) => variant.culture === 'da');
  expect(danishVariant).toBeDefined();
  expect(danishVariant.name).toBe(elementNameDanish);
  expect(danishVariant.state).toBe('Draft');
});

test('can create element with invariant and variant properties', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.documentType.createVariantElementTypeWithVariantAndInvariantProperty(
    mixedElementTypeName,
    groupName,
    variantPropertyName,
    invariantPropertyName,
    textStringDataTypeId
  );
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  // Create element with English culture
  await umbracoUi.library.clickActionsMenuAtRoot();
  await umbracoUi.library.clickCreateActionMenuOption();
  await umbracoUi.library.clickElementButton();
  await umbracoUi.library.clickModalMenuItemWithName(mixedElementTypeName);
  await umbracoUi.library.clickChooseModalButton();
  await umbracoUi.library.enterElementName(elementNameEnglish);
  await umbracoUi.library.enterPropertyValue(variantPropertyName, englishText);
  await umbracoUi.library.enterPropertyValue(invariantPropertyName, invariantText);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeCreated();
  await umbracoUi.waitForTimeout(ConstantHelper.wait.short);
  // Add Danish variant
  await umbracoUi.library.clickSelectVariantButton();
  await umbracoUi.library.clickVariantAddModeButtonForLanguageName(danishLanguage);
  await umbracoUi.library.enterElementName(elementNameDanish);
  await umbracoUi.library.enterPropertyValue(variantPropertyName, danishText);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();

  // Assert
  const elementData = await umbracoApi.element.getByName(elementNameEnglish);
  expect(elementData.variants.length).toBeGreaterThanOrEqual(2);
  // Verify variant properties have different values per culture
  const englishVariantValue = elementData.values.find((variant: any) => variant.culture === 'en-US' && variant.alias === variantPropertyName.toLowerCase());
  const danishVariantValue = elementData.values.find((variant: any) => variant.culture === 'da' && variant.alias === variantPropertyName.toLowerCase());
  expect(englishVariantValue?.value).toBe(englishText);
  expect(danishVariantValue?.value).toBe(danishText);
  // Verify invariant property is shared (culture is null)
  const invariantValue = elementData.values.find((variant: any) => variant.culture === null && variant.alias === invariantPropertyName.toLowerCase());
  expect(invariantValue?.value).toBe(invariantText);
});

test('can publish single culture variant only', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  variantElementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(
    variantElementTypeName,
    groupName,
    variantPropertyName,
    textStringDataTypeId,
    true,
    true
  );
  await umbracoApi.element.createDefaultElementWithEnglishAndDanishVariants(
    variantElementTypeId, 
    elementNameEnglish, 
    elementNameDanish, 
    variantPropertyName, 
    englishText, 
    danishText
  );
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementNameEnglish);
  await umbracoUi.library.switchLanguage(danishLanguage);
  await umbracoUi.library.clickSaveAndPublishButton();
  await umbracoUi.library.clickConfirmToPublishButtonAndWaitForElementToBePublished();

  // Assert
  const elementData = await umbracoApi.element.getByName(elementNameEnglish);
  const englishVariant = elementData.variants.find((variant: any) => variant.culture === 'en-US');
  expect(englishVariant.state).toBe('Draft');
  const danishVariant = elementData.variants.find((variant: any) => variant.culture === 'da');
  expect(danishVariant.state).toBe('Published');
});

test('can publish all cultures at once', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  variantElementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(
    variantElementTypeName,
    groupName,
    variantPropertyName,
    textStringDataTypeId,
    true,
    true
  );
  await umbracoApi.element.createDefaultElementWithEnglishAndDanishVariants(
    variantElementTypeId, 
    elementNameEnglish, 
    elementNameDanish, 
    variantPropertyName, 
    englishText, 
    danishText
  );
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementNameEnglish);
  await umbracoUi.library.clickSaveAndPublishButton();
  await umbracoUi.library.clickSelectAllCheckbox();
  await umbracoUi.library.clickConfirmToPublishButtonAndWaitForElementToBePublished();

  // Assert
  const elementData = await umbracoApi.element.getByName(elementNameEnglish);
  for (const variant of elementData.variants) {
    if (variant.culture !== null) {
      expect(variant.state).toBe('Published');
    }
  }
});

test('can edit variant property for specific culture only', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  variantElementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(
    variantElementTypeName,
    groupName,
    variantPropertyName,
    textStringDataTypeId,
    true,
    true
  );
  await umbracoApi.element.createDefaultElementWithEnglishAndDanishVariants(
    variantElementTypeId, 
    elementNameEnglish, 
    elementNameDanish, 
    variantPropertyName, 
    englishText, 
    danishText
  );
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementNameEnglish);
  await umbracoUi.library.enterTextstring(updatedEnglishText);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();

  // Assert
  const elementData = await umbracoApi.element.getByName(elementNameEnglish);
  const englishValue = elementData.values.find((variant: any) => variant.culture === 'en-US');
  expect(englishValue.value).toBe(updatedEnglishText);
  const danishValue = elementData.values.find((variant: any) => variant.culture === 'da');
  expect(danishValue.value).toBe(danishText);
});

test('can edit element name for specific culture only', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const updatedEnglishName = 'Updated English Name';
  variantElementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(
    variantElementTypeName,
    groupName,
    variantPropertyName,
    textStringDataTypeId,
    true,
    true
  );
  await umbracoApi.element.createDefaultElementWithEnglishAndDanishVariants(
    variantElementTypeId, 
    elementNameEnglish, 
    elementNameDanish, 
    variantPropertyName, 
    englishText, 
    danishText
  );
  await umbracoUi.goToBackOffice();
  await umbracoUi.library.goToSection(ConstantHelper.sections.library);

  // Act
  await umbracoUi.library.goToElementWithName(elementNameEnglish);
  await umbracoUi.library.enterElementName(updatedEnglishName);
  await umbracoUi.library.clickSaveButtonAndWaitForElementToBeUpdated();

  // Assert
  const elementData = await umbracoApi.element.getByName(updatedEnglishName);
  const englishVariant = elementData.variants.find((variant: any) => variant.culture === 'en-US');
  expect(englishVariant.name).toBe(updatedEnglishName);
  const danishVariant = elementData.variants.find((variant: any) => variant.culture === 'da');
  expect(danishVariant.name).toBe(elementNameDanish);

  // Clean
  await umbracoApi.element.ensureNameNotExists(updatedEnglishName);
});
