import type { UmbMbcsSiteCode } from './catalog.js';

export const PRODUCT_DOCUMENT_CATEGORIES = [
	'Manual',
	'Washing Guide',
	'Care Guide',
	'Size & Fit Guide',
	'Warranty',
	'Safety Information',
	'Certificate',
	'Technical Data Sheet',
	'Repair Guide',
	'Inspirational Brochure',
	'Lookbook',
	'Test Report',
] as const;

export type UmbMbcsProductDocumentCategory = (typeof PRODUCT_DOCUMENT_CATEGORIES)[number];

export interface UmbMbcsProductDocument {
	category: UmbMbcsProductDocumentCategory;
	title: string;
	summary: string;
	languages: Array<string>;
	version: string;
	pageCount: number;
	/** YYYY-MM-DD */
	revisionDate: string;
	showOnProductPage: boolean;
	isDraft: boolean;
}

const SHOES_CATEGORY_INDEX = 8;
const ACCESSORY_CATEGORY_INDEX = 9;
const OUTERWEAR_CATEGORY_INDEX = 5;
const OUTDOOR_CATEGORY_INDEX = 6;

const BROCHURES: Record<UmbMbcsSiteCode, Array<string>> = {
	LO: [
		'Little Explorers – Rainy Day Adventures',
		'The Cosy Autumn Edit',
		'First Year Favourites',
		'Playground Season Lookbook',
		'Summer by the Sea',
		'Little Ones Gift Inspiration',
	],
	GU: [
		'Autumn/Winter 2026 Lookbook',
		'Weekend Outdoors',
		'The Workwear Edit',
		'Slow Sunday Loungewear',
		'The Nordic Wardrobe',
		'Built for Weather',
	],
};

const OPTIONAL_LANGUAGES = ['German', 'Swedish', 'Norwegian'];

interface UmbMbcsDocumentSpec {
	category: UmbMbcsProductDocumentCategory;
	title: string;
	summary: string;
	pages: [min: number, max: number];
	/** Defaults to true. */
	isPublic?: boolean;
	/** Certificates and declarations are issued in English only. */
	englishOnly?: boolean;
}

const hasAny = (tags: Set<string>, ...candidates: Array<string>) => candidates.some((tag) => tags.has(tag));

const careDocument = (tags: Set<string>, categoryIndex: number): UmbMbcsDocumentSpec => {
	if (categoryIndex === SHOES_CATEGORY_INDEX) {
		return hasAny(tags, 'leather', 'suede')
			? {
					category: 'Care Guide',
					title: 'Leather Care Guide',
					summary:
						'How to clean, condition and waterproof leather, so the shoes keep their shape through many seasons.',
					pages: [2, 4],
				}
			: {
					category: 'Care Guide',
					title: 'Shoe Care Guide',
					summary: 'Cleaning, drying and storing advice, including what to do after a wet walk.',
					pages: [2, 3],
				};
	}

	if (hasAny(tags, 'leather', 'suede')) {
		return {
			category: 'Care Guide',
			title: 'Leather Care Guide',
			summary: 'Cleaning and conditioning advice that keeps the leather supple and prevents cracking.',
			pages: [2, 4],
		};
	}

	if (hasAny(tags, 'down')) {
		return {
			category: 'Care Guide',
			title: 'Down Care Guide',
			summary: 'Washing, tumble drying with dryer balls and storing down so it keeps its loft.',
			pages: [2, 4],
		};
	}

	if (hasAny(tags, 'merino', 'wool', 'cashmere', 'alpaca')) {
		return {
			category: 'Washing Guide',
			title: 'Wool Washing Guide',
			summary: 'Hand wash or wool cycle at 30 °C, dry flat, and keep it away from direct heat.',
			pages: [2, 4],
		};
	}

	if (hasAny(tags, 'silk')) {
		return {
			category: 'Washing Guide',
			title: 'Silk Care Guide',
			summary: 'Gentle hand wash, no wringing, and how to press silk without leaving marks.',
			pages: [2, 3],
		};
	}

	if (hasAny(tags, 'denim', 'jeans')) {
		return {
			category: 'Washing Guide',
			title: 'Denim Care Guide',
			summary: 'Wash inside out and rarely, to keep the colour and the fit.',
			pages: [2, 3],
		};
	}

	if (hasAny(tags, 'linen')) {
		return {
			category: 'Washing Guide',
			title: 'Linen Care Guide',
			summary: 'Linen gets softer with every wash. Wash cool, line dry and iron while damp.',
			pages: [2, 3],
		};
	}

	if (hasAny(tags, 'rainwear', 'rain', 'outdoor', 'softshell', 'waxed cotton')) {
		return {
			category: 'Washing Guide',
			title: 'Technical Fabric Washing Guide',
			summary: 'Wash without fabric softener and tumble dry on low to reactivate the water-repellent finish.',
			pages: [2, 4],
		};
	}

	return {
		category: 'Washing Guide',
		title: 'Washing Guide',
		summary:
			'Wash at 40 °C with similar colours, and tumble dry on low. Includes a symbol-by-symbol care label explainer.',
		pages: [2, 4],
	};
};

