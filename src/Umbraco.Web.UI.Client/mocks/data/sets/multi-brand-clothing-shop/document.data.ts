import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { littleOnesArticles, outdoorShopArticles, type UmbMbcsArticleRow } from './articles.data.js';
import {
	SITES,
	articleCategoryIndex,
	articleImageId,
	heroMediaId,
	logoMediaId,
	productCategoryIndex,
	productImageId,
	type UmbMbcsSiteCode,
} from './catalog.js';
import {
	blockDocumentLink,
	blockDocumentPicker,
	blockGridValue,
	blockMediaPicker,
	blockRichText,
	blockText,
	blockTextarea,
	checkboxListValue,
	dateValue,
	decimalValue,
	documentPickerValue,
	invariantTextValue,
	mediaPickerValue,
	richTextValue,
	rteInlineBlockTag,
	tagsValue,
	textValue,
	textareaValue,
	toggleValue,
	type UmbMbcsBlock,
	type UmbMbcsValue,
} from './document-values.js';
import { AREA_KEYS, CULTURES, CULTURE_DA, CULTURE_EN, DATA_TYPE_IDS, DOCUMENT_TYPE_IDS, mbcsId } from './ids.js';
import { getPageCopy } from './page-copy.js';
import { littleOnesProducts, outdoorShopProducts, type UmbMbcsProductRow } from './products.data.js';
import { littleOnesStores, outdoorShopStores, type UmbMbcsStoreRow } from './stores.data.js';

const CREATE_DATE = '2026-01-15T10:00:00.000Z';
const DAY_IN_MS = 24 * 60 * 60 * 1000;

const DOCUMENT_TYPE_ICONS = {
	siteRoot: 'icon-home',
	contentPage: 'icon-document',
	products: 'icon-shopping-basket-alt-2',
	stores: 'icon-store',
	store: 'icon-pin-location',
	product: 'icon-tag',
	articles: 'icon-newspaper-alt',
	article: 'icon-article',
} as const;

let variantCounter = 0;
let pageCounter = 0;

const rootId = (site: UmbMbcsSiteCode) => mbcsId('document', site === 'LO' ? 1 : 2);
const productsNodeId = (site: UmbMbcsSiteCode) => mbcsId('document', site === 'LO' ? 3 : 4);
const articlesNodeId = (site: UmbMbcsSiteCode) => mbcsId('document', site === 'LO' ? 5 : 6);
const storesNodeId = (site: UmbMbcsSiteCode) => mbcsId('document', site === 'LO' ? 7 : 8);
const productId = (site: UmbMbcsSiteCode, number: number) =>
	mbcsId('document', (site === 'LO' ? 10000 : 20000) + number);
const articleId = (site: UmbMbcsSiteCode, number: number) =>
	mbcsId('document', (site === 'LO' ? 30000 : 40000) + number);

const isoDate = (date: Date) => date.toISOString();

const truncate = (text: string, maxLength: number) =>
	text.length <= maxLength ? text : `${text.slice(0, maxLength - 1).trimEnd()}…`;

const splitTags = (tags: string) => tags.split(',').map((tag) => tag.trim());

interface UmbMbcsDocumentInit {
	id: string;
	parentId: string | null;
	ancestorIds: Array<string>;
	documentTypeId: string;
	icon: string;
	collectionDataTypeId?: string;
	name: string;
	hasChildren: boolean;
	values: Array<UmbMbcsValue>;
	updateDate: string;
	daState?: DocumentVariantStateModel;
	domains?: UmbMockDocumentModel['domains'];
}

