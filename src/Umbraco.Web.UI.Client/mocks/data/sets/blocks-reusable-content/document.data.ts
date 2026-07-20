import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import type { DocumentVariantResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

export const data: Array<UmbMockDocumentModel> = [
	{
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
								contentUdi: 'umb://element/73f797f1353848f39ab857b7b6528ff4',
								settingsUdi: null,
								contentKey: '73f797f1-3538-48f3-9ab8-57b7b6528ff4',
								settingsKey: null,
							},
							{
								columnSpan: 6,
								rowSpan: 1,
								areas: [],
								contentUdi: 'umb://element/4b97e38d176d4ffda51fff4e461f7b5d',
								settingsUdi: null,
								contentKey: '4b97e38d-176d-4ffd-a51f-ff4e461f7b5d',
								settingsKey: null,
							},
							{
								columnSpan: 6,
								rowSpan: 1,
								areas: [],
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
	},
	{
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
								contentUdi: 'umb://element/a08c8c6a8da246d087b7536985985b24',
								settingsUdi: null,
								contentKey: 'a08c8c6a-8da2-46d0-87b7-536985985b24',
								settingsKey: null,
							},
							{
								key: 'block-list-item-library-element-one-a',
								contentKey: 'library-element-one-id',
								settingsKey: null,
								isExternalContent: true,
							},
							{
								contentKey: 'library-element-two-id',
								settingsKey: null,
								isExternalContent: true,
							},
							{
								key: 'block-list-item-library-element-three',
								contentKey: 'library-element-three-id',
								settingsKey: null,
								isExternalContent: true,
							},
							{
								key: 'block-list-item-library-element-four',
								contentKey: 'library-element-four-id',
								settingsKey: null,
								isExternalContent: true,
							},
							{
								key: 'block-list-item-library-element-one-b',
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
	},
	{
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
	},
	{
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
					markup:
						'<p>This Rich Text Editor allows inserting blocks:</p><umb-rte-block data-content-key="f3a1c8d4-5b2e-4a97-8d6f-3c9e7b2a5d10"></umb-rte-block><p>...and continues after the block.</p>',
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
									contentUdi: 'umb://element/f3a1c8d45b2e4a978d6f3c9e7b2a5d10',
									settingsUdi: null,
									contentKey: 'f3a1c8d4-5b2e-4a97-8d6f-3c9e7b2a5d10',
									settingsKey: null,
								},
							],
						},
					},
				},
			},
		],
		flags: [],
	},
];
