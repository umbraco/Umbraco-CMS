const hex = (value: number, length: number) => value.toString(16).padStart(length, '0');

const NAMESPACE = {
	dataType: 1,
	documentType: 2,
	property: 3,
	container: 4,
	mediaType: 5,
	media: 6,
	document: 7,
	variant: 8,
	block: 9,
	area: 10,
	pickerItem: 11,
} as const;

type UmbMbcsNamespace = keyof typeof NAMESPACE;

/** Deterministic GUID, so ids referenced across files stay stable between reloads. */
export const mbcsId = (namespace: UmbMbcsNamespace, index: number): string =>
	`${hex(NAMESPACE[namespace], 8)}-0000-4000-8000-${hex(index, 12)}`;

/** The `umb://element/...` form used by block layouts. */
export const mbcsElementUdi = (key: string): string => `umb://element/${key.replace(/-/g, '')}`;

export const CULTURE_EN = 'en-US';
export const CULTURE_DA = 'da-DK';
export const CULTURES = [CULTURE_EN, CULTURE_DA] as const;

export const DATA_TYPE_IDS = {
	textstring: mbcsId('dataType', 1),
	textstringMax60: mbcsId('dataType', 2),
	textarea: mbcsId('dataType', 3),
	textareaMax160: mbcsId('dataType', 4),
	richTextEditor: mbcsId('dataType', 5),
	mediaPicker: mbcsId('dataType', 6),
	decimal: mbcsId('dataType', 7),
	toggle: mbcsId('dataType', 8),
	toggleDefaultOn: mbcsId('dataType', 9),
	datePicker: mbcsId('dataType', 10),
	productTags: mbcsId('dataType', 11),
	articleTags: mbcsId('dataType', 12),
	sizes: mbcsId('dataType', 13),
	multiUrlPickerSingle: mbcsId('dataType', 14),
	productPicker: mbcsId('dataType', 15),
	productPickerSingle: mbcsId('dataType', 16),
	articlePickerSingle: mbcsId('dataType', 17),
	productsCollection: mbcsId('dataType', 18),
	articlesCollection: mbcsId('dataType', 19),
	storesCollection: mbcsId('dataType', 22),
	pageContentBlockGrid: mbcsId('dataType', 20),
	imageCropper: mbcsId('dataType', 21),
	articleRichTextEditor: mbcsId('dataType', 23),
	productMaterialsBlockList: mbcsId('dataType', 24),
} as const;

export const DOCUMENT_TYPE_IDS = {
	blocksFolder: mbcsId('documentType', 1),
	siteRoot: mbcsId('documentType', 2),
	contentPage: mbcsId('documentType', 3),
	products: mbcsId('documentType', 4),
	product: mbcsId('documentType', 5),
	articles: mbcsId('documentType', 6),
	article: mbcsId('documentType', 7),
	seoComposition: mbcsId('documentType', 8),
	pageContentComposition: mbcsId('documentType', 9),
	stores: mbcsId('documentType', 17),
	store: mbcsId('documentType', 18),
	productMaterial: mbcsId('documentType', 19),
	oneColumnLayout: mbcsId('documentType', 10),
	twoColumnLayout: mbcsId('documentType', 11),
	heroBlock: mbcsId('documentType', 12),
	imageBlock: mbcsId('documentType', 13),
	textBlock: mbcsId('documentType', 14),
	productTeaserBlock: mbcsId('documentType', 15),
	articleTeaserBlock: mbcsId('documentType', 16),
} as const;

export const MEDIA_TYPE_IDS = {
	folder: mbcsId('mediaType', 1),
	image: mbcsId('mediaType', 2),
} as const;

export const AREA_KEYS = {
	oneColumnMain: mbcsId('area', 1),
	twoColumnLeft: mbcsId('area', 2),
	twoColumnRight: mbcsId('area', 3),
} as const;
