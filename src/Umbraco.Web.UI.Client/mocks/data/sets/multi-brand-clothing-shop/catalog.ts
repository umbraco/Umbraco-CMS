import { CULTURE_DA, CULTURE_EN, mbcsId } from './ids.js';

export type UmbMbcsSiteCode = 'LO' | 'GU';

export interface UmbMbcsSite {
	code: UmbMbcsSiteCode;
	/** Used for file names of the placeholder images. */
	imageSlug: 'lo' | 'gu';
	name: string;
	domains: Array<{ domainName: string; isoCode: string }>;
	productCategories: Array<string>;
	articleCategories: Array<string>;
}

export const SITES: Record<UmbMbcsSiteCode, UmbMbcsSite> = {
	LO: {
		code: 'LO',
		imageSlug: 'lo',
		name: 'Little Ones Clothing Shop',
		domains: [
			{ domainName: 'littleones.com', isoCode: CULTURE_EN },
			{ domainName: 'littleones.dk', isoCode: CULTURE_DA },
		],
		productCategories: [
			'Bodysuits & Rompers',
			'Tops & T-shirts',
			'Knitwear & Sweaters',
			'Trousers & Leggings',
			'Dresses & Skirts',
			'Outerwear',
			'Rainwear & Thermal',
			'Sleepwear',
			'Shoes & Boots',
			'Accessories',
		],
		articleCategories: [
			'Size & Fit',
			'Care & Sustainability',
			'Seasonal Dressing',
			'Everyday Family Life',
			'Behind the Seams',
		],
	},
	GU: {
		code: 'GU',
		imageSlug: 'gu',
		name: 'The Outdoor Shop',
		domains: [
			{ domainName: 'outdoorshop.com', isoCode: CULTURE_EN },
			{ domainName: 'outdoorshop.dk', isoCode: CULTURE_DA },
		],
		productCategories: [
			'T-shirts & Tops',
			'Shirts & Blouses',
			'Knitwear',
			'Trousers & Shorts',
			'Dresses & Skirts',
			'Coats & Jackets',
			'Outdoor',
			'Sleep & Loungewear',
			'Shoes',
			'Bags & Accessories',
		],
		articleCategories: [
			'Style Guides',
			'Care & Repair',
			'Outdoor Living',
			'Materials & Sustainability',
			'Behind the Seams',
		],
	},
};

export const PRODUCTS_PER_CATEGORY = 10;
export const ARTICLES_PER_CATEGORY = 20;

export const MEDIA_FOLDER_IDS = {
	branding: mbcsId('media', 1),
	littleOnesProducts: mbcsId('media', 2),
	littleOnesArticles: mbcsId('media', 3),
	outdoorShopProducts: mbcsId('media', 4),
	outdoorShopArticles: mbcsId('media', 5),
} as const;

export const BRANDING_MEDIA_IDS = {
	logoLittleOnes: mbcsId('media', 10),
	logoOutdoorShop: mbcsId('media', 11),
	heroLittleOnes: mbcsId('media', 12),
	heroOutdoorShop: mbcsId('media', 13),
} as const;

export const logoMediaId = (site: UmbMbcsSiteCode) =>
	site === 'LO' ? BRANDING_MEDIA_IDS.logoLittleOnes : BRANDING_MEDIA_IDS.logoOutdoorShop;

export const heroMediaId = (site: UmbMbcsSiteCode) =>
	site === 'LO' ? BRANDING_MEDIA_IDS.heroLittleOnes : BRANDING_MEDIA_IDS.heroOutdoorShop;

export const productImageId = (site: UmbMbcsSiteCode, categoryIndex: number) =>
	mbcsId('media', 100 + (site === 'LO' ? 0 : 10) + categoryIndex);

export const articleImageId = (site: UmbMbcsSiteCode, categoryIndex: number) =>
	mbcsId('media', 200 + (site === 'LO' ? 0 : 5) + categoryIndex);

export const productCategoryIndex = (productNumber: number) => Math.floor((productNumber - 1) / PRODUCTS_PER_CATEGORY);

export const articleCategoryIndex = (articleNumber: number) => Math.floor((articleNumber - 1) / ARTICLES_PER_CATEGORY);