const warrantyDocument = (site: UmbMbcsSiteCode, categoryIndex: number): UmbMbcsDocumentSpec => {
	if (site === 'LO') {
		return {
			category: 'Warranty',
			title: '2-Year Quality Promise',
			summary: 'If a seam gives way or the fabric fails within two years of normal use, we repair or replace it.',
			pages: [2, 4],
		};
	}

	return categoryIndex === OUTERWEAR_CATEGORY_INDEX || categoryIndex === OUTDOOR_CATEGORY_INDEX
		? {
				category: 'Warranty',
				title: 'Lifetime Guarantee Certificate',
				summary: 'Covers manufacturing faults for the life of the product, with free repairs at any of our stores.',
				pages: [2, 4],
			}
		: {
				category: 'Warranty',
				title: 'Warranty Card',
				summary: 'Two years of cover against manufacturing faults. Keep it together with your receipt.',
				pages: [1, 2],
			};
};

const tagDocuments = (tags: Set<string>): Array<UmbMbcsDocumentSpec> => {
	const documents: Array<UmbMbcsDocumentSpec> = [];

	if (hasAny(tags, 'organic cotton')) {
		documents.push({
			category: 'Certificate',
			title: 'GOTS Organic Certificate',
			summary: 'Proof that the cotton is grown and processed to the Global Organic Textile Standard.',
			pages: [1, 2],
			englishOnly: true,
		});
	}

	if (hasAny(tags, 'down')) {
		documents.push({
			category: 'Certificate',
			title: 'Responsible Down Standard Certificate',
			summary: 'Certifies that the down comes from birds that have not been live-plucked or force-fed.',
			pages: [1, 2],
			englishOnly: true,
		});
	}

	if (hasAny(tags, 'merino', 'wool')) {
		documents.push({
			category: 'Certificate',
			title: 'Mulesing-Free Wool Declaration',
			summary: 'Declaration from the supplier that the wool comes from farms that do not practise mulesing.',
			pages: [1, 2],
			englishOnly: true,
		});
	}

	if (hasAny(tags, 'uv protection')) {
		documents.push({
			category: 'Test Report',
			title: 'UPF 50+ Test Certificate',
			summary: 'Independent laboratory test confirming the sun protection factor of the fabric, also after 40 washes.',
			pages: [2, 4],
			englishOnly: true,
		});
	}

	if (hasAny(tags, 'rainwear', 'rain')) {
		documents.push(
			{
				category: 'Technical Data Sheet',
				title: 'Waterproof Rating Data Sheet (10,000 mm)',
				summary: 'Water column, breathability and seam-taping specifications for the waterproof fabric.',
				pages: [2, 4],
			},
			{
				category: 'Care Guide',
				title: 'Re-proofing Guide',
				summary: 'When and how to restore the water-repellent finish with a wash-in or spray-on treatment.',
				pages: [2, 3],
			},
		);
	}

	if (hasAny(tags, 'waxed cotton')) {
		documents.push({
			category: 'Manual',
			title: 'Re-waxing Manual',
			summary: 'Step-by-step instructions for re-waxing the jacket at home, including how much wax to use.',
			pages: [4, 8],
		});
	}

	if (hasAny(tags, 'cashmere')) {
		documents.push({
			category: 'Care Guide',
			title: 'Pilling & De-pilling Guide',
			summary: 'Why cashmere pills, and how to remove it without damaging the fibres.',
			pages: [2, 3],
		});
	}

	if (hasAny(tags, 'sleeping bags')) {
		documents.push({
			category: 'Safety Information',
			title: 'Safe Sleep Guide & TOG Chart',
			summary: 'Which TOG rating fits which room temperature, and how to dress a child underneath.',
			pages: [4, 6],
		});
	}

	if (hasAny(tags, 'pyjamas', 'sleepwear', 'nightgowns', 'bathrobes')) {
		documents.push({
			category: 'Safety Information',
			title: 'Flammability Declaration',
			summary: 'Declaration of conformity with the flammability requirements for nightwear.',
			pages: [1, 2],
			englishOnly: true,
		});
	}

	if (hasAny(tags, 'baby', 'newborn')) {
		documents.push({
			category: 'Safety Information',
			title: 'Safety Information – Cords & Small Parts (EN 14682)',
			summary: 'Describes how cords, buttons and fasteners are secured, in line with EN 14682.',
			pages: [2, 4],
		});
	}

	if (hasAny(tags, 'snowsuits', 'overalls')) {
		documents.push({
			category: 'Manual',
			title: 'Snowsuit Fitting Guide',
			summary: 'How to size, layer and adjust the suit so it stays warm and allows free movement.',
			pages: [4, 8],
		});
	}

	if (hasAny(tags, 'hiking')) {
		documents.push({
			category: 'Repair Guide',
			title: 'Resoling Service Guide',
			summary: 'When the sole is worn, we can resole the boots. This explains how to send them in.',
			pages: [2, 4],
		});
	}

	if (hasAny(tags, 'running')) {
		documents.push({
			category: 'Manual',
			title: 'Running Shoe Mileage Guide',
			summary: 'How many kilometres to expect from the cushioning, and the signs that it is time for a new pair.',
			pages: [4, 8],
		});
	}

	if (hasAny(tags, 'backpacks')) {
		documents.push({
			category: 'Manual',
			title: 'Backpack Fit & Packing Manual',
			summary: 'Adjusting the straps for your back length, packing the weight right, and closing the roll-top.',
			pages: [8, 16],
		});
	}

	if (hasAny(tags, 'travel')) {
		documents.push({
			category: 'Manual',
			title: 'Travel Packing Manual',
			summary: 'Packing tips for a weekend away, with a checklist to print.',
			pages: [6, 12],
		});
	}

	if (hasAny(tags, 'gift set')) {
		documents.push({
			category: 'Inspirational Brochure',
			title: 'Gift Note Card',
			summary: 'A printable greeting card to go with the gift set.',
			pages: [1, 2],
		});
	}

	if (hasAny(tags, 'christmas')) {
		documents.push({
			category: 'Lookbook',
			title: 'Family Pyjama Lookbook',
			summary: 'Matching Christmas pyjamas for the whole family, with ideas for the morning photo.',
			pages: [16, 24],
		});
	}

	return documents;
};

