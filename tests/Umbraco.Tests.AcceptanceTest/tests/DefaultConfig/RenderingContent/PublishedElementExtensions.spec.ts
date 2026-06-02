import {AliasHelper, test} from '@umbraco/acceptance-test-helpers';

// Data Type
const elementPickerDataTypeName = 'TestElementPicker';
// Document Type
const documentTypeName = 'TestDocumentTypeForContent';
const propertyName = 'Element';
// Element Type
const elementTypeName = 'TestElementType';
// Template
const templateName = 'TestTemplateForContent';
// Content
const contentName = 'TestRenderingElementExtensions';
const danishContentName = 'TestRenderingElementExtensionsDanish';
// Element
const elementName = 'ElementInEnglish';
const danishElementName = 'ElementInDanish';
const secondElementName = 'SecondTestElement';
const composedElementName = 'ComposedElement';
// Composition Element Types
const baseElementTypeName = 'BaseElementType';
const childElementTypeName = 'ChildElementType';
// Aliases 
const elementTypeAlias = AliasHelper.toAlias(elementTypeName);
const pageDocumentTypeAlias = AliasHelper.toAlias(documentTypeName);
const elementPickerPropertyAlias = AliasHelper.toAlias(propertyName);
const baseElementTypeAlias = AliasHelper.toAlias(baseElementTypeName);
const childElementTypeAlias = AliasHelper.toAlias(childElementTypeName);

const currentYear = new Date().getFullYear();

let elementPickerDataTypeId = '';
let textStringDataTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  elementPickerDataTypeId = await umbracoApi.dataType.createDefaultElementPickerDataType(elementPickerDataTypeName);
  const textStringData = await umbracoApi.dataType.getByName('Textstring');
  textStringDataTypeId = textStringData.id;
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(contentName);
  await umbracoApi.document.ensureNameNotExists(danishContentName);
  await umbracoApi.element.ensureNameNotExists(elementName);
  await umbracoApi.element.ensureNameNotExists(danishElementName);
  await umbracoApi.element.ensureNameNotExists(secondElementName);
  await umbracoApi.element.ensureNameNotExists(composedElementName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(elementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childElementTypeName);
  await umbracoApi.documentType.ensureNameNotExists(baseElementTypeName);
  await umbracoApi.template.ensureNameNotExists(templateName);
  await umbracoApi.dataType.ensureNameNotExists(elementPickerDataTypeName);
  await umbracoApi.language.ensureNameNotExists('Danish');
});

test('can render published element extension methods on an invariant element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const currentUser = await umbracoApi.user.getCurrentUser();
  const templateId = await umbracoApi.template.createTemplateWithDisplayingElementPickerExtensionMethods(templateName, propertyName, elementTypeAlias, pageDocumentTypeAlias);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, elementPickerDataTypeId, propertyName, templateId);
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.publish(elementId);
  const contentId = await umbracoApi.document.createDocumentWithElementPickers(contentName, documentTypeId, propertyName, [elementId]);
  await umbracoApi.document.publish(contentId);
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentUrl);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText('Name: ' + elementName);
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasCultureEn: False');
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasCultureDa: False');
  await umbracoUi.contentRender.doesContentRenderValueContainText('IsDocumentType: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('NotDocumentType: False');
  await umbracoUi.contentRender.doesContentRenderValueContainText('CultureDate: ' + currentYear);
  await umbracoUi.contentRender.doesContentRenderValueContainText('CreatorName: ' + currentUser.name);
  await umbracoUi.contentRender.doesContentRenderValueContainText('WriterName: ' + currentUser.name);
  await umbracoUi.contentRender.doesContentRenderValueContainText('PageName: ' + contentName);
  await umbracoUi.contentRender.doesContentRenderValueContainText('PageIsDocumentType: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('PageCreatorName: ' + currentUser.name);
});

