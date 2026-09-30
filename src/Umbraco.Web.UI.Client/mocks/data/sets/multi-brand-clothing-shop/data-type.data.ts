import type { UmbMockDataTypeModel } from '../../mock-data-set.types.js';
import { AREA_KEYS, DATA_TYPE_IDS, DOCUMENT_TYPE_IDS, MEDIA_TYPE_IDS } from './ids.js';

type UmbMbcsDataTypeValues = UmbMockDataTypeModel['values'];

const dataType = (
	id: string,
	name: string,
	editorAlias: string,
	editorUiAlias: string,
	values: UmbMbcsDataTypeValues = [],
): UmbMockDataTypeModel => ({
	id,
	parent: null,
	name,
	editorAlias,
	editorUiAlias,
	hasChildren: false,
	isFolder: false,
	isDeletable: true,
	canIgnoreStartNodes: false,
	flags: [],
	values,
});

const nodePicker = (id: string, name: string, documentTypeId: string, maxNumber: number) =>
	dataType(id, name, 'Umbraco.MultiNodeTreePicker', 'Umb.PropertyEditorUi.ContentPicker', [
		{ alias: 'startNode', value: { type: 'content', id: null, dynamicRoot: null } },
		{ alias: 'filter', value: documentTypeId },
		{ alias: 'minNumber', value: 0 },
		{ alias: 'maxNumber', value: maxNumber },
		{ alias: 'showOpenButton', value: true },
		{ alias: 'ignoreUserStartNodes', value: false },
	]);

const collection = (
	id: string,
	name: string,
	options: {
		pageSize: number;
		orderBy: string;
		orderDirection: 'asc' | 'desc';
		columns: Array<{ alias: string; header: string; isSystem: boolean }>;
		layouts: Array<'table' | 'grid'>;
		tabName: string;
	},
) =>
	dataType(id, name, 'Umbraco.ListView', 'Umb.PropertyEditorUi.Collection', [
		{ alias: 'pageSize', value: options.pageSize },
		{ alias: 'orderBy', value: options.orderBy },
		{ alias: 'orderDirection', value: options.orderDirection },
		{
			alias: 'includeProperties',
			value: options.columns.map((column) => ({ ...column, nameTemplate: null })),
		},
		{
			alias: 'layouts',
			value: options.layouts.map((layout) =>
				layout === 'table'
					? {
							icon: 'icon-list',
							name: 'Document Table Collection View',
							collectionView: 'Umb.CollectionView.Document.Table',
						}
					: {
							icon: 'icon-grid',
							name: 'Document Grid Collection View',
							collectionView: 'Umb.CollectionView.Document.Grid',
						},
			),
		},
		{ alias: 'icon', value: 'icon-layers' },
		{ alias: 'tabName', value: options.tabName },
		{ alias: 'showContentFirst', value: false },
	]);

interface UmbMbcsBlockOptions {
	elementTypeId: string;
	columnSpans: Array<number>;
	allowAtRoot: boolean;
	allowInAreas: boolean;
	areas?: Array<{ key: string; alias: string; columnSpan: number }>;
	label?: string;
}

const block = (options: UmbMbcsBlockOptions) => ({
	contentElementTypeKey: options.elementTypeId,
	label: options.label ?? '',
	columnSpanOptions: options.columnSpans.map((columnSpan) => ({ columnSpan })),
	rowMinSpan: 1,
	rowMaxSpan: 1,
	allowAtRoot: options.allowAtRoot,
	allowInAreas: options.allowInAreas,
	areas: (options.areas ?? []).map((area) => ({
		...area,
		rowSpan: 1,
		minAllowed: 0,
		specifiedAllowance: [],
	})),
	editorSize: 'medium',
	inlineEditing: false,
	forceHideContentEditorInOverlay: false,
});

const SIZES = [
	'50',
	'56',
	'62',
	'68',
	'74',
	'80',
	'86',
	'92',
	'98',
	'104',
	'110',
	'116',
	'122',
	'128',
	'134',
	'140',
	'146',
	'152',
	'XS',
	'S',
	'M',
	'L',
	'XL',
	'XXL',
];