const sizeGuide = (site: UmbMbcsSiteCode, tags: Set<string>): UmbMbcsDocumentSpec => {
	if (site === 'LO') {
		const group = hasAny(tags, 'baby', 'newborn') ? 'Baby' : hasAny(tags, 'toddler') ? 'Toddler' : 'Kids';
		return {
			category: 'Size & Fit Guide',
			title: `${group} Size Chart`,
			summary: 'Height, chest and waist measurements per size, plus tips on sizing up for growth.',
			pages: [2, 4],
		};
	}

	const group = hasAny(tags, 'women') ? "Women's" : hasAny(tags, 'men') ? "Men's" : 'Unisex';
	return {
		category: 'Size & Fit Guide',
		title: `${group} Size Chart`,
		summary: 'Measurements per size and how this cut fits compared with our other styles.',
		pages: [2, 4],
	};
};

const categoryDocuments = (
	site: UmbMbcsSiteCode,
	categoryIndex: number,
	tags: Set<string>,
	number: number,
): Array<UmbMbcsDocumentSpec> => {
	const documents: Array<UmbMbcsDocumentSpec> = [];

	if (categoryIndex === SHOES_CATEGORY_INDEX) {
		documents.push({
			category: 'Size & Fit Guide',
			title: 'Printable Foot Measuring Template',
			summary: 'Print at 100 %, stand on the sheet and find the right size in minutes.',
			pages: [1, 2],
		});
	} else if (categoryIndex !== ACCESSORY_CATEGORY_INDEX && number % 3 === 1) {
		documents.push(sizeGuide(site, tags));
	}

	if (categoryIndex === OUTERWEAR_CATEGORY_INDEX || categoryIndex === OUTDOOR_CATEGORY_INDEX) {
		documents.push({
			category: 'Technical Data Sheet',
			title: 'Technical Data Sheet',
			summary: 'Fabric weights, insulation values, temperature range and seam construction at a glance.',
			pages: [2, 6],
		});

		documents.push(
			site === 'LO'
				? {
						category: 'Manual',
						title: 'Layering Guide for Little Explorers',
						summary: 'What to put on under the outer layer at different temperatures, from 10 °C down to −10 °C.',
						pages: [4, 8],
					}
				: number % 2 === 0
					? {
							category: 'Repair Guide',
							title: 'Field Repair Manual',
							summary: 'Fix a torn seam, a broken zip or a punctured membrane on the trail.',
							pages: [8, 16],
						}
					: {
							category: 'Manual',
							title: 'Layering System Guide',
							summary: 'Base layer, mid layer, shell: combine them for the weather you are heading into.',
							pages: [6, 12],
						},
		);
	}

	return documents;
};

