import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import { CULTURES, mbcsElementUdi, mbcsId } from './ids.js';

export type UmbMbcsValue = UmbMockDocumentModel['values'][number];

let pickerItemCounter = 0;
let blockCounter = 0;

const invariant = (editorAlias: string, alias: string, value: unknown): Array<UmbMbcsValue> => [
	{ editorAlias, alias, culture: null, segment: null, value },
];

const varying = (editorAlias: string, alias: string, value: unknown): Array<UmbMbcsValue> =>
	CULTURES.map((culture) => ({ editorAlias, alias, culture, segment: null, value }));

export const textValue = (alias: string, value: string) => varying('Umbraco.TextBox', alias, value);

export const textareaValue = (alias: string, value: string) => varying('Umbraco.TextArea', alias, value);

export const escapeMarkup = (text: string) => text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

export const rteInlineBlockTag = (key: string) =>
	`<umb-rte-block-inline data-content-key="${key}"></umb-rte-block-inline>`;

/**
 * @param alias The property alias.
 * @param markup The markup, or a function that receives the keys of the given blocks so it can place them.
 * @param blocks Blocks to embed in the rich text.
 */
export const richTextValue = (
	alias: string,
	markup: string | ((blockKeys: Array<string>) => string),
	blocks: Array<UmbMbcsBlock> = [],
) => {
	const instances = blocks.map(withKeys);

	return varying('Umbraco.RichText', alias, {
		markup: typeof markup === 'function' ? markup(instances.map((block) => block.key)) : markup,
		blocks: {
			layout: instances.length
				? {
						'Umbraco.RichText': instances.map((block) => ({
							contentUdi: mbcsElementUdi(block.key),
							settingsUdi: null,
							contentKey: block.key,
							settingsKey: null,
						})),
					}
				: {},
			contentData: instances.map(toContentData),
			settingsData: [],
			expose: instances.flatMap(toExposeEntries),
		},
	});
};

export const invariantTextValue = (alias: string, value: string) => invariant('Umbraco.TextBox', alias, value);

export const toggleValue = (alias: string, value: boolean) => invariant('Umbraco.TrueFalse', alias, value);

export const decimalValue = (alias: string, value: number) => invariant('Umbraco.Decimal', alias, value);

export const dateValue = (alias: string, value: string) => invariant('Umbraco.DateTime', alias, value);

export const tagsValue = (alias: string, tags: Array<string>) => invariant('Umbraco.Tags', alias, tags);

export const checkboxListValue = (alias: string, items: Array<string>) =>
	invariant('Umbraco.CheckBoxList', alias, items);

export const documentPickerValue = (alias: string, documentIds: Array<string>) =>
	invariant(
		'Umbraco.MultiNodeTreePicker',
		alias,
		documentIds.map((unique) => ({ type: 'document', unique })),
	);

export const mediaPickerValue = (alias: string, mediaId: string) =>
	invariant('Umbraco.MediaPicker3', alias, [
		{
			key: mbcsId('pickerItem', ++pickerItemCounter),
			mediaKey: mediaId,
			mediaTypeAlias: '',
			crops: [],
			focalPoint: null,
		},
	]);

export const documentLinkValue = (alias: string, documentId: string, name: string) =>
	invariant('Umbraco.MultiUrlPicker', alias, [
		{ type: 'document', unique: documentId, name, target: null, queryString: null },
	]);

type UmbMbcsBlockPropertyValue = {
	alias: string;
	editorAlias: string;
	value: unknown;
	varies?: boolean;
};

export interface UmbMbcsBlock {
	elementTypeId: string;
	columnSpan: number;
	values?: Array<UmbMbcsBlockPropertyValue>;
	areas?: Array<{ key: string; items: Array<UmbMbcsBlock> }>;
}

interface UmbMbcsBlockInstance extends UmbMbcsBlock {
	key: string;
	areas?: Array<{ key: string; items: Array<UmbMbcsBlockInstance> }>;
}

const withKeys = (block: UmbMbcsBlock): UmbMbcsBlockInstance => ({
	...block,
	key: mbcsId('block', ++blockCounter),
	areas: block.areas?.map((area) => ({ key: area.key, items: area.items.map(withKeys) })),
});