const createDocument = (init: UmbMbcsDocumentInit): UmbMockDocumentModel => ({
	id: init.id,
	createDate: CREATE_DATE,
	parent: init.parentId ? { id: init.parentId } : null,
	ancestors: init.ancestorIds.map((id) => ({ id })),
	documentType: {
		id: init.documentTypeId,
		icon: init.icon,
		...(init.collectionDataTypeId ? { collection: { id: init.collectionDataTypeId } } : {}),
	},
	hasChildren: init.hasChildren,
	noAccess: false,
	isProtected: false,
	isTrashed: false,
	template: null,
	variants: CULTURES.map((culture) => {
		const state =
			culture === CULTURE_DA
				? (init.daState ?? DocumentVariantStateModel.PUBLISHED)
				: DocumentVariantStateModel.PUBLISHED;
		return {
			state,
			publishDate: state === DocumentVariantStateModel.PUBLISHED ? init.updateDate : null,
			culture,
			segment: null,
			name: init.name,
			createDate: CREATE_DATE,
			updateDate: init.updateDate,
			id: mbcsId('variant', ++variantCounter),
			flags: [],
		};
	}),
	values: init.values,
	flags: [],
	...(init.domains ? { domains: init.domains } : {}),
});

const seoValues = (
	siteName: string,
	title: string,
	description: string,
	options: { ogImageId?: string; excludeFromSitemap?: boolean } = {},
): Array<UmbMbcsValue> => [
	...textValue('metaTitle', truncate(`${title} | ${siteName}`, 60)),
	...textareaValue('metaDescription', truncate(description, 160)),
	...(options.ogImageId ? mediaPickerValue('ogImage', options.ogImageId) : []),
	...toggleValue('noIndex', false),
	...toggleValue('excludeFromSitemap', options.excludeFromSitemap ?? false),
];

// ---------------------------------------------------------------------------------------------------------------------
// Products
// ---------------------------------------------------------------------------------------------------------------------

const COLOURS = [
	'Natural white',
	'Navy',
	'Sage green',
	'Dusty rose',
	'Charcoal',
	'Mustard',
	'Forest green',
	'Sand',
	'Sky blue',
	'Burgundy',
];

const MATERIALS: Array<[tag: string, material: string]> = [
	['Organic Cotton', 'Organic cotton'],
	['Merino', 'Merino wool'],
	['Cashmere', 'Cashmere'],
	['Alpaca', 'Alpaca blend'],
	['Wool', 'Wool'],
	['Bamboo', 'Bamboo viscose'],
	['Linen', 'Linen'],
	['Denim', 'Cotton denim'],
	['Leather', 'Leather'],
	['Suede', 'Suede'],
	['Fleece', 'Recycled polyester fleece'],
	['Silk', 'Silk'],
	['Corduroy', 'Cotton corduroy'],
	['Velour', 'Cotton velour'],
	['Muslin', 'Cotton muslin'],
	['Down', 'Recycled down'],
	['Waxed Cotton', 'Waxed cotton'],
	['Softshell', 'Recycled polyester softshell'],
	['Canvas', 'Cotton canvas'],
];

const BABY_SIZES = ['50', '56', '62', '68', '74', '80', '86', '92'];
const TODDLER_SIZES = ['86', '92', '98', '104', '110', '116'];
const BIG_KIDS_SIZES = ['122', '128', '134', '140', '146', '152'];
const KIDS_SIZES = ['92', '98', '104', '110', '116', '122', '128'];
const WOMEN_SIZES = ['XS', 'S', 'M', 'L', 'XL'];
const MEN_SIZES = ['S', 'M', 'L', 'XL', 'XXL'];
const UNISEX_SIZES = ['XS', 'S', 'M', 'L', 'XL', 'XXL'];

const SHOE_CATEGORY_INDEX = { LO: 8, GU: 8 } as const;
const ACCESSORY_CATEGORY_INDEX = { LO: 9, GU: 9 } as const;

const productMaterial = (tags: Array<string>) => {
	const match = MATERIALS.find(([tag]) => tags.some((candidate) => candidate.toLowerCase() === tag.toLowerCase()));
	return match?.[1] ?? 'Cotton jersey';
};

