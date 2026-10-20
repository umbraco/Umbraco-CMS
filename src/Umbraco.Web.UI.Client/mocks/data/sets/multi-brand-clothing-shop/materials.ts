import type { UmbMbcsMaterialImageKey, UmbMbcsSiteCode } from './catalog.js';

const IMAGE_BY_MATERIAL: Record<string, UmbMbcsMaterialImageKey> = {
	'Organic cotton': 'red-gold',
	'Cotton jersey': 'red-gold',
	'Cotton rib': 'red-gold',
	'Cotton poplin': 'red-gold',
	'Cotton flannel': 'red-gold',
	'Cotton lining': 'red-gold',
	'Cotton twill': 'red-gold',
	'Cotton corduroy': 'red-gold',
	'Cotton denim': 'red-gold',
	'Cotton muslin': 'red-gold',
	Wool: 'fabric-texture',
	'Merino wool': 'fabric-texture',
	Lambswool: 'fabric-texture',
	Cashmere: 'fabric-texture',
	'Alpaca blend': 'fabric-texture',
	'Wool blend': 'fabric-texture',
	'Wool felt': 'fabric-texture',
	Linen: 'linen-fabric',
	'Cotton canvas': 'linen-fabric',
	'Waxed cotton': 'linen-fabric',
	'Recycled polyester': 'dark-geometric',
	'Recycled polyester softshell': 'dark-geometric',
	'Recycled down fill': 'dark-geometric',
	Elastane: 'dark-geometric',
	Leather: 'dark-geometric',
	Suede: 'dark-geometric',
	'Rubber sole': 'dark-geometric',
	'Natural rubber': 'dark-geometric',
	'Textile lining': 'dark-geometric',
	'Waterproof membrane': 'water-droplets',
	'Water-repellent finish': 'water-droplets',
	Fleece: 'paper-pastel',
	Velour: 'paper-pastel',
	Tulle: 'paper-pastel',
	'Bamboo viscose': 'paper-pastel',
	Viscose: 'paper-pastel',
	Silk: 'shimmering-red',
	Satin: 'shimmering-red',
};

const MATERIAL_BY_TAG: Record<string, string> = {
	'organic cotton': 'Organic cotton',
	merino: 'Merino wool',
	cashmere: 'Cashmere',
	alpaca: 'Alpaca blend',
	wool: 'Wool',
	bamboo: 'Bamboo viscose',
	linen: 'Linen',
	denim: 'Cotton denim',
	jeans: 'Cotton denim',
	leather: 'Leather',
	suede: 'Suede',
	fleece: 'Fleece',
	thermal: 'Fleece',
	silk: 'Silk',
	corduroy: 'Cotton corduroy',
	velour: 'Velour',
	muslin: 'Cotton muslin',
	down: 'Recycled down fill',
	'waxed cotton': 'Waxed cotton',
	softshell: 'Recycled polyester softshell',
	canvas: 'Cotton canvas',
	rain: 'Waterproof membrane',
	rainwear: 'Waterproof membrane',
	wellies: 'Natural rubber',
	slippers: 'Wool felt',
	leggings: 'Elastane',
	party: 'Satin',
};

const POOLS: Record<UmbMbcsSiteCode, Array<Array<string>>> = {
	LO: [
		['Organic cotton', 'Cotton rib', 'Elastane'],
		['Cotton jersey', 'Organic cotton', 'Elastane'],
		['Merino wool', 'Organic cotton', 'Wool blend'],
		['Cotton twill', 'Elastane', 'Cotton jersey'],
		['Cotton jersey', 'Tulle', 'Cotton lining', 'Elastane'],
		['Recycled polyester', 'Fleece', 'Recycled down fill', 'Water-repellent finish'],
		['Waterproof membrane', 'Recycled polyester', 'Fleece', 'Water-repellent finish'],
		['Organic cotton', 'Bamboo viscose', 'Cotton rib', 'Fleece'],
		['Leather', 'Rubber sole', 'Textile lining', 'Wool felt'],
		['Merino wool', 'Wool blend', 'Fleece', 'Organic cotton'],
	],
	GU: [
		['Cotton jersey', 'Organic cotton', 'Elastane', 'Cotton rib'],
		['Cotton poplin', 'Linen', 'Silk', 'Cotton lining'],
		['Merino wool', 'Cashmere', 'Lambswool', 'Alpaca blend'],
		['Cotton twill', 'Elastane', 'Linen', 'Wool'],
		['Viscose', 'Linen', 'Cotton jersey', 'Satin'],
		['Wool', 'Recycled down fill', 'Cotton lining', 'Waxed cotton'],
		['Recycled polyester', 'Waterproof membrane', 'Merino wool', 'Fleece'],
		['Organic cotton', 'Cotton flannel', 'Silk', 'Wool'],
		['Leather', 'Rubber sole', 'Suede', 'Textile lining'],
		['Cotton canvas', 'Leather', 'Recycled polyester', 'Textile lining'],
	],
};

export const getMaterial = (name: string): UmbMbcsMaterial => ({ name, imageKey: IMAGE_BY_MATERIAL[name] });

const MAX_MATERIALS = 4;
const SPECIFIC_WOOLS = ['Merino wool', 'Lambswool', 'Cashmere', 'Alpaca blend'];
const GENERIC_WOOLS = ['Wool', 'Wool blend'];

export interface UmbMbcsMaterial {
	name: string;
	imageKey: UmbMbcsMaterialImageKey;
}

/** Between one and four materials: the ones the tags point at first, then the typical ones for the category. */
export const getProductMaterials = (
	site: UmbMbcsSiteCode,
	categoryIndex: number,
	tags: Array<string>,
	productNumber: number,
): Array<UmbMbcsMaterial> => {
	const fromTags = tags.map((tag) => MATERIAL_BY_TAG[tag.toLowerCase()]).filter((name): name is string => !!name);
	const count = 1 + ((productNumber + categoryIndex) % MAX_MATERIALS);

	const candidates = [...new Set([...fromTags, ...POOLS[site][categoryIndex]])];
	const hasSpecificWool = candidates.some((name) => SPECIFIC_WOOLS.includes(name));

	return candidates
		.filter((name) => !(hasSpecificWool && GENERIC_WOOLS.includes(name)))
		.slice(0, count)
		.map((name) => ({ name, imageKey: IMAGE_BY_MATERIAL[name] }));
};