const toLayoutItem = (block: UmbMbcsBlockInstance): Record<string, unknown> => ({
	columnSpan: block.columnSpan,
	rowSpan: 1,
	areas: (block.areas ?? []).map((area) => ({ key: area.key, items: area.items.map(toLayoutItem) })),
	contentUdi: mbcsElementUdi(block.key),
	settingsUdi: null,
	contentKey: block.key,
	settingsKey: null,
});

const flatten = (block: UmbMbcsBlockInstance): Array<UmbMbcsBlockInstance> => [
	block,
	...(block.areas ?? []).flatMap((area) => area.items.flatMap(flatten)),
];

/**
 * The Block Grid itself is invariant; text inside the blocks varies through the element types (block-level variation),
 * so every block is exposed in each culture.
 */
const toContentData = (block: UmbMbcsBlockInstance) => ({
	contentTypeKey: block.elementTypeId,
	key: block.key,
	values: (block.values ?? []).flatMap(
		(property): Array<UmbMbcsValue> =>
			property.varies
				? CULTURES.map((culture) => ({
						editorAlias: property.editorAlias,
						alias: property.alias,
						culture,
						segment: null,
						value: property.value,
					}))
				: [
						{
							editorAlias: property.editorAlias,
							alias: property.alias,
							culture: null,
							segment: null,
							value: property.value,
						},
					],
	),
});

const toExposeEntries = (block: UmbMbcsBlockInstance) =>
	CULTURES.map((culture) => ({ contentKey: block.key, culture, segment: null }));

export const blockGridValue = (alias: string, blocks: Array<UmbMbcsBlock>): Array<UmbMbcsValue> => {
	const instances = blocks.map(withKeys);
	const all = instances.flatMap(flatten);

	return invariant('Umbraco.BlockGrid', alias, {
		contentData: all.map(toContentData),
		settingsData: [],
		expose: all.flatMap(toExposeEntries),
		layout: { 'Umbraco.BlockGrid': instances.map(toLayoutItem) },
	});
};

const toBlockListValue = (blocks: Array<UmbMbcsBlock>) => {
	const instances = blocks.map(withKeys);

	return {
		contentData: instances.map(toContentData),
		settingsData: [],
		expose: instances.flatMap(toExposeEntries),
		layout: {
			'Umbraco.BlockList': instances.map((block) => ({
				contentUdi: mbcsElementUdi(block.key),
				settingsUdi: null,
				contentKey: block.key,
				settingsKey: null,
			})),
		},
	};
};

export const blockListValue = (alias: string, blocks: Array<UmbMbcsBlock>): Array<UmbMbcsValue> =>
	invariant('Umbraco.BlockList', alias, toBlockListValue(blocks));

/** A Block List nested as a property of another block. */
export const blockBlockList = (alias: string, blocks: Array<UmbMbcsBlock>): UmbMbcsBlockPropertyValue => ({
	alias,
	editorAlias: 'Umbraco.BlockList',
	value: toBlockListValue(blocks),
});

export const blockText = (alias: string, value: string): UmbMbcsBlockPropertyValue => ({
	alias,
	editorAlias: 'Umbraco.TextBox',
	value,
	varies: true,
});

export const blockTextarea = (alias: string, value: string): UmbMbcsBlockPropertyValue => ({
	alias,
	editorAlias: 'Umbraco.TextArea',
	value,
	varies: true,
});

export const blockRichText = (alias: string, markup: string): UmbMbcsBlockPropertyValue => ({
	alias,
	editorAlias: 'Umbraco.RichText',
	value: { markup, blocks: { layout: {}, contentData: [], settingsData: [], expose: [] } },
	varies: true,
});

export const blockMediaPicker = (alias: string, mediaId: string): UmbMbcsBlockPropertyValue => ({
	alias,
	editorAlias: 'Umbraco.MediaPicker3',
	value: [
		{
			key: mbcsId('pickerItem', ++pickerItemCounter),
			mediaKey: mediaId,
			mediaTypeAlias: '',
			crops: [],
			focalPoint: null,
		},
	],
});

export const blockDocumentPicker = (alias: string, documentId: string): UmbMbcsBlockPropertyValue => ({
	alias,
	editorAlias: 'Umbraco.MultiNodeTreePicker',
	value: [{ type: 'document', unique: documentId }],
});

export const blockDocumentLink = (alias: string, documentId: string, name: string): UmbMbcsBlockPropertyValue => ({
	alias,
	editorAlias: 'Umbraco.MultiUrlPicker',
	value: [{ type: 'document', unique: documentId, name, target: null, queryString: null }],
});
