import {expect} from '@playwright/test';
import {ApiHelpers, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

// Raised from the 60s default - chained index polls would otherwise hit the test timeout first.
test.describe.configure({timeout: 120000});

const danishIsoCode = 'da';
const englishIsoCode = 'en-US';
const documentTypeName = 'DeliveryApiSearchCultureVarianceDocumentType';
const englishName = 'DeliveryApiSearchCultureVariance English';
const danishName = 'DeliveryApiSearchCultureVariance Danish';

let documentTypeId = '';

async function queryNamesForCulture(umbracoApi: ApiHelpers, culture: string, filter: string) {
  const response = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery({'Accept-Language': culture}, undefined, filter);
  const json = await response.json();
  return json.items.map((item: {name: string}) => item.name);
}

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.language.createDanishLanguage();
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithTextstringAndAllowAsRootAndAllowSelfAsChild(documentTypeName, true) ?? '';
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(englishName);
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('indexes each culture variant separately and returns only the requested culture\'s name', async ({umbracoApi}) => {
  // Arrange
  const documentIds = await umbracoApi.document.createPublishedVariantChain(documentTypeId, [{[englishIsoCode]: englishName, [danishIsoCode]: danishName}]);
  const documentId = documentIds[0];
  const filter = 'contentType:' + (await umbracoApi.documentType.getByName(documentTypeName)).alias;

  // Act
  await expect
    .poll(async () => queryNamesForCulture(umbracoApi, englishIsoCode, filter), {timeout: ConstantHelper.timeout.veryLong})
    .toContain(englishName);
  await expect
    .poll(async () => queryNamesForCulture(umbracoApi, danishIsoCode, filter), {timeout: ConstantHelper.timeout.veryLong})
    .toContain(danishName);

  // Assert - once both variants are confirmed indexed, neither culture's query may return the other's name
  const englishScopedNames = await queryNamesForCulture(umbracoApi, englishIsoCode, filter);
  expect(englishScopedNames).not.toContain(danishName);

  const danishScopedNames = await queryNamesForCulture(umbracoApi, danishIsoCode, filter);
  expect(danishScopedNames).not.toContain(englishName);

  const englishItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(documentId, {'Accept-Language': englishIsoCode});
  const englishItemJson = await englishItem.json();
  expect(englishItemJson.name).toBe(englishName);
});
