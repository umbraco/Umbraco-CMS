import {expect} from '@playwright/test';
import {ApiHelpers, ConstantHelper, test} from '@umbraco/acceptance-test-helpers';

const danishIsoCode = 'da';
const englishIsoCode = 'en-US';
const documentTypeName = 'DeliveryApiSearchCultureVarianceDocumentType';
const englishName = 'DeliveryApiSearchCultureVariance English';
const danishName = 'DeliveryApiSearchCultureVariance Danish';

let documentTypeId = '';

async function queryForCulture(umbracoApi: ApiHelpers, culture: string, filter: string) {
  const response = await umbracoApi.contentDeliveryApi.getContentItemsFromAQuery({'Accept-Language': culture}, undefined, filter);
  return await response.json();
}

async function queryNamesForCulture(umbracoApi: ApiHelpers, culture: string, filter: string) {
  return (await queryForCulture(umbracoApi, culture, filter)).items.map((item: {name: string}) => item.name);
}

test.beforeEach(async ({umbracoApi}) => {
  test.slow();
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
    .poll(async () => queryNamesForCulture(umbracoApi, englishIsoCode, filter), {timeout: ConstantHelper.timeout.pageLoad})
    .toContain(englishName);
  await expect
    .poll(async () => queryNamesForCulture(umbracoApi, danishIsoCode, filter), {timeout: ConstantHelper.timeout.pageLoad})
    .toContain(danishName);

  // Assert
  await expect
    .poll(async () => (await queryForCulture(umbracoApi, englishIsoCode, filter)).total, {timeout: ConstantHelper.timeout.pageLoad})
    .toBe(1);
  expect(await queryNamesForCulture(umbracoApi, englishIsoCode, filter)).toEqual([englishName]);

  await expect
    .poll(async () => (await queryForCulture(umbracoApi, danishIsoCode, filter)).total, {timeout: ConstantHelper.timeout.pageLoad})
    .toBe(1);
  expect(await queryNamesForCulture(umbracoApi, danishIsoCode, filter)).toEqual([danishName]);

  const englishItem = await umbracoApi.contentDeliveryApi.getContentItemWithId(documentId, {'Accept-Language': englishIsoCode});
  const englishItemJson = await englishItem.json();
  expect(englishItemJson.name).toBe(englishName);
});
