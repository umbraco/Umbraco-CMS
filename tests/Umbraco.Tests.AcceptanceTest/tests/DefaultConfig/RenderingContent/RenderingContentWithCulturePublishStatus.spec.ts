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
const childToUnpublishName = 'ChildToUnpublish';
const grandchildContentName = 'Grandchild';

// Template
const renderTemplateName = 'RenderDescendantsTemplate';

let renderTemplateId = '';
let rootContentId = '';
let childToUnpublishId = '';

test.beforeEach(async ({umbracoApi}) => {
  await umbracoApi.document.ensureNameNotExists(rootContentName);
  await umbracoApi.language.ensureIsoCodeNotExists(danishIsoCode);
  await umbracoApi.language.createDanishLanguage();
  renderTemplateId = await umbracoApi.template.createTemplateWithDisplayingValue(renderTemplateName, '\n@foreach (var content in Model.Descendants())\n{\n\t<li>@content.Name</li>\n}');
  const grandchildDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithTemplateAndAllowedChildNode(grandchildDocumentTypeName, renderTemplateId);
  const childDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithTemplateAndAllowedChildNode(childDocumentTypeName, renderTemplateId, false, grandchildDocumentTypeId);
  const rootDocumentTypeId = await umbracoApi.documentType.createVariantDocumentTypeWithTemplateAndAllowedChildNode(rootDocumentTypeName, renderTemplateId, true, childDocumentTypeId);

  rootContentId = await umbracoApi.document.createVariantDocumentWithTemplateAndParent(rootContentName, rootDocumentTypeId, renderTemplateId, cultures);
  await umbracoApi.document.publishDocumentWithCultures(rootContentId, cultures);
  const publishedChildId = await umbracoApi.document.createVariantDocumentWithTemplateAndParent(publishedChildName, childDocumentTypeId, renderTemplateId, cultures, rootContentId);
  await umbracoApi.document.publishDocumentWithCultures(publishedChildId, cultures);
  childToUnpublishId = await umbracoApi.document.createVariantDocumentWithTemplateAndParent(childToUnpublishName, childDocumentTypeId, renderTemplateId, cultures, rootContentId);
  await umbracoApi.document.publishDocumentWithCultures(childToUnpublishId, cultures);
  const grandchildId = await umbracoApi.document.createVariantDocumentWithTemplateAndParent(grandchildContentName, grandchildDocumentTypeId, renderTemplateId, cultures, childToUnpublishId);
  await umbracoApi.document.publishDocumentWithCultures(grandchildId, cultures);

  await umbracoApi.document.updateDomainsForVariantDocument(rootContentId, [
    {domainName: '/en', isoCode: englishIsoCode},
    {domainName: '/da', isoCode: danishIsoCode}
  ]);
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
  await umbracoUi.contentRender.doesContentRenderValueContainText(publishedChildName);
  await umbracoUi.contentRender.doesContentRenderValueContainText(grandchildContentName);
});

test('a descendant is not rendered when its ancestor is unpublished in the requested culture', async ({umbracoApi, umbracoUi}) => {
  // Arrange
  await umbracoApi.document.unpublish(childToUnpublishId, [englishIsoCode]);

  // Act
  const rootEnglishUrl = await umbracoApi.document.getDocumentUrlByCulture(rootContentId, englishIsoCode);
  await umbracoUi.contentRender.navigateToRenderedContentPage(rootEnglishUrl);

  // Assert
  // In English the published child is a descendant, but the grandchild is excluded because its ancestor is unpublished in English
  await umbracoUi.contentRender.doesContentRenderValueContainText(publishedChildName);
  await umbracoUi.contentRender.doesContentRenderValueNotContainText(grandchildContentName);
});