const productSizes = (site: UmbMbcsSiteCode, categoryIndex: number, tags: Array<string>) => {
	if (categoryIndex === SHOE_CATEGORY_INDEX[site] || categoryIndex === ACCESSORY_CATEGORY_INDEX[site]) return [];

	if (site === 'LO') {
		if (tags.includes('Baby') || tags.includes('Newborn')) return BABY_SIZES;
		if (tags.includes('Toddler')) return TODDLER_SIZES;
		if (tags.includes('Big Kids')) return BIG_KIDS_SIZES;
		return KIDS_SIZES;
	}

	if (tags.includes('Women')) return WOMEN_SIZES;
	if (tags.includes('Men')) return MEN_SIZES;
	return UNISEX_SIZES;
};

const buildProducts = (
	site: UmbMbcsSiteCode,
	rows: Array<UmbMbcsProductRow>,
	parentId: string,
	ancestorIds: Array<string>,
): Array<UmbMockDocumentModel> =>
	rows.map(([sku, name, tagList, price], index) => {
		const number = index + 1;
		const tags = splitTags(tagList);
		const categoryIndex = productCategoryIndex(number);
		const colour = COLOURS[(number * 7) % COLOURS.length];
		const material = productMaterial(tags);
		const description = `${name} from ${site === 'LO' ? 'Little Ones' : 'The Outdoor Shop'}, made from ${material.toLowerCase()} and designed for everyday wear. Shown in ${colour.toLowerCase()}.`;

		return createDocument({
			id: productId(site, number),
			parentId,
			ancestorIds,
			documentTypeId: DOCUMENT_TYPE_IDS.product,
			icon: DOCUMENT_TYPE_ICONS.product,
			name,
			hasChildren: false,
			updateDate: isoDate(new Date(Date.UTC(2026, 5, 1) + number * DAY_IN_MS)),
			daState: number % 17 === 0 ? DocumentVariantStateModel.DRAFT : DocumentVariantStateModel.PUBLISHED,
			values: [
				...textareaValue('description', description),
				...mediaPickerValue('picture', productImageId(site, categoryIndex)),
				...decimalValue('price', price),
				...invariantTextValue('sku', sku),
				...tagsValue('tags', tags),
				...checkboxListValue('sizes', productSizes(site, categoryIndex, tags)),
				...textValue('colour', colour),
				...textValue('material', material),
				...toggleValue('inStock', number % 9 !== 0),
				...seoValues(SITES[site].name, name, description, { ogImageId: productImageId(site, categoryIndex) }),
			],
		});
	});

// ---------------------------------------------------------------------------------------------------------------------
// Articles
// ---------------------------------------------------------------------------------------------------------------------

const AUTHORS: Record<UmbMbcsSiteCode, Array<string>> = {
	LO: ['Maja Nielsen', 'Jonas Kristensen', 'Sofie Lund', 'Emil Holm', 'Line Bach'],
	GU: ['Anders Vinther', 'Camilla Dahl', 'Mikkel Skov', 'Freja Winther', 'Jesper Toft'],
};

const relatedProductNumbers = (
	site: UmbMbcsSiteCode,
	productRows: Array<UmbMbcsProductRow>,
	articleTags: Array<string>,
	articleNumber: number,
) => {
	const wanted = new Set(articleTags.map((tag) => tag.toLowerCase()));
	return productRows
		.map((row, index) => ({
			number: index + 1,
			score: splitTags(row[2]).filter((tag) => wanted.has(tag.toLowerCase())).length,
		}))
		.sort(
			(a, b) =>
				b.score - a.score ||
				((a.number + articleNumber) % productRows.length) - ((b.number + articleNumber) % productRows.length),
		)
		.slice(0, 3)
		.map((candidate) => candidate.number);
};

