import {expect} from '@playwright/test';
import {test} from '@umbraco/acceptance-test-helpers';

// Languages
const danishIsoCode = 'da';
const englishIsoCode = 'en-US';
// Document types
const documentTypeName = 'Broken Publish Path Document Type';
const invariantDocumentTypeName = 'Broken Publish Path Invariant Document Type';
// Variant contents
const rootNames = {'en-US': 'Root en-US', 'da': 'Root da'};
const childNames = {'en-US': 'Child en-US', 'da': 'Child da'};
const grandchildNames = {'en-US': 'Grandchild en-US', 'da': 'Grandchild da'};
const greatGrandchildNames = {'en-US': 'Great Grandchild en-US', 'da': 'Great Grandchild da'};
// Invariant contents
const invariantNodeNames = ['Invariant Root', 'Invariant Child', 'Invariant Grandchild'];
let documentTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.language.createDanishLanguage();
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithTextstringAndAllowAsRootAndAllowSelfAsChild(documentTypeName, true);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(rootNames[englishIsoCode]);
  await umbracoApi.document.ensureNameNotExists(invariantNodeNames[0]);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(invariantDocumentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('can fetch a grandchild when the full ancestor chain is published in the requested culture', async ({umbracoApi}) => {
  // Arrange
  const documentIds = await umbracoApi.document.createPublishedVariantChain(documentTypeId, [rootNames, childNames, grandchildNames]);
  const grandchildId = documentIds[2];

  // Act
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(grandchildId, {'Accept-Language': englishIsoCode});

  // Assert
  expect(contentItem.status()).toBe(200);
  const contentItemJson = await contentItem.json();
  expect(contentItemJson.route.path).toBe('/child-en-us/grandchild-en-us/');
});

test('can fetch a grandchild in a second culture when the full ancestor chain is published', async ({umbracoApi}) => {
  // Arrange
  const documentIds = await umbracoApi.document.createPublishedVariantChain(documentTypeId, [rootNames, childNames, grandchildNames]);
  const grandchildId = documentIds[2];

  // Act
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(grandchildId, {'Accept-Language': danishIsoCode});

  // Assert
  expect(contentItem.status()).toBe(200);
  const contentItemJson = await contentItem.json();
  expect(contentItemJson.route.path).toBe('/child-da/grandchild-da/');
});

test('cannot fetch a grandchild in a culture where an ancestor is unpublished', async ({umbracoApi}) => {
  // Arrange
  const documentIds = await umbracoApi.document.createPublishedVariantChain(documentTypeId, [rootNames, childNames, grandchildNames]);
  const childId = documentIds[1];
  const grandchildId = documentIds[2];
  await umbracoApi.document.unpublishDocumentWithCulture(childId, danishIsoCode);

  // Act
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(grandchildId, {'Accept-Language': danishIsoCode});

  // Assert
  expect(contentItem.status()).toBe(404);
});

test('can fetch a grandchild in the culture where its ancestors are published', async ({umbracoApi}) => {
  // Arrange
  const documentIds = await umbracoApi.document.createPublishedVariantChain(documentTypeId, [rootNames, childNames, grandchildNames]);
  const childId = documentIds[1];
  const grandchildId = documentIds[2];
  await umbracoApi.document.unpublishDocumentWithCulture(childId, danishIsoCode);

  // Act
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(grandchildId, {'Accept-Language': englishIsoCode});

  // Assert
  expect(contentItem.status()).toBe(200);
  const contentItemJson = await contentItem.json();
  expect(contentItemJson.route.path).toBe('/child-en-us/grandchild-en-us/');
});

test('cannot fetch a child in a culture where the root is unpublished', async ({umbracoApi}) => {
  // Arrange
  const documentIds = await umbracoApi.document.createPublishedVariantChain(documentTypeId, [rootNames, childNames]);
  const rootId = documentIds[0];
  const childId = documentIds[1];
  await umbracoApi.document.unpublishDocumentWithCulture(rootId, danishIsoCode);

  // Act
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(childId, {'Accept-Language': danishIsoCode});

  // Assert
  expect(contentItem.status()).toBe(404);
});

test('cannot fetch a deeply nested content node when a mid-level ancestor is unpublished in the requested culture', async ({umbracoApi}) => {
  // Arrange
  const documentIds = await umbracoApi.document.createPublishedVariantChain(documentTypeId, [rootNames, childNames, grandchildNames, greatGrandchildNames]);
  const grandchildId = documentIds[2];
  const greatGrandchildId = documentIds[3];
  await umbracoApi.document.unpublishDocumentWithCulture(grandchildId, danishIsoCode);

  // Act
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(greatGrandchildId, {'Accept-Language': danishIsoCode});

  // Assert
  expect(contentItem.status()).toBe(404);
});

test('can fetch a deeply nested content node when the whole chain is published', async ({umbracoApi}) => {
  // Arrange
  const documentIds = await umbracoApi.document.createPublishedVariantChain(documentTypeId, [rootNames, childNames, grandchildNames, greatGrandchildNames]);
  const greatGrandchildId = documentIds[3];

  // Act
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(greatGrandchildId, {'Accept-Language': englishIsoCode});

  // Assert
  expect(contentItem.status()).toBe(200);
  const contentItemJson = await contentItem.json();
  expect(contentItemJson.route.path).toBe('/child-en-us/grandchild-en-us/great-grandchild-en-us/');
});

test('cannot fetch a grandchild when an invariant ancestor is unpublished', async ({umbracoApi}) => {
  // Arrange
  const invariantDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithTextstringAndAllowAsRootAndAllowSelfAsChild(invariantDocumentTypeName, false);
  const documentIds = await umbracoApi.document.createPublishedInvariantChain(invariantDocumentTypeId, invariantNodeNames);
  const childId = documentIds[1];
  const grandchildId = documentIds[2];
  await umbracoApi.document.unpublish(childId);

  // Act
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(grandchildId);

  // Assert
  expect(contentItem.status()).toBe(404);
});

test('can fetch a grandchild when the invariant chain is fully published', async ({umbracoApi}) => {
  // Arrange
  const invariantDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithTextstringAndAllowAsRootAndAllowSelfAsChild(invariantDocumentTypeName, false);
  const documentIds = await umbracoApi.document.createPublishedInvariantChain(invariantDocumentTypeId, invariantNodeNames);
  const grandchildId = documentIds[2];

  // Act
  const contentItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(grandchildId);

  // Assert
  expect(contentItem.status()).toBe(200);
  const contentItemJson = await contentItem.json();
  expect(contentItemJson.route.path).toBe('/invariant-child/invariant-grandchild/');
});