test('can render culture-specific element extension methods per request culture', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.createDanishLanguage();
  const templateId = await umbracoApi.template.createTemplateWithDisplayingElementPickerExtensionMethods(templateName, propertyName, elementTypeAlias, pageDocumentTypeAlias);
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditorAndAllowedTemplate(documentTypeName, propertyName, elementPickerDataTypeId, templateId);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(elementTypeName, 'TestGroup', 'Textstring', textStringDataTypeId, true, true);
  const elementId = await umbracoApi.element.createDefaultElementWithEnglishAndDanishVariants(elementTypeId, elementName, danishElementName, 'Textstring', 'English text', 'Dansk tekst');
  await umbracoApi.element.publish(elementId, {publishSchedules: [{culture: 'en-US'}, {culture: 'da'}]});
  const contentId = await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(contentName, documentTypeId, elementPickerPropertyAlias, 'Umbraco.ElementPicker', [{isoCode: 'en-US', name: contentName}, {isoCode: 'da', name: danishContentName}], [elementId]);
  await umbracoApi.document.publish(contentId, {publishSchedules: [{culture: 'en-US'}, {culture: 'da'}]});
  await umbracoApi.document.updateDomainsForVariantDocument(contentId, [{domainName: '/en', isoCode: 'en-US'}, {domainName: '/da', isoCode: 'da'}]);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage('/en');

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText('Name: ' + elementName);
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasCultureEn: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasCultureDa: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('CultureDate: ' + currentYear);
  await umbracoUi.contentRender.navigateToRenderedContentPage('/da');
  await umbracoUi.contentRender.doesContentRenderValueContainText('Name: ' + danishElementName);
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasCultureEn: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasCultureDa: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('CultureDate: ' + currentYear);
});

test('can render culture-specific element methods using explicit culture arguments', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.createDanishLanguage();
  const templateId = await umbracoApi.template.createTemplateWithDisplayingElementPickerVarianceAndIdentityMethods(templateName, propertyName, elementTypeAlias, pageDocumentTypeAlias);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, elementPickerDataTypeId, propertyName, templateId);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(elementTypeName, 'TestGroup', 'Textstring', textStringDataTypeId, true, true);
  const elementId = await umbracoApi.element.createDefaultElementWithEnglishAndDanishVariants(elementTypeId, elementName, danishElementName, 'Textstring', 'English text', 'Dansk tekst');
  await umbracoApi.element.publish(elementId, {publishSchedules: [{culture: 'en-US'}, {culture: 'da'}]});
  const contentId = await umbracoApi.document.createDocumentWithElementPickers(contentName, documentTypeId, propertyName, [elementId]);
  await umbracoApi.document.publish(contentId);
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentUrl);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText('Name-en: [' + elementName + ']');
  await umbracoUi.contentRender.doesContentRenderValueContainText('Name-da: [' + danishElementName + ']');
  await umbracoUi.contentRender.doesContentRenderValueContainText('Name-fr: []');
  await umbracoUi.contentRender.doesContentRenderValueContainText('InvariantOrEn: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('InvariantOrFr: False');
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasCultureUpper: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('IsDocTypeUpper: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('CultureDateDa: ' + currentYear);
  await umbracoUi.contentRender.doesContentRenderValueContainText('CultureDateFr: 1');
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasValueTextstringDa: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasValueMissing: False');
});

test('can render variance methods on an invariant element ignoring the culture argument', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const templateId = await umbracoApi.template.createTemplateWithDisplayingElementPickerVarianceAndIdentityMethods(templateName, propertyName, elementTypeAlias, pageDocumentTypeAlias);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, elementPickerDataTypeId, propertyName, templateId);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementType(elementTypeName, 'TestGroup', 'Textstring', textStringDataTypeId);
  const elementId = await umbracoApi.element.createElementWithTextContent(elementName, elementTypeId, 'Some text', 'Textstring');
  await umbracoApi.element.publish(elementId);
  const contentId = await umbracoApi.document.createDocumentWithElementPickers(contentName, documentTypeId, propertyName, [elementId]);
  await umbracoApi.document.publish(contentId);
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentUrl);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText('InvariantOrDa: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('CultureDateFr: ' + currentYear);
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasCultureUpper: False');
  await umbracoUi.contentRender.doesContentRenderValueContainText('Name-en: [' + elementName + ']');
});

test('can compare elements using IsEqual and IsNotEqual', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const templateId = await umbracoApi.template.createTemplateWithDisplayingElementPickerVarianceAndIdentityMethods(templateName, propertyName, elementTypeAlias, pageDocumentTypeAlias);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, elementPickerDataTypeId, propertyName, templateId);
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const firstElementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  const secondElementId = await umbracoApi.element.createDefaultElement(secondElementName, elementTypeId);
  await umbracoApi.element.publish(firstElementId);
  await umbracoApi.element.publish(secondElementId);
  const contentId = await umbracoApi.document.createDocumentWithElementPickers(contentName, documentTypeId, propertyName, [firstElementId, secondElementId]);
  await umbracoApi.document.publish(contentId);
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentUrl);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText('IsEqualFirst-' + elementName + ': True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('IsNotEqualFirst-' + elementName + ': False');
  await umbracoUi.contentRender.doesContentRenderValueContainText('IsEqualFirst-' + secondElementName + ': False');
  await umbracoUi.contentRender.doesContentRenderValueContainText('IsNotEqualFirst-' + secondElementName + ': True');
});