const buildArticles = (
	site: UmbMbcsSiteCode,
	rows: Array<UmbMbcsArticleRow>,
	productRows: Array<UmbMbcsProductRow>,
	parentId: string,
	ancestorIds: Array<string>,
): Array<UmbMockDocumentModel> =>
	rows.map(([, title, tagList], index) => {
		const number = index + 1;
		const tags = splitTags(tagList);
		const siteName = SITES[site].name;
		const publishDate = new Date(Date.UTC(2026, 8, 1) - number * 3 * DAY_IN_MS);
		const teaser = `${title}: practical advice from the team at ${siteName} on ${tags.slice(0, 2).join(' and ').toLowerCase()}.`;
		const relatedNumbers = relatedProductNumbers(site, productRows, tags, number);
		const markup = ([teaserKey]: Array<string>) =>
			[
				`<h2>${title}</h2>`,
				`<p>${teaser}</p>`,
				`<p>${rteInlineBlockTag(teaserKey)}</p>`,
				`<p>This guide covers ${tags.join(', ').toLowerCase()}, with tips you can put to use straight away.</p>`,
				'<ul><li>Start with what you already own</li><li>Choose quality over quantity</li><li>Look after your clothes so they last</li></ul>',
			].join('');
		const heroImageId = articleImageId(site, articleCategoryIndex(number));

		return createDocument({
			id: articleId(site, number),
			parentId,
			ancestorIds,
			documentTypeId: DOCUMENT_TYPE_IDS.article,
			icon: DOCUMENT_TYPE_ICONS.article,
			name: title,
			hasChildren: false,
			updateDate: isoDate(publishDate),
			values: [
				...textareaValue('teaser', teaser),
				...mediaPickerValue('heroImage', heroImageId),
				...richTextValue('text', markup, [productTeaser(site, relatedNumbers[0], 'Related product')]),
				...textValue('author', AUTHORS[site][number % AUTHORS[site].length]),
				...dateValue('publishDate', isoDate(publishDate).slice(0, 10)),
				...tagsValue('tags', tags),
				...documentPickerValue(
					'relatedProducts',
					relatedNumbers.map((relatedNumber) => productId(site, relatedNumber)),
				),
				...seoValues(siteName, title, teaser, { ogImageId: heroImageId }),
			],
		});
	});

const buildStores = (
	site: UmbMbcsSiteCode,
	rows: Array<UmbMbcsStoreRow>,
	parentId: string,
	ancestorIds: Array<string>,
): Array<UmbMockDocumentModel> =>
	rows.map(([name, address, phone], index) => {
		const siteName = SITES[site].name;
		const description = `Visit ${siteName} in ${name}.`;

		return createDocument({
			id: mbcsId('document', (site === 'LO' ? 50000 : 60000) + index + 1),
			parentId,
			ancestorIds,
			documentTypeId: DOCUMENT_TYPE_IDS.store,
			icon: DOCUMENT_TYPE_ICONS.store,
			name,
			hasChildren: false,
			updateDate: '2026-09-01T10:00:00.000Z',
			values: [
				...textareaValue('address', address),
				...invariantTextValue('phone', phone),
				...textareaValue('openingHours', 'Mon–Fri 10:00–18:00, Sat 10:00–16:00, Sun closed'),
				...seoValues(siteName, `${siteName} ${name}`, description),
			],
		});
	});

// ---------------------------------------------------------------------------------------------------------------------
// Pages
// ---------------------------------------------------------------------------------------------------------------------

interface UmbMbcsPage {
	name: string;
	kind?: 'products' | 'articles' | 'stores';
	hidden?: boolean;
	children?: Array<UmbMbcsPage>;
}

const page = (name: string, children?: Array<UmbMbcsPage>, hidden = false): UmbMbcsPage => ({ name, children, hidden });
const products = (): UmbMbcsPage => ({ name: 'Products', kind: 'products' });
const articles = (): UmbMbcsPage => ({ name: 'Articles', kind: 'articles' });
const stores = (): UmbMbcsPage => ({ name: 'Stores', kind: 'stores' });

