import { CompositionTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbMockDocumentTypeModel } from '../../mock-data-set.types.js';
import { DATA_TYPE_IDS, DOCUMENT_TYPE_IDS, mbcsId } from './ids.js';

type UmbMbcsProperty = UmbMockDocumentTypeModel['properties'][number];
type UmbMbcsContainer = UmbMockDocumentTypeModel['containers'][number];

interface UmbMbcsPropertyOptions {
	alias: string;
	name: string;
	dataTypeId: string;
	container: string | null;
	varies?: boolean;
	mandatory?: boolean;
	labelOnTop?: boolean;
}

let propertyCounter = 0;
let containerCounter = 0;

const tab = (name: string, sortOrder = 0): UmbMbcsContainer => ({
	id: mbcsId('container', ++containerCounter),
	parent: null,
	name,
	type: 'Tab',
	sortOrder,
});

const toProperties = (options: Array<UmbMbcsPropertyOptions>): Array<UmbMbcsProperty> =>
	options.map((option, sortOrder) => ({
		id: mbcsId('property', ++propertyCounter),
		container: option.container ? { id: option.container } : null,
		alias: option.alias,
		name: option.name,
		description: null,
		dataType: { id: option.dataTypeId },
		variesByCulture: option.varies ?? false,
		variesBySegment: false,
		sortOrder,
		validation: {
			mandatory: option.mandatory ?? false,
			mandatoryMessage: null,
			regEx: null,
			regExMessage: null,
		},
		appearance: { labelOnTop: option.labelOnTop ?? false },
	}));

interface UmbMbcsDocumentTypeOptions {
	id: string;
	alias: string;
	name: string;
	icon: string;
	allowedAsRoot?: boolean;
	isElement?: boolean;
	parent?: string;
	properties?: Array<UmbMbcsPropertyOptions>;
	containers?: Array<UmbMbcsContainer>;
	allowedChildren?: Array<string>;
	compositions?: Array<string>;
	collection?: string;
}

const documentType = (options: UmbMbcsDocumentTypeOptions): UmbMockDocumentTypeModel => ({
	id: options.id,
	alias: options.alias,
	name: options.name,
	description: null,
	icon: options.icon,
	allowedTemplates: [],
	defaultTemplate: null,
	allowedAsRoot: options.allowedAsRoot ?? false,
	variesByCulture: true,
	variesBySegment: false,
	isElement: options.isElement ?? false,
	hasChildren: false,
	parent: options.parent ? { id: options.parent } : null,
	isFolder: false,
	properties: toProperties(options.properties ?? []),
	containers: options.containers ?? [],
	allowedDocumentTypes: (options.allowedChildren ?? []).map((id, sortOrder) => ({
		documentType: { id },
		sortOrder,
	})),
	compositions: (options.compositions ?? []).map((id) => ({
		documentType: { id },
		compositionType: CompositionTypeModel.COMPOSITION,
	})),
	cleanup: {
		preventCleanup: false,
		keepAllVersionsNewerThanDays: null,
		keepLatestVersionPerDayForDays: null,
	},
	...(options.collection ? { collection: { id: options.collection } } : {}),
	flags: [],
});

const seoTab = tab('SEO', 10);
const pageContentTab = tab('Content');
const siteTab = tab('Site', 1);
const productsTab = tab('Content');
const articlesTab = tab('Content');
const storesTab = tab('Content');
const storeTab = tab('Store');
const productMaterialTab = tab('Content');
const materialShowcaseTab = tab('Content');
const productTab = tab('Product');
const productDocumentTab = tab('Document');
const articleTab = tab('Article');
const heroBlockTab = tab('Content');
const imageBlockTab = tab('Content');
const textBlockTab = tab('Content');
const productTeaserTab = tab('Content');
const articleTeaserTab = tab('Content');

const { textstring, textarea, mediaPicker, toggle } = DATA_TYPE_IDS;