/** Deterministic spread over the document metadata, so collection columns show a realistic mix. */
const finalize = (
	spec: UmbMbcsDocumentSpec,
	productName: string,
	number: number,
	index: number,
	prefixWithProduct: boolean,
): UmbMbcsProductDocument => {
	const seed = number + index * 3;
	const [minPages, maxPages] = spec.pages;
	const monthsFromStart = (number * 7 + index * 5) % 20;
	const day = 1 + ((number * 3 + index) % 27);
	const revision = new Date(Date.UTC(2025, monthsFromStart, day));
	const languages = spec.englishOnly ? ['English'] : ['English', 'Danish'];

	if (!spec.englishOnly) {
		OPTIONAL_LANGUAGES.forEach((language, languageIndex) => {
			if (seed % [3, 5, 7][languageIndex] === 0) languages.push(language);
		});
	}

	return {
		category: spec.category,
		title: prefixWithProduct ? `${productName} – ${spec.title}` : spec.title,
		summary: spec.summary,
		languages,
		version: `v${1 + (seed % 3)}.${(number * (index + 1)) % 4}`,
		pageCount: minPages + (seed % (maxPages - minPages + 1)),
		revisionDate: revision.toISOString().slice(0, 10),
		showOnProductPage: spec.isPublic ?? true,
		isDraft: seed % 13 === 0,
	};
};

/**
 * Between three and six documents per product: the ones every product has (care and warranty), the ones its tags
 * point at, and the ones typical for its category.
 */
export const getProductDocuments = (
	site: UmbMbcsSiteCode,
	categoryIndex: number,
	tagList: Array<string>,
	productName: string,
	number: number,
): Array<UmbMbcsProductDocument> => {
	const tags = new Set(tagList.map((tag) => tag.toLowerCase()));

	const specs: Array<UmbMbcsDocumentSpec> = [
		careDocument(tags, categoryIndex),
		warrantyDocument(site, categoryIndex),
		...tagDocuments(tags),
		...categoryDocuments(site, categoryIndex, tags, number),
	];

	if (number % 3 === 0) {
		const brochure = BROCHURES[site][(number / 3) % BROCHURES[site].length];
		specs.push({
			category: number % 2 === 0 ? 'Lookbook' : 'Inspirational Brochure',
			title: brochure,
			summary: `${brochure}: styling ideas and outfit inspiration built around this product.`,
			pages: [16, 48],
		});
	}

	if (number % 4 === 2) {
		specs.push({
			category: 'Test Report',
			title: 'Supplier Lab Test Report',
			summary: 'Internal: colour fastness, shrinkage and seam strength results from the supplier laboratory.',
			pages: [8, 20],
			isPublic: false,
			englishOnly: true,
		});
	}

	if (number % 5 === 0) {
		specs.push({
			category: 'Technical Data Sheet',
			title: 'Care Label Specification',
			summary: 'Internal: the exact wording and symbols for the care label, as approved by the supplier.',
			pages: [1, 2],
			isPublic: false,
			englishOnly: true,
		});
	}

	const seen = new Set<string>();

	return specs
		.filter((spec) => !seen.has(spec.title) && !!seen.add(spec.title))
		.map((spec, index) => finalize(spec, productName, number, index, !BROCHURES[site].includes(spec.title)));
};