const LITTLE_ONES_PAGES: Array<UmbMbcsPage> = [
	products(),
	articles(),
	stores(),
	page('New Customer', [page('Baby (0–2 years)'), page('Toddler (2–6 years)'), page('Big Kids (6–12 years)')]),
	page('Sale', [page('Up to 50% Off'), page('Last Sizes')]),
	page('Guides', [
		page('Gift Guide', [
			page('Baby Shower Gifts'),
			page('Birthday Gifts'),
			page('Christmas Gifts'),
			page('Gift Cards'),
		]),
		page('Size Guide', [page('Baby Sizes (50–92 cm)'), page('Kids Sizes (98–152 cm)'), page('Shoe Sizes (EU 18–38)')]),
	]),
	page('Customer Service', [
		page('Shipping & Delivery'),
		page('Returns & Exchanges'),
		page('Payment Options'),
		page('FAQ'),
		page('Contact Us'),
	]),
	page('About Us', [page('Our Story'), page('Sustainability'), page('Careers')]),
	page('My Account', [page('Order History'), page('Wishlist'), page('Addresses')], true),
	page('Basket', [page('Checkout', [page('Order Confirmation')])], true),
	page('Legal', [page('Terms & Conditions'), page('Privacy Policy (GDPR)'), page('Cookie Policy')], true),
];

const OUTDOOR_SHOP_PAGES: Array<UmbMbcsPage> = [
	products(),
	articles(),
	stores(),
	page('New Customer', [page('Women'), page('Men'), page('Unisex')]),
	page('Sale', [page('Up to 50% Off'), page('Outlet')]),
	page('Lookbook', [page('Autumn/Winter 2026'), page('Workwear Edit'), page('Weekend Outdoors')]),
	page('Guides', [
		page('Size Guide', [
			page("Women's Sizes (EU 32–48)"),
			page("Men's Sizes (EU 44–58)"),
			page('Shoe Sizes (EU 36–47)'),
		]),
	]),
	page('Repair Service', [page('Book a Repair'), page('Care Instructions'), page('Lifetime Guarantee')]),
	page('Customer Service', [
		page('Shipping & Delivery'),
		page('Returns & Exchanges'),
		page('Payment Options'),
		page('Gift Cards'),
		page('FAQ'),
		page('Contact Us'),
	]),
	page('About Us', [page('Our Story'), page('Sustainability'), page('Careers'), page('Press')]),
	page(
		'My Account',
		[page('Order History'), page('Wishlist'), page('Addresses'), page('Newsletter Preferences')],
		true,
	),
	page('Basket', [page('Checkout', [page('Order Confirmation')])], true),
	page('Legal', [page('Terms & Conditions'), page('Privacy Policy (GDPR)'), page('Cookie Policy')], true),
];

const HOME_PRODUCT_NUMBERS = [1, 14, 23, 52];
const HOME_PRODUCT_LABELS = ['New in', 'Customer favourite', 'Staff pick', 'Bestseller'];
const HOME_ARTICLE_NUMBERS = [1, 21, 41];

const oneColumn = (items: Array<UmbMbcsBlock>): UmbMbcsBlock => ({
	elementTypeId: DOCUMENT_TYPE_IDS.oneColumnLayout,
	columnSpan: 12,
	areas: [{ key: AREA_KEYS.oneColumnMain, items }],
});

const twoColumn = (left: Array<UmbMbcsBlock>, right: Array<UmbMbcsBlock>): UmbMbcsBlock => ({
	elementTypeId: DOCUMENT_TYPE_IDS.twoColumnLayout,
	columnSpan: 12,
	areas: [
		{ key: AREA_KEYS.twoColumnLeft, items: left },
		{ key: AREA_KEYS.twoColumnRight, items: right },
	],
});

const heroBlock = (site: UmbMbcsSiteCode, headline: string, subheadline: string): UmbMbcsBlock => ({
	elementTypeId: DOCUMENT_TYPE_IDS.heroBlock,
	columnSpan: 12,
	values: [
		blockText('headline', headline),
		blockTextarea('subheadline', subheadline),
		blockMediaPicker('image', heroMediaId(site)),
		blockDocumentLink('link', productsNodeId(site), 'Shop now'),
	],
});

