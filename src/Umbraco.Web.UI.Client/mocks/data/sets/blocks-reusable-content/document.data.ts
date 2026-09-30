import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import type { DocumentVariantResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

const blockGridDocument: UmbMockDocumentModel = {
	ancestors: [],
	template: null,
	id: '17cd53f2-93b3-4e34-ade2-916e7a6639ed',
	createDate: '2023-04-19 09:00:36',
	parent: null,
	documentType: {
		id: '1addd0ad-0e34-4386-801b-79cf7beb8cf1',
		icon: 'icon-grid color-green',
	},
	hasChildren: false,
	noAccess: false,
	isProtected: false,
	isTrashed: false,
	variants: [
		{
			state: 'Published' as UmbDocumentVariantState,
			publishDate: '2026-04-16 12:30:12.7971658',
			culture: null,
			segment: null,
			name: 'Block Grid',
			createDate: '2023-04-19 09:00:36',
			updateDate: '2026-04-16 12:30:12.7971658',
			id: '17cd53f2-93b3-4e34-ade2-916e7a6639ed',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.BlockGrid',
			alias: 'blockGridDefaultConfig',
			culture: null,
			segment: null,
			value: {
				contentData: [
					{
						contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
						key: '73f797f1-3538-48f3-9ab8-57b7b6528ff4',
						values: [
							{
								editorAlias: 'Umbraco.TextBox',
								culture: null,
								segment: null,
								alias: 'title',
								value: 'Element One, full width',
							},
						],
					},
					{
						contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
						key: '4b97e38d-176d-4ffd-a51f-ff4e461f7b5d',
						values: [
							{
								editorAlias: 'Umbraco.TextBox',
								culture: null,
								segment: null,
								alias: 'title',
								value: 'Element one, half width (left)',
							},
						],
					},
				],
				settingsData: [],
				expose: [
					{
						contentKey: '73f797f1-3538-48f3-9ab8-57b7b6528ff4',
						culture: null,
						segment: null,
					},
					{
						contentKey: '4b97e38d-176d-4ffd-a51f-ff4e461f7b5d',
						culture: null,
						segment: null,
					},
				],
				layout: {
					'Umbraco.BlockGrid': [
						{
							columnSpan: 12,
							rowSpan: 1,
							areas: [],
							key: '73f797f1-3538-48f3-9ab8-57b7b6528ff4',
							contentKey: '73f797f1-3538-48f3-9ab8-57b7b6528ff4',
							settingsKey: null,
						},
						{
							columnSpan: 6,
							rowSpan: 1,
							areas: [],
							key: '4b97e38d-176d-4ffd-a51f-ff4e461f7b5d',
							contentKey: '4b97e38d-176d-4ffd-a51f-ff4e461f7b5d',
							settingsKey: null,
						},
						{
							columnSpan: 6,
							rowSpan: 1,
							areas: [],
							key: 'block-grid-item-library-element-two',
							contentKey: 'library-element-two-id',
							settingsKey: null,
							isExternalContent: true,
						},
					],
				},
			},
		},
	],
	flags: [],
};

const blockListDocument: UmbMockDocumentModel = {
	ancestors: [],
	template: null,
	id: '39842212-489e-46ec-a63b-6eeff36c7156',
	createDate: '2023-04-17 14:03:51',
	parent: null,
	documentType: {
		id: '61c6b912-8fe8-4e10-a07b-4f777b99489b',
		icon: 'icon-bulleted-list color-green',
	},
	hasChildren: false,
	noAccess: false,
	isProtected: false,
	isTrashed: false,
	variants: [
		{
			state: 'Published' as UmbDocumentVariantState,
			publishDate: '2026-04-16 12:30:41.8293415',
			culture: null,
			segment: null,
			name: 'Block List',
			createDate: '2023-04-17 14:03:51',
			updateDate: '2026-04-16 12:30:41.8293415',
			id: '39842212-489e-46ec-a63b-6eeff36c7156',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.BlockList',
			alias: 'blockListDefaultConfig',
			culture: null,
			segment: null,
			value: {
				contentData: [
					{
						contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
						key: 'a08c8c6a-8da2-46d0-87b7-536985985b24',
						values: [
							{
								editorAlias: 'Umbraco.TextBox',
								culture: null,
								segment: null,
								alias: 'title',
								value: 'This is Element One',
							},
						],
					},
				],
				settingsData: [],
				expose: [
					{
						contentKey: 'a08c8c6a-8da2-46d0-87b7-536985985b24',
						culture: null,
						segment: null,
					},
				],
				layout: {
					'Umbraco.BlockList': [
						{
							key: 'a08c8c6a-8da2-46d0-87b7-536985985b24',
							contentUdi: 'umb://element/a08c8c6a8da246d087b7536985985b24',
							settingsUdi: null,
							contentKey: 'a08c8c6a-8da2-46d0-87b7-536985985b24',
							settingsKey: null,
						},
						{
							key: 'block-list-item-library-element-two',
							contentKey: 'library-element-two-id',
							settingsKey: null,
							isExternalContent: true,
						},
					],
				},
			},
		},
	],
	flags: [],
};

const blockSingleDocument: UmbMockDocumentModel = {
	ancestors: [],
	template: null,
	id: 'd98b0eaf-8a5d-4644-a2cc-861f94e56df1',
	createDate: '2026-04-16 11:10:14.571705',
	parent: null,
	documentType: {
		id: '9309d592-ebc7-4f72-a1bd-ebdabca4c643',
		icon: 'icon-shape-square color-green',
	},
	hasChildren: false,
	noAccess: false,
	isProtected: false,
	isTrashed: false,
	variants: [
		{
			state: 'Published' as UmbDocumentVariantState,
			publishDate: '2026-04-16 11:10:36.9975709',
			culture: null,
			segment: null,
			name: 'Block Single',
			createDate: '2026-04-16 11:10:14.571705',
			updateDate: '2026-04-16 11:10:36.9975709',
			id: 'd98b0eaf-8a5d-4644-a2cc-861f94e56df1',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.SingleBlock',
			alias: 'blockSingleDefaultConfig',
			culture: null,
			segment: null,
			value: {
				contentData: [],
				settingsData: [],
				expose: [],
				layout: {
					'Umbraco.SingleBlock': [
						{
							key: 'block-single-item-library-element-one',
							contentKey: 'library-element-one-id',
							settingsKey: null,
							isExternalContent: true,
						},
					],
				},
			},
		},
	],
	flags: [],
};

const richTextEditorDocument: UmbMockDocumentModel = {
	ancestors: [],
	template: null,
	id: '464ca81d-30e0-4169-899a-0556303b878c',
	createDate: '2023-02-20 16:23:17',
	parent: null,
	documentType: {
		id: 'fd62fafc-9cfd-470a-a260-93af5d1ed641',
		icon: 'icon-browser-window color-green',
	},
	hasChildren: false,
	noAccess: false,
	isProtected: false,
	isTrashed: false,
	variants: [
		{
			state: 'Published' as UmbDocumentVariantState,
			publishDate: '2026-04-16 11:10:37.0771127',
			culture: null,
			segment: null,
			name: 'Rich Text Editor',
			createDate: '2023-02-20 16:23:17',
			updateDate: '2026-04-16 11:10:37.0771127',
			id: '464ca81d-30e0-4169-899a-0556303b878c',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.RichText',
			alias: 'richTextEditorWithBlocks',
			culture: null,
			segment: null,
			value: {
				markup: `<p>This Rich Text Editor allows inserting blocks:</p>
<umb-rte-block data-content-key="f3a1c8d4-5b2e-4a97-8d6f-3c9e7b2a5d10"></umb-rte-block>
<p>...and continues after the block.</p>
<umb-rte-block data-key="rich-text-editor-library-element-two" data-content-key="library-element-two-id"></umb-rte-block>
<p>...and can include a block from the library.</p>`,
				blocks: {
					contentData: [
						{
							contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
							key: 'f3a1c8d4-5b2e-4a97-8d6f-3c9e7b2a5d10',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'title',
									value: 'This is Element One, inside a Rich Text Editor',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: 'f3a1c8d4-5b2e-4a97-8d6f-3c9e7b2a5d10',
							culture: null,
							segment: null,
						},
					],
					layout: {
						'Umbraco.RichText': [
							{
								key: 'f3a1c8d4-5b2e-4a97-8d6f-3c9e7b2a5d10',
								contentKey: 'f3a1c8d4-5b2e-4a97-8d6f-3c9e7b2a5d10',
								settingsKey: null,
							},
							{
								key: 'rich-text-editor-library-element-two',
								contentKey: 'library-element-two-id',
								settingsKey: null,
								isExternalContent: true,
							},
						],
					},
				},
			},
		},
	],
	flags: [],
};

const variantBlockListDocument: UmbMockDocumentModel = {
	ancestors: [],
	template: null,
	id: '8b5e2d7a-4c19-4a3f-9e6b-1d0f7a2c8e53',
	createDate: '2026-04-16 13:00:00.000000',
	parent: null,
	documentType: {
		id: 'c7d3e1a4-2b58-4f69-9a0e-6d1f8b3c5e72',
		icon: 'icon-bulleted-list color-green',
	},
	hasChildren: false,
	noAccess: false,
	isProtected: false,
	isTrashed: false,
	variants: [
		{
			state: 'Published' as UmbDocumentVariantState,
			publishDate: '2026-04-16 13:05:00.000000',
			culture: 'en-US',
			segment: null,
			name: 'Variant Block List',
			createDate: '2026-04-16 13:00:00.000000',
			updateDate: '2026-04-16 13:05:00.000000',
			id: 'variant-block-list-en-us',
			flags: [],
		},
		{
			state: 'Draft' as UmbDocumentVariantState,
			publishDate: null,
			culture: 'da',
			segment: null,
			name: 'Variant Bloklist',
			createDate: '2026-04-16 13:00:00.000000',
			updateDate: '2026-04-16 13:00:00.000000',
			id: 'variant-block-list-da',
			flags: [],
		},
	],
	values: [
		{
			editorAlias: 'Umbraco.BlockList',
			alias: 'blockListVariant',
			culture: 'en-US',
			segment: null,
			value: {
				contentData: [
					{
						contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
						key: '2f6c9a1e-7b4d-4e38-a5c2-9d1e3b7f4a60',
						values: [
							{
								editorAlias: 'Umbraco.TextBox',
								culture: null,
								segment: null,
								alias: 'title',
								value: 'This is Element One (English)',
							},
						],
					},
				],
				settingsData: [],
				expose: [
					{
						contentKey: '2f6c9a1e-7b4d-4e38-a5c2-9d1e3b7f4a60',
						culture: null,
						segment: null,
					},
				],
				layout: {
					'Umbraco.BlockList': [
						{
							key: '2f6c9a1e-7b4d-4e38-a5c2-9d1e3b7f4a60',
							contentKey: '2f6c9a1e-7b4d-4e38-a5c2-9d1e3b7f4a60',
							settingsKey: null,
						},
						{
							key: 'variant-block-list-en-library-element-two',
							contentKey: 'library-element-two-id',
							settingsKey: null,
							isExternalContent: true,
						},
					],
				},
			},
		},
		{
			editorAlias: 'Umbraco.BlockList',
			alias: 'blockListVariant',
			culture: 'da',
			segment: null,
			value: {
				contentData: [
					{
						contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
						key: '5a8d3c7f-1e92-4b06-8f4a-6c2b9e0d7a31',
						values: [
							{
								editorAlias: 'Umbraco.TextBox',
								culture: null,
								segment: null,
								alias: 'title',
								value: 'Dette er Element Et (dansk)',
							},
						],
					},
				],
				settingsData: [],
				expose: [
					{
						contentKey: '5a8d3c7f-1e92-4b06-8f4a-6c2b9e0d7a31',
						culture: null,
						segment: null,
					},
				],
				layout: {
					'Umbraco.BlockList': [
						{
							key: '5a8d3c7f-1e92-4b06-8f4a-6c2b9e0d7a31',
							contentKey: '5a8d3c7f-1e92-4b06-8f4a-6c2b9e0d7a31',
							settingsKey: null,
						},
						{
							key: 'variant-block-list-da-library-element-one',
							contentKey: 'library-element-one-id',
							settingsKey: null,
							isExternalContent: true,
						},
					],
				},
			},
		},
		{
			editorAlias: 'Umbraco.BlockList',
			alias: 'blockListShared',
			culture: null,
			segment: null,
			value: {
				contentData: [
					{
						contentTypeKey: 'b818bb55-31e1-4537-9c42-17471a176089',
						key: 'b3e7a5c9-8d21-4f64-a0b8-7e5c1d9f2a46',
						values: [
							{
								editorAlias: 'Umbraco.TextBox',
								culture: null,
								segment: null,
								alias: 'title',
								value: 'This is a shared Element One',
							},
						],
					},
					{
						contentTypeKey: 'a9c5e3f7-6b12-4d84-9e0f-2b7a4d1c8e65',
						key: 'c8f1d6a3-5e72-4b09-9d4a-1a7b3e5c8f20',
						values: [
							{
								editorAlias: 'Umbraco.TextBox',
								culture: 'en-US',
								segment: null,
								alias: 'title',
								value: 'Only exposed in English',
							},
						],
					},
				],
				settingsData: [],
				expose: [
					{
						contentKey: 'b3e7a5c9-8d21-4f64-a0b8-7e5c1d9f2a46',
						culture: null,
						segment: null,
					},
					{
						contentKey: 'c8f1d6a3-5e72-4b09-9d4a-1a7b3e5c8f20',
						culture: 'en-US',
						segment: null,
					},
				],
				layout: {
					'Umbraco.BlockList': [
						{
							key: 'b3e7a5c9-8d21-4f64-a0b8-7e5c1d9f2a46',
							contentKey: 'b3e7a5c9-8d21-4f64-a0b8-7e5c1d9f2a46',
							settingsKey: null,
						},
						{
							key: 'variant-block-list-shared-library-element-two',
							contentKey: 'library-element-two-id',
							settingsKey: null,
							isExternalContent: true,
						},
						{
							key: 'c8f1d6a3-5e72-4b09-9d4a-1a7b3e5c8f20',
							contentKey: 'c8f1d6a3-5e72-4b09-9d4a-1a7b3e5c8f20',
							settingsKey: null,
						},
						{
							key: 'variant-block-list-shared-library-variant-element',
							contentKey: 'library-variant-element-id',
							settingsKey: null,
							isExternalContent: true,
						},
					],
				},
			},
		},
	],
	flags: [],
};

export const data: Array<UmbMockDocumentModel> = [
	blockGridDocument,
	blockListDocument,
	blockSingleDocument,
	richTextEditorDocument,
	variantBlockListDocument,
];