export const data: Array<UmbMockDataTypeModel> = [
	dataType(DATA_TYPE_IDS.textstring, 'Textstring', 'Umbraco.TextBox', 'Umb.PropertyEditorUi.TextBox'),
	dataType(
		DATA_TYPE_IDS.textstringMax60,
		'Textstring (max 60 characters)',
		'Umbraco.TextBox',
		'Umb.PropertyEditorUi.TextBox',
		[{ alias: 'maxChars', value: 60 }],
	),
	dataType(DATA_TYPE_IDS.textarea, 'Textarea', 'Umbraco.TextArea', 'Umb.PropertyEditorUi.TextArea'),
	dataType(
		DATA_TYPE_IDS.textareaMax160,
		'Textarea (max 160 characters)',
		'Umbraco.TextArea',
		'Umb.PropertyEditorUi.TextArea',
		[{ alias: 'maxChars', value: 160 }],
	),
	dataType(DATA_TYPE_IDS.richTextEditor, 'Richtext editor', 'Umbraco.RichText', 'Umb.PropertyEditorUi.Tiptap', [
		{
			alias: 'toolbar',
			value: [
				[
					[
						'Umb.Tiptap.Toolbar.StyleSelect',
						'Umb.Tiptap.Toolbar.Bold',
						'Umb.Tiptap.Toolbar.Italic',
						'Umb.Tiptap.Toolbar.BulletList',
						'Umb.Tiptap.Toolbar.OrderedList',
						'Umb.Tiptap.Toolbar.Link',
						'Umb.Tiptap.Toolbar.MediaPicker',
					],
				],
			],
		},
		{
			alias: 'extensions',
			value: [
				'Umb.Tiptap.RichTextEssentials',
				'Umb.Tiptap.Figure',
				'Umb.Tiptap.Image',
				'Umb.Tiptap.Link',
				'Umb.Tiptap.MediaUpload',
				'Umb.Tiptap.Blockquote',
				'Umb.Tiptap.Bold',
				'Umb.Tiptap.BulletList',
				'Umb.Tiptap.Heading',
				'Umb.Tiptap.Italic',
				'Umb.Tiptap.OrderedList',
				'Umb.Tiptap.TrailingNode',
			],
		},
		{ alias: 'maxImageSize', value: 500 },
	]),
	dataType(
		DATA_TYPE_IDS.mediaPicker,
		'Media Picker (1 image)',
		'Umbraco.MediaPicker3',
		'Umb.PropertyEditorUi.MediaPicker',
		[
			{ alias: 'filter', value: MEDIA_TYPE_IDS.image },
			{ alias: 'multiple', value: false },
			{ alias: 'validationLimit', value: { min: 0, max: 1 } },
		],
	),
	dataType(DATA_TYPE_IDS.decimal, 'Decimal (step 0.01)', 'Umbraco.Decimal', 'Umb.PropertyEditorUi.Decimal', [
		{ alias: 'step', value: 0.01 },
	]),
	dataType(DATA_TYPE_IDS.toggle, 'Toggle', 'Umbraco.TrueFalse', 'Umb.PropertyEditorUi.Toggle'),
	dataType(DATA_TYPE_IDS.toggleDefaultOn, 'Toggle (default on)', 'Umbraco.TrueFalse', 'Umb.PropertyEditorUi.Toggle', [
		{ alias: 'default', value: true },
	]),
	dataType(DATA_TYPE_IDS.datePicker, 'Date Picker', 'Umbraco.DateTime', 'Umb.PropertyEditorUi.DatePicker', [
		{ alias: 'format', value: 'YYYY-MM-DD' },
	]),
	dataType(DATA_TYPE_IDS.productTags, 'Tags (productTags)', 'Umbraco.Tags', 'Umb.PropertyEditorUi.Tags', [
		{ alias: 'group', value: 'productTags' },
		{ alias: 'storageType', value: 'Json' },
	]),
	dataType(DATA_TYPE_IDS.articleTags, 'Tags (articleTags)', 'Umbraco.Tags', 'Umb.PropertyEditorUi.Tags', [
		{ alias: 'group', value: 'articleTags' },
		{ alias: 'storageType', value: 'Json' },
	]),
	dataType(DATA_TYPE_IDS.sizes, 'Sizes', 'Umbraco.CheckBoxList', 'Umb.PropertyEditorUi.CheckBoxList', [
		{ alias: 'items', value: SIZES },
	]),
	dataType(
		DATA_TYPE_IDS.multiUrlPickerSingle,
		'Multi URL Picker (max 1)',
		'Umbraco.MultiUrlPicker',
		'Umb.PropertyEditorUi.MultiUrlPicker',
		[{ alias: 'maxNumber', value: 1 }],
	),
	nodePicker(DATA_TYPE_IDS.productPicker, 'Product Picker', DOCUMENT_TYPE_IDS.product, 0),
	nodePicker(DATA_TYPE_IDS.productPickerSingle, 'Product Picker (max 1)', DOCUMENT_TYPE_IDS.product, 1),
	nodePicker(DATA_TYPE_IDS.articlePickerSingle, 'Article Picker (max 1)', DOCUMENT_TYPE_IDS.article, 1),
	collection(DATA_TYPE_IDS.productsCollection, 'Products Collection', {
		pageSize: 25,
		orderBy: 'name',
		orderDirection: 'asc',
		columns: [
			{ alias: 'price', header: 'Price', isSystem: false },
			{ alias: 'tags', header: 'Tags', isSystem: false },
			{ alias: 'inStock', header: 'In stock', isSystem: false },
			{ alias: 'updateDate', header: 'Last edited', isSystem: true },
		],
		layouts: ['table', 'grid'],
		tabName: 'Products',
	}),
	collection(DATA_TYPE_IDS.articlesCollection, 'Articles Collection', {
		pageSize: 20,
		orderBy: 'publishDate',
		orderDirection: 'desc',
		columns: [
			{ alias: 'author', header: 'Author', isSystem: false },
			{ alias: 'publishDate', header: 'Publish date', isSystem: false },
			{ alias: 'tags', header: 'Tags', isSystem: false },
		],
		layouts: ['table'],
		tabName: 'Articles',
	}),
	collection(DATA_TYPE_IDS.storesCollection, 'Stores Collection', {
		pageSize: 10,
		orderBy: 'name',
		orderDirection: 'asc',
		columns: [
			{ alias: 'address', header: 'Address', isSystem: false },
			{ alias: 'phone', header: 'Phone', isSystem: false },
			{ alias: 'updateDate', header: 'Last edited', isSystem: true },
		],
		layouts: ['table', 'grid'],
		tabName: 'Stores',
	}),
	dataType(
		DATA_TYPE_IDS.pageContentBlockGrid,
		'Page Content – Block Grid',
		'Umbraco.BlockGrid',
		'Umb.PropertyEditorUi.BlockGrid',
		[
			{
				alias: 'blocks',
				value: [
					block({
						elementTypeId: DOCUMENT_TYPE_IDS.oneColumnLayout,
						columnSpans: [12],
						allowAtRoot: true,
						allowInAreas: false,
						areas: [{ key: AREA_KEYS.oneColumnMain, alias: 'main', columnSpan: 12 }],
					}),
					block({
						elementTypeId: DOCUMENT_TYPE_IDS.twoColumnLayout,
						columnSpans: [12],
						allowAtRoot: true,
						allowInAreas: false,
						areas: [
							{ key: AREA_KEYS.twoColumnLeft, alias: 'left', columnSpan: 6 },
							{ key: AREA_KEYS.twoColumnRight, alias: 'right', columnSpan: 6 },
						],
					}),
					block({
						elementTypeId: DOCUMENT_TYPE_IDS.heroBlock,
						columnSpans: [12],
						allowAtRoot: true,
						allowInAreas: false,
						label: '{=headline}',
					}),
					block({
						elementTypeId: DOCUMENT_TYPE_IDS.imageBlock,
						columnSpans: [6, 12],
						allowAtRoot: false,
						allowInAreas: true,
						label: '{=caption}',
					}),
					block({
						elementTypeId: DOCUMENT_TYPE_IDS.textBlock,
						columnSpans: [6, 12],
						allowAtRoot: false,
						allowInAreas: true,
					}),
					block({
						elementTypeId: DOCUMENT_TYPE_IDS.productTeaserBlock,
						columnSpans: [3, 4, 6],
						allowAtRoot: false,
						allowInAreas: true,
						label: '{=label}',
					}),
					block({
						elementTypeId: DOCUMENT_TYPE_IDS.articleTeaserBlock,
						columnSpans: [3, 4, 6],
						allowAtRoot: false,
						allowInAreas: true,
					}),
				],
			},
			{ alias: 'gridColumns', value: 12 },
			{ alias: 'validationLimit', value: {} },
			{ alias: 'useLiveEditing', value: false },
		],
	),
	dataType(DATA_TYPE_IDS.imageCropper, 'Image Cropper', 'Umbraco.ImageCropper', 'Umb.PropertyEditorUi.ImageCropper', [
		{ alias: 'crops', value: [] },
	]),
];