const textBlock = (markup: string, columnSpan = 12): UmbMbcsBlock => ({
	elementTypeId: DOCUMENT_TYPE_IDS.textBlock,
	columnSpan,
	values: [blockRichText('text', markup)],
});

const imageBlock = (mediaId: string, caption: string): UmbMbcsBlock => ({
	elementTypeId: DOCUMENT_TYPE_IDS.imageBlock,
	columnSpan: 12,
	values: [blockMediaPicker('image', mediaId), blockText('caption', caption)],
});

const productTeaser = (site: UmbMbcsSiteCode, number: number, label: string): UmbMbcsBlock => ({
	elementTypeId: DOCUMENT_TYPE_IDS.productTeaserBlock,
	columnSpan: 3,
	values: [blockDocumentPicker('product', productId(site, number)), blockText('label', label)],
});

const articleTeaser = (site: UmbMbcsSiteCode, number: number): UmbMbcsBlock => ({
	elementTypeId: DOCUMENT_TYPE_IDS.articleTeaserBlock,
	columnSpan: 4,
	values: [blockDocumentPicker('article', articleId(site, number))],
});

const homeBlocks = (site: UmbMbcsSiteCode): Array<UmbMbcsBlock> => [
	heroBlock(site, SITES[site].name, 'Quality clothing, made to last.'),
	oneColumn([
		textBlock(
			`<h2>Welcome to ${SITES[site].name}</h2><p>Browse our full range, read our guides and find something you will love.</p>`,
		),
	]),
	oneColumn(HOME_PRODUCT_NUMBERS.map((number, index) => productTeaser(site, number, HOME_PRODUCT_LABELS[index]))),
	twoColumn(
		[imageBlock(productImageId(site, 0), 'Our latest arrivals')],
		[
			textBlock(
				'<h3>Made to last</h3><p>We design for everyday life and repair, re-use and recycle wherever we can.</p>',
			),
		],
	),
	oneColumn(HOME_ARTICLE_NUMBERS.map((number) => articleTeaser(site, number))),
];

const landingBlocks = (site: UmbMbcsSiteCode, name: string, index: number, hidden: boolean): Array<UmbMbcsBlock> => {
	const copy = getPageCopy(site, name);
	const headline = copy?.headline ?? name;
	const paragraphs = copy?.paragraphs ?? [`This page is part of ${SITES[site].name}.`];

	return [
		heroBlock(site, name, `Everything you need to know about ${name.toLowerCase()}.`),
		oneColumn([textBlock(`<h2>${headline}</h2>${paragraphs.map((paragraph) => `<p>${paragraph}</p>`).join('')}`)]),
		...(hidden
			? []
			: [
					twoColumn(
						[imageBlock(productImageId(site, index % 10), name)],
						[textBlock(`<p>${copy?.aside ?? `Explore ${name.toLowerCase()}.`}</p>`)],
					),
				]),
	];
};

