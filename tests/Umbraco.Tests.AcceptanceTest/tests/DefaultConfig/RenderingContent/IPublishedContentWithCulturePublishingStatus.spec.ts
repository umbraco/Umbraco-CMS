import {AliasHelper, test} from '@umbraco/acceptance-test-helpers';
import {expect} from '@playwright/test';

// Languages
const danishLanguageName = 'Danish';
const englishIsoCode = 'en-US';
const danishIsoCode = 'da';

// Document Types (root -> child -> grandchild, all variant)
const rootDocumentTypeName = 'RootDocumentType';
const childDocumentTypeName = 'ChildDocumentType';
const grandchildDocumentTypeName = 'GrandchildDocumentType';

// Content (one name + value per culture)
const rootContentEnglishName = 'RootEnglish';
const rootContentDanishName = 'RootDanish';
const childContentEnglishName = 'ChildEnglish';
const childContentDanishName = 'ChildDanish';
const grandchildContentEnglishName = 'GrandchildEnglish';
const grandchildContentDanishName = 'GrandchildDanish';
const grandchildEnglishValue = 'Grandchild English Value';
const grandchildDanishValue = 'Grandchild Danish Value';

// Data Type & Template
const dataTypeName = 'Textstring';
const renderTemplateName = 'RenderCultureValueTemplate';

let dataTypeId = '';
let renderTemplateId = '';
let rootContentId = '';
let childContentId = '';
let grandchildContentId = '';

async function setupVariantBranch(umbracoApi): Promise<void> {
  renderTemplateId = await umbracoApi.template.createTemplateWithDisplayingStringValue(renderTemplateName, AliasHelper.toAlias(dataTypeName));

  const grandchildDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithPropertyEditorTemplateAndAllowedChildNode(grandchildDocumentTypeName, dataTypeName, dataTypeId, renderTemplateId, false);
  const childDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithPropertyEditorTemplateAndAllowedChildNode(childDocumentTypeName, dataTypeName, dataTypeId, renderTemplateId, false, grandchildDocumentTypeId);
  const rootDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithPropertyEditorTemplateAndAllowedChildNode(rootDocumentTypeName, dataTypeName, dataTypeId, renderTemplateId, true, childDocumentTypeId);

  rootContentId = await umbracoApi.document.createVariantDocumentWithTextContentAndParent(rootDocumentTypeId, renderTemplateId, dataTypeName, [
    {isoCode: englishIsoCode, name: rootContentEnglishName, value: 'Root English Value'},
    {isoCode: danishIsoCode, name: rootContentDanishName, value: 'Root Danish Value'}
  ]);
  childContentId = await umbracoApi.document.createVariantDocumentWithTextContentAndParent(childDocumentTypeId, renderTemplateId, dataTypeName, [
    {isoCode: englishIsoCode, name: childContentEnglishName, value: 'Child English Value'},
    {isoCode: danishIsoCode, name: childContentDanishName, value: 'Child Danish Value'}
  ], rootContentId);
  grandchildContentId = await umbracoApi.document.createVariantDocumentWithTextContentAndParent(grandchildDocumentTypeId, renderTemplateId, dataTypeName, [
    {isoCode: englishIsoCode, name: grandchildContentEnglishName, value: grandchildEnglishValue},
    {isoCode: danishIsoCode, name: grandchildContentDanishName, value: grandchildDanishValue}
  ], childContentId);

  // The whole branch must be published in both cultures first, so a single culture can later be unpublished on one node in isolation
  await umbracoApi.document.publishWithCultures(rootContentId, [englishIsoCode, danishIsoCode]);
  await umbracoApi.document.publishWithCultures(childContentId, [englishIsoCode, danishIsoCode]);
  await umbracoApi.document.publishWithCultures(grandchildContentId, [englishIsoCode, danishIsoCode]);

  // Route each culture through a left-hand path domain so the front-end can resolve the requested culture
  await umbracoApi.document.updateDomainsWithCultures(rootContentId, [
    {domainName: '/en', isoCode: englishIsoCode},
    {domainName: '/da', isoCode: danishIsoCode}
  ]);
}

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(grandchildDocumentTypeName);
  await umbracoApi.template.ensureNameNotExists(renderTemplateName);
  await umbracoApi.document.ensureNameNotExists(rootContentEnglishName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  await umbracoApi.language.createDanishLanguage();
  dataTypeId = (await umbracoApi.dataType.getByName(dataTypeName)).id;
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(rootContentEnglishName);
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(grandchildDocumentTypeName);
  await umbracoApi.template.ensureNameNotExists(renderTemplateName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('can render a descendant in a culture where its ancestor chain is published', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await setupVariantBranch(umbracoApi);
  // The Danish culture is unpublished on the child only - the English chain stays fully published
  await umbracoApi.document.unpublish(childContentId, [danishIsoCode]);
  const grandchildEnglishUrl = await umbracoApi.document.getDocumentUrlByCulture(grandchildContentId, englishIsoCode);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(grandchildEnglishUrl);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(grandchildEnglishValue);
});

test('cannot render a descendant in a culture where its ancestor is unpublished', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await setupVariantBranch(umbracoApi);
  // The child is unpublished in Danish, so its Danish descendants have no published ancestor path
  await umbracoApi.document.unpublish(childContentId, [danishIsoCode]);

  // Act
  const grandchildDanishUrl = await umbracoApi.document.getDocumentUrlByCulture(grandchildContentId, danishIsoCode);

  // Assert
  // The grandchild is published in Danish, but is no longer routable because the ancestor path is not published in that culture
  expect(grandchildDanishUrl).toBeFalsy();
});

