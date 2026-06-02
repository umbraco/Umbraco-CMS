import {test} from '@umbraco/acceptance-test-helpers';

// Languages
const englishIsoCode = 'en-US';
const danishIsoCode = 'da';
const cultures = [englishIsoCode, danishIsoCode];

// Document Types
const rootDocumentTypeName = 'RootDocumentType';
const childDocumentTypeName = 'ChildDocumentType';
const grandchildDocumentTypeName = 'GrandchildDocumentType';

// Content
const rootContentName = 'RootContent';
const publishedChildName = 'PublishedChild';
const unpublishedChildName = 'UnpublishedChild';
const grandchildContentName = 'Grandchild';

// Template
const renderTemplateName = 'RenderDescendantsTemplate';

let renderTemplateId = '';
let rootContentId = '';
let unpublishedChildId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(rootContentName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  await umbracoApi.language.createDanishLanguage();
  renderTemplateId = await umbracoApi.template.createTemplateWithDisplayingValue(renderTemplateName, '\n@foreach (var content in Model.Descendants())\n{\n\t<li>@content.Name</li>\n}');
  const grandchildDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithTemplateAndAllowedChildNode(grandchildDocumentTypeName, renderTemplateId);
  const childDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithTemplateAndAllowedChildNode(childDocumentTypeName, renderTemplateId, false, grandchildDocumentTypeId);
  const rootDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithTemplateAndAllowedChildNode(rootDocumentTypeName, renderTemplateId, true, childDocumentTypeId);

  rootContentId = await umbracoApi.document.createVariantDocumentWithParent(rootDocumentTypeId, renderTemplateId, rootContentName, cultures);
  await umbracoApi.document.publishWithCultures(rootContentId, cultures);
  const publishedChildId = await umbracoApi.document.createVariantDocumentWithParent(childDocumentTypeId, renderTemplateId, publishedChildName, cultures, rootContentId);
  await umbracoApi.document.publishWithCultures(publishedChildId, cultures);
  unpublishedChildId = await umbracoApi.document.createVariantDocumentWithParent(childDocumentTypeId, renderTemplateId, unpublishedChildName, cultures, rootContentId);
  await umbracoApi.document.publishWithCultures(unpublishedChildId, cultures);
  const grandchildId = await umbracoApi.document.createVariantDocumentWithParent(grandchildDocumentTypeId, renderTemplateId, grandchildContentName, cultures, unpublishedChildId);
  await umbracoApi.document.publishWithCultures(grandchildId, cultures);

  const domainsData = await umbracoApi.document.getDomains(rootContentId);
  domainsData.domains = [
    {domainName: '/en', isoCode: englishIsoCode},
    {domainName: '/da', isoCode: danishIsoCode}
  ];
  await umbracoApi.document.updateDomains(rootContentId, domainsData);
});

test.afterEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(rootContentName);
  await umbracoApi.documentType.ensureNameNotExists(rootDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(childDocumentTypeName);
  await umbracoApi.documentType.ensureNameNotExists(grandchildDocumentTypeName);
  await umbracoApi.template.ensureNameNotExists(renderTemplateName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
});

test('can see a descendant is rendered when its ancestor is published in the requested culture', async ({umbracoApi, umbracoUi}) => {
  // Act
  const rootDanishUrl = await umbracoApi.document.getDocumentUrlByCulture(rootContentId, danishIsoCode);
  await umbracoUi.contentRender.navigateToRenderedContentPage(rootDanishUrl);

  // Assert
  await umbracoUi.contentRender.doesContentRenderValueContainText(grandchildContentName);
});

test('a descendant is not rendered when its ancestor is unpublished in the requested culture', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.unpublish(unpublishedChildId, [englishIsoCode]);

  // Act
  const rootEnglishUrl = await umbracoApi.document.getDocumentUrlByCulture(rootContentId, englishIsoCode);
  await umbracoUi.contentRender.navigateToRenderedContentPage(rootEnglishUrl);

  // Assert
  // In English only the published child is a descendant the grandchild is excluded because its ancestor is unpublished in English
  await umbracoUi.contentRender.doesContentRenderValueContainText(publishedChildName, true);
});