const buildPages = (
	site: UmbMbcsSiteCode,
	pages: Array<UmbMbcsPage>,
	parentId: string,
	ancestorIds: Array<string>,
	output: Array<UmbMockDocumentModel>,
) => {
	const siteName = SITES[site].name;
	const productRows = site === 'LO' ? littleOnesProducts : outdoorShopProducts;
	const articleRows = site === 'LO' ? littleOnesArticles : outdoorShopArticles;
	const storeRows = site === 'LO' ? littleOnesStores : outdoorShopStores;
	const isFirstLevel = ancestorIds.length === 0;

	pages.forEach((entry, index) => {
		const ancestors = [...ancestorIds, parentId];
		const id =
			entry.kind === 'products'
				? productsNodeId(site)
				: entry.kind === 'articles'
					? articlesNodeId(site)
					: entry.kind === 'stores'
						? storesNodeId(site)
						: mbcsId('document', (site === 'LO' ? 100 : 500) + ++pageCounter);
		const intro = `Everything from ${siteName}, in one place.`;

		if (entry.kind) {
			const isProducts = entry.kind === 'products';
			const isStores = entry.kind === 'stores';
			const listing = isProducts
				? {
						documentTypeId: DOCUMENT_TYPE_IDS.products,
						icon: DOCUMENT_TYPE_ICONS.products,
						collectionDataTypeId: DATA_TYPE_IDS.productsCollection,
					}
				: isStores
					? {
							documentTypeId: DOCUMENT_TYPE_IDS.stores,
							icon: DOCUMENT_TYPE_ICONS.stores,
							collectionDataTypeId: DATA_TYPE_IDS.storesCollection,
						}
					: {
							documentTypeId: DOCUMENT_TYPE_IDS.articles,
							icon: DOCUMENT_TYPE_ICONS.articles,
							collectionDataTypeId: DATA_TYPE_IDS.articlesCollection,
						};
			output.push(
				createDocument({
					id,
					parentId,
					ancestorIds: ancestors,
					...listing,
					name: entry.name,
					hasChildren: true,
					updateDate: '2026-09-01T10:00:00.000Z',
					values: [
						...textValue('title', entry.name),
						...textareaValue('intro', intro),
						...mediaPickerValue('heroImage', heroMediaId(site)),
						...blockGridValue('content', [heroBlock(site, entry.name, intro)]),
						...seoValues(siteName, entry.name, intro),
					],
				}),
			);
			output.push(
				...(isProducts
					? buildProducts(site, productRows, id, [...ancestors, id])
					: isStores
						? buildStores(site, storeRows, id, [...ancestors, id])
						: buildArticles(site, articleRows, productRows, id, [...ancestors, id])),
			);
			return;
		}

		const hidden = entry.hidden ?? false;
		output.push(
			createDocument({
				id,
				parentId,
				ancestorIds: ancestors,
				documentTypeId: DOCUMENT_TYPE_IDS.contentPage,
				icon: DOCUMENT_TYPE_ICONS.contentPage,
				name: entry.name,
				hasChildren: (entry.children?.length ?? 0) > 0,
				updateDate: '2026-09-01T10:00:00.000Z',
				values: [
					...(isFirstLevel ? blockGridValue('content', landingBlocks(site, entry.name, index, hidden)) : []),
					...seoValues(siteName, entry.name, `Everything you need to know about ${entry.name.toLowerCase()}.`, {
						excludeFromSitemap: hidden,
					}),
				],
			}),
		);

		if (entry.children) buildPages(site, entry.children, id, ancestors, output);
	});
};

const buildSite = (site: UmbMbcsSiteCode, pages: Array<UmbMbcsPage>): Array<UmbMockDocumentModel> => {
	const { name, domains } = SITES[site];
	const id = rootId(site);
	const output: Array<UmbMockDocumentModel> = [
		createDocument({
			id,
			parentId: null,
			ancestorIds: [],
			documentTypeId: DOCUMENT_TYPE_IDS.siteRoot,
			icon: DOCUMENT_TYPE_ICONS.siteRoot,
			name,
			hasChildren: true,
			updateDate: '2026-09-01T10:00:00.000Z',
			domains: { defaultIsoCode: CULTURE_EN, domains },
			values: [
				...textValue('siteName', name),
				...mediaPickerValue('logo', logoMediaId(site)),
				...blockGridValue('content', homeBlocks(site)),
				...seoValues(name, name, `Welcome to ${name}. Quality clothing, made to last.`, {
					ogImageId: heroMediaId(site),
				}),
			],
		}),
	];

	buildPages(site, pages, id, [], output);
	return output;
};

export const data: Array<UmbMockDocumentModel> = [
	...buildSite('LO', LITTLE_ONES_PAGES),
	...buildSite('GU', OUTDOOR_SHOP_PAGES),
];
