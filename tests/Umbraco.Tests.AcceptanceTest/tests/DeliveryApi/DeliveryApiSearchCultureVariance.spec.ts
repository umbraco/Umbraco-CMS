import {test} from '@umbraco/acceptance-test-helpers';

const danishIsoCode = 'da';
const englishIsoCode = 'en-US';
const documentTypeName = 'DeliveryApiSearchCultureVarianceDocumentType';
const englishName = 'DeliveryApiSearchCultureVariance English';
const danishName = 'DeliveryApiSearchCultureVariance Danish';
const englishOnlyDocumentTypeName = 'DeliveryApiSearchCultureVarianceEnglishOnlyDocumentType';
const englishOnlyDocumentName = 'DeliveryApiSearchCultureVariance EnglishOnly';
const textstringDataTypeName = 'Textstring';

let documentTypeId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(englishOnlyDocumentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);

  await umbracoApi.language.createDanishLanguage();
  documentTypeId = await umbracoApi.documentType.createDocumentTypeWithTextstringAndAllowAsRootAndAllowSelfAsChild(documentTypeName, true) ?? '';
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(englishOnlyDocumentTypeName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('can query each culture variant separately', async ({umbracoApi}) => {
  test.slow();

  // Arrange
  await umbracoApi.document.createPublishedVariantChain(documentTypeId, [{[englishIsoCode]: englishName, [danishIsoCode]: danishName}]);
  const filter = 'contentType:' + (await umbracoApi.documentType.getByName(documentTypeName)).alias;
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames(filter, undefined, [englishName], undefined, {'Accept-Language': englishIsoCode});
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames(filter, undefined, [danishName], undefined, {'Accept-Language': danishIsoCode});

  // Assert
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryTotalIs(filter, 1, {'Accept-Language': englishIsoCode});
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryTotalIs(filter, 1, {'Accept-Language': danishIsoCode});
});

test('cannot find a culture variant that is not published', async ({umbracoApi}) => {
  test.slow();

  // Arrange
  const textstringDataType = await umbracoApi.dataType.getByName(textstringDataTypeName);
  const englishOnlyDocumentTypeId = await umbracoApi.documentType.createDocumentTypeWithPropertyEditor(englishOnlyDocumentTypeName, textstringDataTypeName, textstringDataType.id, 'CultureGroup', true, true) ?? '';
  const documentId = await umbracoApi.document.createDocumentWithTwoCultureSpecificValues(englishOnlyDocumentName, englishOnlyDocumentTypeId, textstringDataTypeName, englishIsoCode, 'English value', danishIsoCode, 'Danish value') ?? '';
  await umbracoApi.document.publishDocumentWithCulture(documentId, englishIsoCode);
  const filter = 'contentType:' + (await umbracoApi.documentType.getByName(englishOnlyDocumentTypeName)).alias;
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryReturnsNames(filter, undefined, [englishOnlyDocumentName], 1, {'Accept-Language': englishIsoCode});

  // Assert
  await umbracoApi.contentDeliveryApi.waitUntilContentQueryTotalIs(filter, 0, {'Accept-Language': danishIsoCode});
});
