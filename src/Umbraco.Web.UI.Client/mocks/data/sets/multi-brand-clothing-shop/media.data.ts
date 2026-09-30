import type { UmbMockMediaModel } from '../../mock-data-set.types.js';
import {
	BRANDING_MEDIA_IDS,
	MEDIA_FOLDER_IDS,
	SITES,
	articleImageId,
	productImageId,
	type UmbMbcsSiteCode,
} from './catalog.js';
import { MEDIA_TYPE_IDS } from './ids.js';

const PHOTO_FILE_NAMES = new Set([
	'hero-little-ones',
	'lo-article-1',
	'lo-article-3',
	'lo-article-4',
	'lo-product-8',
	'lo-product-1',
	'lo-product-2',
	'lo-product-3',
	'lo-product-5',
	'lo-product-10',
	'lo-article-2',
	'gu-product-2',
	'gu-product-7',
	'gu-article-1',
	'gu-article-3',
	'hero-outdoor-shop',
]);

const CREATE_DATE = '2026-01-15 10:00:00';

const folder = (id: string, name: string, hasChildren = true): UmbMockMediaModel => ({
	id,
	createDate: CREATE_DATE,
	parent: null,
	hasChildren,
	noAccess: false,
	isTrashed: false,
	mediaType: { id: MEDIA_TYPE_IDS.folder, icon: 'icon-folder' },
	values: [],
	variants: [
		{
			publishDate: CREATE_DATE,
			culture: null,
			segment: null,
			name,
			createDate: CREATE_DATE,
			updateDate: CREATE_DATE,
		},
	],
	flags: [],
});

const image = (id: string, parentId: string, name: string, fileName: string): UmbMockMediaModel => ({
	id,
	createDate: CREATE_DATE,
	parent: { id: parentId },
	hasChildren: false,
	noAccess: false,
	isTrashed: false,
	mediaType: { id: MEDIA_TYPE_IDS.image, icon: 'icon-picture' },
	values: [
		{
			editorAlias: 'Umbraco.ImageCropper',
			alias: 'umbracoFile',
			value: {
				focalPoint: { left: 0.5, top: 0.5 },
				crops: [],
				src: `/umbraco/backoffice/assets/mbcs-${fileName}.${PHOTO_FILE_NAMES.has(fileName) ? 'jpg' : 'svg'}`,
			},
		},
	],
	variants: [
		{
			publishDate: CREATE_DATE,
			culture: null,
			segment: null,
			name,
			createDate: CREATE_DATE,
			updateDate: CREATE_DATE,
		},
	],
	flags: [],
});

const categoryImages = (
	site: UmbMbcsSiteCode,
	kind: 'product' | 'article',
	parentId: string,
	categories: Array<string>,
) =>
	categories.map((category, index) =>
		image(
			kind === 'product' ? productImageId(site, index) : articleImageId(site, index),
			parentId,
			`${SITES[site].name} – ${category}`,
			`${SITES[site].imageSlug}-${kind}-${index + 1}`,
		),
	);

export const data: Array<UmbMockMediaModel> = [
	folder(MEDIA_FOLDER_IDS.branding, 'Branding'),
	folder(MEDIA_FOLDER_IDS.littleOnesProducts, 'Little Ones – Products'),
	folder(MEDIA_FOLDER_IDS.littleOnesArticles, 'Little Ones – Articles'),
	folder(MEDIA_FOLDER_IDS.outdoorShopProducts, 'The Outdoor Shop – Products'),
	folder(MEDIA_FOLDER_IDS.outdoorShopArticles, 'The Outdoor Shop – Articles'),
	image(BRANDING_MEDIA_IDS.logoLittleOnes, MEDIA_FOLDER_IDS.branding, 'Little Ones logo', 'logo-little-ones'),
	image(BRANDING_MEDIA_IDS.logoOutdoorShop, MEDIA_FOLDER_IDS.branding, 'The Outdoor Shop logo', 'logo-outdoor-shop'),
	image(BRANDING_MEDIA_IDS.heroLittleOnes, MEDIA_FOLDER_IDS.branding, 'Little Ones hero', 'hero-little-ones'),
	image(BRANDING_MEDIA_IDS.heroOutdoorShop, MEDIA_FOLDER_IDS.branding, 'The Outdoor Shop hero', 'hero-outdoor-shop'),
	...categoryImages('LO', 'product', MEDIA_FOLDER_IDS.littleOnesProducts, SITES.LO.productCategories),
	...categoryImages('LO', 'article', MEDIA_FOLDER_IDS.littleOnesArticles, SITES.LO.articleCategories),
	...categoryImages('GU', 'product', MEDIA_FOLDER_IDS.outdoorShopProducts, SITES.GU.productCategories),
	...categoryImages('GU', 'article', MEDIA_FOLDER_IDS.outdoorShopArticles, SITES.GU.articleCategories),
];
