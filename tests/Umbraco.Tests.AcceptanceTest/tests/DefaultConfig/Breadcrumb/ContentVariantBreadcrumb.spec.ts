import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const danishIsoCode = 'da';
const englishIsoCode = 'en-US';
const danishLanguageName = 'Danish';
const documentTypeName = 'BreadcrumbVariantDocumentType';
const rootNames = {'en-US': 'BreadcrumbRoot en-US', 'da': 'BreadcrumbRoot da'};
const childNames = {'en-US': 'BreadcrumbChild en-US', 'da': 'BreadcrumbChild da'};
const grandchildNames = {'en-US': 'BreadcrumbGrandchild en-US', 'da': 'BreadcrumbGrandchild da'};
const contentSectionName = 'Content';
let documentTypeId = '';

test.beforeEach(async ({umbracoApi, umbracoUi}) => {
  await umbracoApi.document.ensureNameNotExists(rootNames[englishIsoCode]);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  await umbracoApi.language.createDanishLanguage();
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithTextstringAndAllowAsRootAndAllowSelfAsChild(documentTypeName, true);
  await umbracoUi.goToBackOffice();
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(rootNames[englishIsoCode]);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

async function openGrandchildInDanish(umbracoUi) {
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.openContentCaretButtonForName(rootNames[englishIsoCode]);
  await umbracoUi.content.openContentCaretButtonForName(childNames[englishIsoCode]);
  await umbracoUi.content.goToContentWithName(grandchildNames[englishIsoCode]);
  await umbracoUi.content.switchLanguage(danishLanguageName);
}

test('can see the ancestor names of the active culture in the breadcrumb', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createPublishedVariantChain(documentTypeId, [rootNames, childNames, grandchildNames]);

  // Act
  await openGrandchildInDanish(umbracoUi);

  // Assert
  await umbracoUi.content.doesWorkspaceBreadcrumbHaveItems([contentSectionName, rootNames[danishIsoCode], childNames[danishIsoCode], grandchildNames[danishIsoCode]]);
});

test('can see the fallback name in parentheses for an ancestor without the active culture in the breadcrumb', {tag: '@release'}, async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const englishOnlyChildNames = {'en-US': childNames[englishIsoCode]};
  await umbracoApi.document.createPublishedVariantChain(documentTypeId, [rootNames, englishOnlyChildNames, grandchildNames]);

  // Act
  await openGrandchildInDanish(umbracoUi);

  // Assert
  await umbracoUi.content.doesWorkspaceBreadcrumbHaveItems([contentSectionName, rootNames[danishIsoCode], `(${childNames[englishIsoCode]})`, grandchildNames[danishIsoCode]]);
});
