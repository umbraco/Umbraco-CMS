import { UmbElementVariantState } from '@umbraco-cms/backoffice/element';
import type { UmbMockElementModel } from '../../mock-data-set.types.js';
import { articleImageId, heroMediaId, productImageId } from './catalog.js';
import {
	documentLinkValue,
	escapeMarkup,
	invariantTextValue,
	invariantTextareaValue,
	mediaPickerValue,
	richTextValue,
	textValue,
	textareaValue,
	type UmbMbcsValue,
} from './document-values.js';
import { CULTURES, CULTURE_DA, CULTURE_EN, DOCUMENT_TYPE_IDS, ELEMENT_FOLDER_IDS, ELEMENT_IDS, mbcsId } from './ids.js';

const CREATE_DATE = '2026-02-10T09:00:00.000Z';
const UPDATE_DATE = '2026-08-20T09:00:00.000Z';

const LITTLE_ONES_PRODUCTS_ID = mbcsId('document', 3);
const OUTDOOR_SHOP_PRODUCTS_ID = mbcsId('document', 4);
const OUTDOOR_SHOP_FIRST_PRODUCT_ID = mbcsId('document', 20001);

const ICONS = {
	promoBanner: 'icon-megaphone',
	sizeGuide: 'icon-ruler',
	heroBlock: 'icon-picture',
	textBlock: 'icon-font',
} as const;

type UmbMbcsElementState = (typeof UmbElementVariantState)[keyof typeof UmbElementVariantState];

let variantCounter = 0;

const folder = (
	id: string,
	name: string,
	parentId: string | null,
	ancestorIds: Array<string>,
): UmbMockElementModel => ({
	ancestors: ancestorIds.map((ancestorId) => ({ id: ancestorId })),
	id,
	createDate: CREATE_DATE,
	parent: parentId ? { id: parentId } : null,
	documentType: null,
	hasChildren: true,
	isTrashed: false,
	isFolder: true,
	name,
	variants: [],
	values: [],
	flags: [],
	noAccess: false,
});

interface UmbMbcsElementInit {
	id: string;
	name: string;
	documentTypeId: string;
	icon: string;
	parentId: string;
	ancestorIds: Array<string>;
	values: Array<UmbMbcsValue>;
	/** The state per culture. A single state makes the element invariant. */
	state: UmbMbcsElementState | { [CULTURE_EN]: UmbMbcsElementState; [CULTURE_DA]: UmbMbcsElementState };
	trashed?: boolean;
}

const element = (init: UmbMbcsElementInit): UmbMockElementModel => {
	const state = init.state;
	const cultureStates: Array<[string | null, UmbMbcsElementState]> =
		typeof state === 'string' ? [[null, state]] : CULTURES.map((culture) => [culture, state[culture]]);
	const trashed = init.trashed ?? false;

	return {
		ancestors: trashed ? [] : init.ancestorIds.map((id) => ({ id })),
		id: init.id,
		createDate: CREATE_DATE,
		parent: trashed ? null : { id: init.parentId },
		...(trashed ? { originalParent: { id: init.parentId } } : {}),
		documentType: { id: init.documentTypeId, icon: init.icon },
		hasChildren: false,
		isTrashed: trashed,
		isFolder: false,
		name: init.name,
		variants: cultureStates.map(([culture, variantState]) => ({
			id: mbcsId('variant', 50000 + ++variantCounter),
			flags: [],
			state: trashed ? UmbElementVariantState.TRASHED : variantState,
			culture,
			name: init.name,
			createDate: CREATE_DATE,
			updateDate: UPDATE_DATE,
			publishDate: variantState === UmbElementVariantState.PUBLISHED ? UPDATE_DATE : null,
		})),
		values: init.values,
		flags: [],
		noAccess: false,
	};
};

const { PUBLISHED, DRAFT } = UmbElementVariantState;

const SHARED = [ELEMENT_FOLDER_IDS.shared];
const LITTLE_ONES_PROMOTIONS = [ELEMENT_FOLDER_IDS.littleOnes, ELEMENT_FOLDER_IDS.littleOnesPromotions];
const LITTLE_ONES_HEROES = [ELEMENT_FOLDER_IDS.littleOnes, ELEMENT_FOLDER_IDS.littleOnesHeroes];
const OUTDOOR_SHOP_PROMOTIONS = [ELEMENT_FOLDER_IDS.outdoorShop, ELEMENT_FOLDER_IDS.outdoorShopPromotions];
const OUTDOOR_SHOP_HEROES = [ELEMENT_FOLDER_IDS.outdoorShop, ELEMENT_FOLDER_IDS.outdoorShopHeroes];