test('Parent and Ancestors return ancestors that are unpublished in the requested culture', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const contentPickerDataTypeName = 'Content Picker';
  const pickerDocumentTypeName = 'PickerDocumentType';
  const pickerContentName = 'PickerContent';
  const pickerTemplateName = 'PickerAncestorsTemplate';
  const pickerPropertyName = 'Content Picker';

  await umbracoApi.documentType.ensureNameNotExists(pickerDocumentTypeName);
  await umbracoApi.document.ensureNameNotExists(pickerContentName);
  await umbracoApi.template.ensureNameNotExists(pickerTemplateName);

  await setupVariantBranch(umbracoApi);
  // The English culture is unpublished on the child only - the grandchild itself remains published in English
  await umbracoApi.document.unpublish(childContentId, [englishIsoCode]);

  const contentPickerDataTypeId = (await umbracoApi.dataType.getByName(contentPickerDataTypeName)).id;
  const pickerTemplateId = await umbracoApi.template.createTemplateWithDisplayingContentPickerAncestors(pickerTemplateName, AliasHelper.toAlias(pickerPropertyName));
  const pickerDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditorAndAllowedTemplate(pickerDocumentTypeName, contentPickerDataTypeId, pickerPropertyName, pickerTemplateId);
  const pickerContentId = await umbracoApi.document.createDocumentWithContentPicker(pickerContentName, pickerDocumentTypeId, grandchildContentId);
  await umbracoApi.document.publish(pickerContentId);
  const pickerUrl = await umbracoApi.document.getDocumentUrl(pickerContentId);

  // Act
  await umbracoUi.contentRender.navigateToRenderedContentPage(pickerUrl);

  // Assert
  // The picked grandchild still resolves its unpublished-in-English parent (child) and full ancestor chain (child + root)
  await umbracoUi.contentRender.doesContentRenderValueContainText('Parent:');
  await umbracoUi.contentRender.doesContentRenderValueContainText('Ancestors:2');

  // Clean
  await umbracoApi.document.ensureNameNotExists(pickerContentName);
  await umbracoApi.documentType.ensureNameNotExists(pickerDocumentTypeName);
  await umbracoApi.template.ensureNameNotExists(pickerTemplateName);
});