export const data: Array<UmbMockDocumentTypeModel> = [
	{
		...documentType({
			id: DOCUMENT_TYPE_IDS.blocksFolder,
			alias: 'blocksFolder',
			name: 'Blocks',
			icon: 'icon-folder',
		}),
		hasChildren: true,
		isFolder: true,
		allowedAsRoot: true,
		variesByCulture: false,
	},
	documentType({
		id: DOCUMENT_TYPE_IDS.seoComposition,
		alias: 'seoComposition',
		name: 'SEO',
		icon: 'icon-search',
		containers: [seoTab],
		properties: [
			{
				alias: 'metaTitle',
				name: 'Meta title',
				dataTypeId: DATA_TYPE_IDS.textstringMax60,
				container: seoTab.id,
				varies: true,
			},
			{
				alias: 'metaDescription',
				name: 'Meta description',
				dataTypeId: DATA_TYPE_IDS.textareaMax160,
				container: seoTab.id,
				varies: true,
			},
			{ alias: 'ogImage', name: 'Social share image', dataTypeId: mediaPicker, container: seoTab.id },
			{
				alias: 'canonicalUrl',
				name: 'Canonical URL',
				dataTypeId: DATA_TYPE_IDS.multiUrlPickerSingle,
				container: seoTab.id,
			},
			{ alias: 'noIndex', name: 'Hide from search engines', dataTypeId: toggle, container: seoTab.id },
			{ alias: 'excludeFromSitemap', name: 'Exclude from sitemap', dataTypeId: toggle, container: seoTab.id },
		],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.pageContentComposition,
		alias: 'pageContentComposition',
		name: 'Page Content',
		icon: 'icon-grid',
		containers: [pageContentTab],
		properties: [
			{
				alias: 'content',
				name: 'Content',
				dataTypeId: DATA_TYPE_IDS.pageContentBlockGrid,
				container: pageContentTab.id,
				labelOnTop: true,
			},
		],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.siteRoot,
		alias: 'siteRoot',
		name: 'Site Root',
		icon: 'icon-home',
		allowedAsRoot: true,
		containers: [siteTab],
		properties: [
			{ alias: 'siteName', name: 'Site name', dataTypeId: textstring, container: siteTab.id, varies: true },
			{ alias: 'logo', name: 'Logo', dataTypeId: mediaPicker, container: siteTab.id },
		],
		allowedChildren: [
			DOCUMENT_TYPE_IDS.contentPage,
			DOCUMENT_TYPE_IDS.products,
			DOCUMENT_TYPE_IDS.articles,
			DOCUMENT_TYPE_IDS.stores,
		],
		compositions: [DOCUMENT_TYPE_IDS.seoComposition, DOCUMENT_TYPE_IDS.pageContentComposition],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.contentPage,
		alias: 'contentPage',
		name: 'Content Page',
		icon: 'icon-document',
		allowedChildren: [DOCUMENT_TYPE_IDS.contentPage],
		compositions: [DOCUMENT_TYPE_IDS.seoComposition, DOCUMENT_TYPE_IDS.pageContentComposition],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.products,
		alias: 'products',
		name: 'Products',
		icon: 'icon-shopping-basket-alt-2',
		containers: [productsTab],
		properties: [
			{ alias: 'title', name: 'Title', dataTypeId: textstring, container: productsTab.id, varies: true },
			{ alias: 'intro', name: 'Intro', dataTypeId: textarea, container: productsTab.id, varies: true },
			{ alias: 'heroImage', name: 'Hero image', dataTypeId: mediaPicker, container: productsTab.id },
		],
		allowedChildren: [DOCUMENT_TYPE_IDS.product],
		compositions: [DOCUMENT_TYPE_IDS.seoComposition, DOCUMENT_TYPE_IDS.pageContentComposition],
		collection: DATA_TYPE_IDS.productsCollection,
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.product,
		alias: 'product',
		name: 'Product',
		icon: 'icon-tag',
		containers: [productTab],
		properties: [
			{
				alias: 'description',
				name: 'Description',
				dataTypeId: textarea,
				container: productTab.id,
				varies: true,
			},
			{
				alias: 'picture',
				name: 'Picture',
				dataTypeId: mediaPicker,
				container: productTab.id,
				mandatory: true,
			},
			{
				alias: 'price',
				name: 'Price (DKK)',
				dataTypeId: DATA_TYPE_IDS.decimal,
				container: productTab.id,
				mandatory: true,
			},
			{ alias: 'sku', name: 'SKU', dataTypeId: textstring, container: productTab.id },
			{ alias: 'tags', name: 'Tags', dataTypeId: DATA_TYPE_IDS.productTags, container: productTab.id },
			{
				alias: 'materials',
				name: 'Materials',
				dataTypeId: DATA_TYPE_IDS.productMaterialsBlockList,
				container: productTab.id,
			},
			{ alias: 'sizes', name: 'Sizes', dataTypeId: DATA_TYPE_IDS.sizes, container: productTab.id },
			{ alias: 'colour', name: 'Colour', dataTypeId: textstring, container: productTab.id, varies: true },
			{ alias: 'material', name: 'Material', dataTypeId: textstring, container: productTab.id, varies: true },
			{ alias: 'inStock', name: 'In stock', dataTypeId: DATA_TYPE_IDS.toggleDefaultOn, container: productTab.id },
		],
		allowedChildren: [DOCUMENT_TYPE_IDS.productDocument],
		compositions: [DOCUMENT_TYPE_IDS.seoComposition],
		collection: DATA_TYPE_IDS.productDocumentsCollection,
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.productDocument,
		alias: 'productDocument',
		name: 'Product Document',
		icon: 'icon-document',
		containers: [productDocumentTab],
		properties: [
			{
				alias: 'documentCategory',
				name: 'Type',
				dataTypeId: DATA_TYPE_IDS.productDocumentCategory,
				container: productDocumentTab.id,
				mandatory: true,
			},
			{
				alias: 'file',
				name: 'File',
				dataTypeId: DATA_TYPE_IDS.productDocumentFile,
				container: productDocumentTab.id,
			},
			{ alias: 'summary', name: 'Summary', dataTypeId: textarea, container: productDocumentTab.id, varies: true },
			{
				alias: 'languages',
				name: 'Languages',
				dataTypeId: DATA_TYPE_IDS.productDocumentLanguages,
				container: productDocumentTab.id,
			},
			{ alias: 'version', name: 'Version', dataTypeId: textstring, container: productDocumentTab.id },
			{ alias: 'pageCount', name: 'Pages', dataTypeId: DATA_TYPE_IDS.integer, container: productDocumentTab.id },
			{
				alias: 'revisionDate',
				name: 'Revision date',
				dataTypeId: DATA_TYPE_IDS.datePicker,
				container: productDocumentTab.id,
			},
			{
				alias: 'showOnProductPage',
				name: 'Show on product page',
				dataTypeId: DATA_TYPE_IDS.toggleDefaultOn,
				container: productDocumentTab.id,
			},
		],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.articles,
		alias: 'articles',
		name: 'Articles',
		icon: 'icon-newspaper-alt',
		containers: [articlesTab],
		properties: [
			{ alias: 'title', name: 'Title', dataTypeId: textstring, container: articlesTab.id, varies: true },
			{ alias: 'intro', name: 'Intro', dataTypeId: textarea, container: articlesTab.id, varies: true },
			{ alias: 'heroImage', name: 'Hero image', dataTypeId: mediaPicker, container: articlesTab.id },
		],
		allowedChildren: [DOCUMENT_TYPE_IDS.article],
		compositions: [DOCUMENT_TYPE_IDS.seoComposition, DOCUMENT_TYPE_IDS.pageContentComposition],
		collection: DATA_TYPE_IDS.articlesCollection,
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.article,
		alias: 'article',
		name: 'Article',
		icon: 'icon-notepad',
		containers: [articleTab],
		properties: [
			{ alias: 'teaser', name: 'Teaser', dataTypeId: textarea, container: articleTab.id, varies: true },
			{ alias: 'heroImage', name: 'Hero image', dataTypeId: mediaPicker, container: articleTab.id },
			{
				alias: 'text',
				name: 'Text',
				dataTypeId: DATA_TYPE_IDS.articleRichTextEditor,
				container: articleTab.id,
				varies: true,
				mandatory: true,
			},
			{ alias: 'author', name: 'Author', dataTypeId: textstring, container: articleTab.id, varies: true },
			{
				alias: 'publishDate',
				name: 'Publish date',
				dataTypeId: DATA_TYPE_IDS.datePicker,
				container: articleTab.id,
			},
			{ alias: 'tags', name: 'Tags', dataTypeId: DATA_TYPE_IDS.articleTags, container: articleTab.id },
			{
				alias: 'relatedProducts',
				name: 'Related products',
				dataTypeId: DATA_TYPE_IDS.productPicker,
				container: articleTab.id,
			},
		],
		compositions: [DOCUMENT_TYPE_IDS.seoComposition],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.stores,
		alias: 'stores',
		name: 'Stores',
		icon: 'icon-store',
		containers: [storesTab],
		properties: [
			{ alias: 'title', name: 'Title', dataTypeId: textstring, container: storesTab.id, varies: true },
			{ alias: 'intro', name: 'Intro', dataTypeId: textarea, container: storesTab.id, varies: true },
			{ alias: 'heroImage', name: 'Hero image', dataTypeId: mediaPicker, container: storesTab.id },
		],
		allowedChildren: [DOCUMENT_TYPE_IDS.store],
		compositions: [DOCUMENT_TYPE_IDS.seoComposition, DOCUMENT_TYPE_IDS.pageContentComposition],
		collection: DATA_TYPE_IDS.storesCollection,
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.store,
		alias: 'store',
		name: 'Store',
		icon: 'icon-pin-location',
		containers: [storeTab],
		properties: [
			{ alias: 'address', name: 'Address', dataTypeId: textarea, container: storeTab.id, varies: true },
			{ alias: 'phone', name: 'Phone', dataTypeId: textstring, container: storeTab.id },
			{
				alias: 'openingHours',
				name: 'Opening hours',
				dataTypeId: textarea,
				container: storeTab.id,
				varies: true,
			},
		],
		compositions: [DOCUMENT_TYPE_IDS.seoComposition],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.productMaterial,
		alias: 'productMaterial',
		name: 'Product Material',
		icon: 'icon-palette',
		isElement: true,
		parent: DOCUMENT_TYPE_IDS.blocksFolder,
		containers: [productMaterialTab],
		properties: [
			{
				alias: 'material',
				name: 'Material',
				dataTypeId: textstring,
				container: productMaterialTab.id,
				varies: true,
			},
			{ alias: 'image', name: 'Image', dataTypeId: mediaPicker, container: productMaterialTab.id },
		],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.materialShowcaseBlock,
		alias: 'materialShowcaseBlock',
		name: 'Material Showcase',
		icon: 'icon-palette',
		isElement: true,
		parent: DOCUMENT_TYPE_IDS.blocksFolder,
		containers: [materialShowcaseTab],
		properties: [
			{
				alias: 'headline',
				name: 'Headline',
				dataTypeId: textstring,
				container: materialShowcaseTab.id,
				varies: true,
			},
			{
				alias: 'intro',
				name: 'Intro',
				dataTypeId: textarea,
				container: materialShowcaseTab.id,
				varies: true,
			},
			{
				alias: 'materials',
				name: 'Materials',
				dataTypeId: DATA_TYPE_IDS.productMaterialsBlockList,
				container: materialShowcaseTab.id,
			},
		],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.oneColumnLayout,
		alias: 'oneColumnLayout',
		name: 'One Column Layout',
		icon: 'icon-layout',
		isElement: true,
		parent: DOCUMENT_TYPE_IDS.blocksFolder,
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.twoColumnLayout,
		alias: 'twoColumnLayout',
		name: 'Two Column Layout',
		icon: 'icon-layout',
		isElement: true,
		parent: DOCUMENT_TYPE_IDS.blocksFolder,
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.heroBlock,
		alias: 'heroBlock',
		name: 'Hero Block',
		icon: 'icon-picture',
		isElement: true,
		parent: DOCUMENT_TYPE_IDS.blocksFolder,
		containers: [heroBlockTab],
		properties: [
			{ alias: 'headline', name: 'Headline', dataTypeId: textstring, container: heroBlockTab.id, varies: true },
			{
				alias: 'subheadline',
				name: 'Subheadline',
				dataTypeId: textarea,
				container: heroBlockTab.id,
				varies: true,
			},
			{ alias: 'image', name: 'Image', dataTypeId: mediaPicker, container: heroBlockTab.id },
			{
				alias: 'link',
				name: 'Link',
				dataTypeId: DATA_TYPE_IDS.multiUrlPickerSingle,
				container: heroBlockTab.id,
			},
		],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.imageBlock,
		alias: 'imageBlock',
		name: 'Image Block',
		icon: 'icon-picture',
		isElement: true,
		parent: DOCUMENT_TYPE_IDS.blocksFolder,
		containers: [imageBlockTab],
		properties: [
			{ alias: 'image', name: 'Image', dataTypeId: mediaPicker, container: imageBlockTab.id },
			{ alias: 'caption', name: 'Caption', dataTypeId: textstring, container: imageBlockTab.id, varies: true },
		],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.textBlock,
		alias: 'textBlock',
		name: 'Text Block',
		icon: 'icon-font',
		isElement: true,
		parent: DOCUMENT_TYPE_IDS.blocksFolder,
		containers: [textBlockTab],
		properties: [
			{
				alias: 'text',
				name: 'Text',
				dataTypeId: DATA_TYPE_IDS.richTextEditor,
				container: textBlockTab.id,
				varies: true,
			},
		],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.productTeaserBlock,
		alias: 'productTeaserBlock',
		name: 'Product Teaser',
		icon: 'icon-tag',
		isElement: true,
		parent: DOCUMENT_TYPE_IDS.blocksFolder,
		containers: [productTeaserTab],
		properties: [
			{
				alias: 'product',
				name: 'Product',
				dataTypeId: DATA_TYPE_IDS.productPickerSingle,
				container: productTeaserTab.id,
			},
			{ alias: 'label', name: 'Label', dataTypeId: textstring, container: productTeaserTab.id, varies: true },
		],
	}),
	documentType({
		id: DOCUMENT_TYPE_IDS.articleTeaserBlock,
		alias: 'articleTeaserBlock',
		name: 'Article Teaser',
		icon: 'icon-article',
		isElement: true,
		parent: DOCUMENT_TYPE_IDS.blocksFolder,
		containers: [articleTeaserTab],
		properties: [
			{
				alias: 'article',
				name: 'Article',
				dataTypeId: DATA_TYPE_IDS.articlePickerSingle,
				container: articleTeaserTab.id,
			},
		],
	}),
];