test('can render published content extensions on a variant element', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.createDanishLanguage();
  const templateId = await umbracoApi.template.createTemplateWithDisplayingElementPickerVarianceAndIdentityMethods(templateName, propertyName, elementTypeAlias, pageDocumentTypeAlias);
  const documentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithInvariantPropertyEditorAndAllowedTemplate(documentTypeName, propertyName, elementPickerDataTypeId, templateId);
  const elementTypeId = await umbracoApi.documentType.createEmptyElementType(elementTypeName);
  const elementId = await umbracoApi.element.createDefaultElement(elementName, elementTypeId);
  await umbracoApi.element.publish(elementId);
  const contentId = await umbracoApi.document.createDocumentWithMultipleVariantsWithSharedProperty(contentName, documentTypeId, elementPickerPropertyAlias, 'Umbraco.ElementPicker', [{isoCode: 'en-US', name: contentName}, {isoCode: 'da', name: danishContentName}], [elementId]);
  await umbracoApi.document.publish(contentId, {publishSchedules: [{culture: 'en-US'}, {culture: 'da'}]});
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentUrl);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText('PageHasCultureEn: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('PageInvariantOrFr: False');
  await umbracoUi.contentRender.doesContentRenderValueContainText('PageCultureDateEn: ' + currentYear);
  await umbracoUi.contentRender.doesContentRenderValueContainText('PageIsEqualSelf: True');
});

test('can render published element extensions on elements using composition', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const templateId = await umbracoApi.template.createTemplateWithDisplayingElementPickerCompositionMethods(templateName, propertyName, baseElementTypeAlias, childElementTypeAlias);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, elementPickerDataTypeId, propertyName, templateId);
  const baseElementTypeId = await umbracoApi.documentType.createEmptyElementType(baseElementTypeName);
  const childElementTypeId = await umbracoApi.documentType.createElementTypeWithAComposition(childElementTypeName, baseElementTypeId, true);
  const elementId = await umbracoApi.element.createDefaultElement(composedElementName, childElementTypeId);
  await umbracoApi.element.publish(elementId);
  const contentId = await umbracoApi.document.createDocumentWithElementPickers(contentName, documentTypeId, propertyName, [elementId]);
  await umbracoApi.document.publish(contentId);
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentUrl);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText('IsDocTypeBase: False');
  await umbracoUi.contentRender.doesContentRenderValueContainText('IsDocTypeBaseRecursive: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('IsComposedOfBase: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('IsDocTypeChild: True');
});

test('can render fallback language values for variant elements', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.language.createDanishLanguage();
  const englishValue = 'English value';
  const textStringPropertyAlias = AliasHelper.toAlias('Textstring');
  const templateId = await umbracoApi.template.createTemplateWithDisplayingElementPickerFallbackMethods(templateName, propertyName, textStringPropertyAlias);
  const documentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(documentTypeName, elementPickerDataTypeId, propertyName, templateId);
  const elementTypeId = await umbracoApi.documentType.createDefaultElementTypeWithVaryByCulture(elementTypeName, 'TestGroup', 'Textstring', textStringDataTypeId, true, true);
  // English has a value, Danish is empty, so Danish must fall back to English
  const elementId = await umbracoApi.element.createDefaultElementWithEnglishAndDanishVariants(elementTypeId, elementName, danishElementName, 'Textstring', englishValue, '');
  await umbracoApi.element.publish(elementId, {publishSchedules: [{culture: 'en-US'}, {culture: 'da'}]});
  const contentId = await umbracoApi.document.createDocumentWithElementPickers(contentName, documentTypeId, propertyName, [elementId]);
  await umbracoApi.document.publish(contentId);
  const contentUrl = await umbracoApi.document.getDocumentUrl(contentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(contentUrl);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasValueDaNoFallback: False');
  await umbracoUi.contentRender.doesContentRenderValueContainText('TextDaNoFallback: []');
  await umbracoUi.contentRender.doesContentRenderValueContainText('HasValueDaFallbackLanguage: True');
  await umbracoUi.contentRender.doesContentRenderValueContainText('TextDaFallbackLanguage: [' + englishValue + ']');
});
