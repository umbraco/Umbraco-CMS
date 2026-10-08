import {ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// Languages
const danishIsoCode = 'da';
const englishIsoCode = 'en-US';
// Document type
const documentTypeName = 'Broken Publish Path Document Type';
// Content
const rootNames = {'en-US': 'Root en-US', 'da': 'Root da'};
const childNames = {'en-US': 'Child en-US', 'da': 'Child da'};
const grandchildNames = {'en-US': 'Grandchild en-US', 'da': 'Grandchild da'};
// Content URLs
const grandchildUrl = '/child-en-us/grandchild-en-us/';
const collapsedGrandchildUrl = '/grandchild-en-us/';
let documentTypeId = '';

async function openGrandchildInfoTab(umbracoUi) {
  await umbracoUi.goToBackOffice();
  await umbracoUi.content.goToSection(ConstantHelper.sections.content);
  await umbracoUi.content.openContentCaretButtonForName(rootNames[englishIsoCode]);
  await umbracoUi.content.openContentCaretButtonForName(childNames[englishIsoCode]);
  await umbracoUi.content.goToContentWithName(grandchildNames[englishIsoCode]);
  await umbracoUi.content.clickInfoTab();
}

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.language.createDanishLanguage();
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithTextstringAndAllowAsRootAndAllowSelfAsChild(documentTypeName, true);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(rootNames[englishIsoCode]);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('can see the multi-level url for a grandchild when the ancestor chain is fully published', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.createPublishedVariantChain(documentTypeId, [rootNames, childNames, grandchildNames]);

  // Act
  await openGrandchildInfoTab(umbracoUi);

  // Assert
  await umbracoUi.content.doesDocumentHaveLink(grandchildUrl);
});

test('cannot see a routable url for a grandchild when an ancestor is unpublished', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  const documentIds = await umbracoApi.document.createPublishedVariantChain(documentTypeId, [rootNames, childNames, grandchildNames]);
  const childId = documentIds[1];
  await umbracoApi.document.unpublishDocumentWithCulture(childId, englishIsoCode);

  // Act
  await openGrandchildInfoTab(umbracoUi);

  // Assert
  await umbracoUi.content.doesDocumentHaveLink(ConstantHelper.documentUrlInfoMessages.cannotBeRouted);
  await umbracoUi.content.doesDocumentNotHaveLink(collapsedGrandchildUrl);
});