export const data: Array<UmbMockElementModel> = [
	folder(ELEMENT_FOLDER_IDS.shared, 'Shared', null, []),
	folder(ELEMENT_FOLDER_IDS.littleOnes, 'Little Ones', null, []),
	folder(ELEMENT_FOLDER_IDS.littleOnesPromotions, 'Promotions', ELEMENT_FOLDER_IDS.littleOnes, [
		ELEMENT_FOLDER_IDS.littleOnes,
	]),
	folder(ELEMENT_FOLDER_IDS.littleOnesHeroes, 'Heroes', ELEMENT_FOLDER_IDS.littleOnes, [ELEMENT_FOLDER_IDS.littleOnes]),
	folder(ELEMENT_FOLDER_IDS.outdoorShop, 'The Outdoor Shop', null, []),
	folder(ELEMENT_FOLDER_IDS.outdoorShopPromotions, 'Promotions', ELEMENT_FOLDER_IDS.outdoorShop, [
		ELEMENT_FOLDER_IDS.outdoorShop,
	]),
	folder(ELEMENT_FOLDER_IDS.outdoorShopHeroes, 'Heroes', ELEMENT_FOLDER_IDS.outdoorShop, [
		ELEMENT_FOLDER_IDS.outdoorShop,
	]),

	element({
		id: ELEMENT_IDS.sizeGuideKids,
		name: 'Size guide: Kids',
		documentTypeId: DOCUMENT_TYPE_IDS.sizeGuide,
		icon: ICONS.sizeGuide,
		parentId: ELEMENT_FOLDER_IDS.shared,
		ancestorIds: SHARED,
		state: PUBLISHED,
		values: [
			...invariantTextValue('title', 'Kids size guide'),
			...mediaPickerValue('chart', productImageId('LO', 0)),
			...invariantTextareaValue('notes', 'Sizes run small. If your child is between sizes, choose the larger one.'),
		],
	}),
	element({
		id: ELEMENT_IDS.sizeGuideOutdoor,
		name: 'Size guide: Outdoor',
		documentTypeId: DOCUMENT_TYPE_IDS.sizeGuide,
		icon: ICONS.sizeGuide,
		parentId: ELEMENT_FOLDER_IDS.shared,
		ancestorIds: SHARED,
		state: PUBLISHED,
		values: [
			...invariantTextValue('title', 'Outdoor size guide'),
			...mediaPickerValue('chart', productImageId('GU', 0)),
			...invariantTextareaValue('notes', 'Cut for layering. Choose your usual size, or one up for a looser fit.'),
		],
	}),
	element({
		id: ELEMENT_IDS.deliveryAndReturns,
		name: 'Delivery & returns',
		documentTypeId: DOCUMENT_TYPE_IDS.textBlock,
		icon: ICONS.textBlock,
		parentId: ELEMENT_FOLDER_IDS.shared,
		ancestorIds: SHARED,
		state: { [CULTURE_EN]: PUBLISHED, [CULTURE_DA]: PUBLISHED },
		values: [
			...richTextValue(
				'text',
				`<h3>${escapeMarkup('Delivery & returns')}</h3><p>Free delivery on orders over 500 DKK. Return unworn items within 30 days.</p>`,
			),
		],
	}),

	element({
		id: ELEMENT_IDS.backToSchool,
		name: 'Back to school',
		documentTypeId: DOCUMENT_TYPE_IDS.promoBanner,
		icon: ICONS.promoBanner,
		parentId: ELEMENT_FOLDER_IDS.littleOnesPromotions,
		ancestorIds: LITTLE_ONES_PROMOTIONS,
		state: { [CULTURE_EN]: PUBLISHED, [CULTURE_DA]: PUBLISHED },
		values: [
			...textValue('headline', 'Back to school'),
			...textareaValue('body', 'Everything for the new term, from rain jackets to indoor shoes.'),
			...mediaPickerValue('image', articleImageId('LO', 0)),
			...documentLinkValue('link', LITTLE_ONES_PRODUCTS_ID, 'Shop the range'),
		],
	}),
	element({
		id: ELEMENT_IDS.winterSale,
		name: 'Winter sale',
		documentTypeId: DOCUMENT_TYPE_IDS.promoBanner,
		icon: ICONS.promoBanner,
		parentId: ELEMENT_FOLDER_IDS.littleOnesPromotions,
		ancestorIds: LITTLE_ONES_PROMOTIONS,
		state: { [CULTURE_EN]: PUBLISHED, [CULTURE_DA]: DRAFT },
		values: [
			...textValue('headline', 'Winter sale'),
			...textareaValue('body', 'Up to 40% off warm layers while stocks last.'),
			...mediaPickerValue('image', productImageId('LO', 1)),
		],
	}),
	element({
		id: ELEMENT_IDS.summerClearance,
		name: 'Summer clearance 2025',
		documentTypeId: DOCUMENT_TYPE_IDS.promoBanner,
		icon: ICONS.promoBanner,
		parentId: ELEMENT_FOLDER_IDS.littleOnesPromotions,
		ancestorIds: LITTLE_ONES_PROMOTIONS,
		state: { [CULTURE_EN]: PUBLISHED, [CULTURE_DA]: PUBLISHED },
		trashed: true,
		values: [
			...textValue('headline', 'Summer clearance'),
			...textareaValue('body', 'Last chance to save on summer favourites.'),
			...mediaPickerValue('image', productImageId('LO', 2)),
		],
	}),
	element({
		id: ELEMENT_IDS.littleOnesHero,
		name: 'Little Ones seasonal hero',
		documentTypeId: DOCUMENT_TYPE_IDS.heroBlock,
		icon: ICONS.heroBlock,
		parentId: ELEMENT_FOLDER_IDS.littleOnesHeroes,
		ancestorIds: LITTLE_ONES_HEROES,
		state: { [CULTURE_EN]: PUBLISHED, [CULTURE_DA]: PUBLISHED },
		values: [
			...textValue('headline', 'Little Ones'),
			...textareaValue('subheadline', 'Quality clothing, made to last.'),
			...mediaPickerValue('image', heroMediaId('LO')),
			...documentLinkValue('link', LITTLE_ONES_PRODUCTS_ID, 'Shop now'),
		],
	}),

	element({
		id: ELEMENT_IDS.freeDeliveryWeekend,
		name: 'Free delivery weekend',
		documentTypeId: DOCUMENT_TYPE_IDS.promoBanner,
		icon: ICONS.promoBanner,
		parentId: ELEMENT_FOLDER_IDS.outdoorShopPromotions,
		ancestorIds: OUTDOOR_SHOP_PROMOTIONS,
		state: { [CULTURE_EN]: PUBLISHED, [CULTURE_DA]: PUBLISHED },
		values: [
			...textValue('headline', 'Free delivery weekend'),
			...textareaValue('body', 'No minimum spend, all weekend.'),
			...documentLinkValue('link', OUTDOOR_SHOP_PRODUCTS_ID, 'Start shopping'),
		],
	}),
	element({
		id: ELEMENT_IDS.springTrailKit,
		name: 'Spring trail kit',
		documentTypeId: DOCUMENT_TYPE_IDS.promoBanner,
		icon: ICONS.promoBanner,
		parentId: ELEMENT_FOLDER_IDS.outdoorShopPromotions,
		ancestorIds: OUTDOOR_SHOP_PROMOTIONS,
		state: { [CULTURE_EN]: DRAFT, [CULTURE_DA]: DRAFT },
		values: [
			...textValue('headline', 'Spring trail kit'),
			...textareaValue('body', 'Light, packable and ready for muddy paths.'),
			...mediaPickerValue('image', productImageId('GU', 1)),
			...documentLinkValue('link', OUTDOOR_SHOP_FIRST_PRODUCT_ID, 'See the kit'),
		],
	}),
	element({
		id: ELEMENT_IDS.outdoorShopHero,
		name: 'Outdoor seasonal hero',
		documentTypeId: DOCUMENT_TYPE_IDS.heroBlock,
		icon: ICONS.heroBlock,
		parentId: ELEMENT_FOLDER_IDS.outdoorShopHeroes,
		ancestorIds: OUTDOOR_SHOP_HEROES,
		state: { [CULTURE_EN]: PUBLISHED, [CULTURE_DA]: PUBLISHED },
		values: [
			...textValue('headline', 'The Outdoor Shop'),
			...textareaValue('subheadline', 'Gear up for every season.'),
			...mediaPickerValue('image', heroMediaId('GU')),
			...documentLinkValue('link', OUTDOOR_SHOP_PRODUCTS_ID, 'Shop now'),
		],
	}),
];
