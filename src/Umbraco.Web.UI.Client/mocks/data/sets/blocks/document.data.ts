import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import {
	NESTED_BLOCK_LIST_DOCUMENT_TYPE_ID,
	NESTED_BLOCK_LIST_INNER_ELEMENT_TYPE_ID,
	NESTED_BLOCK_LIST_OUTER_ELEMENT_TYPE_ID,
	VARIANT_BLOCK_ELEMENT_TYPE_ID,
	VARIANT_BLOCK_LIST_DOCUMENT_TYPE_ID,
	VARIANT_HIDDEN_EDITOR_ELEMENT_TYPE_ID,
	VARIANT_INLINE_BLOCK_LIST_DOCUMENT_TYPE_ID,
} from './document-type.data.js';
import type { DocumentVariantResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

export const data: Array<UmbMockDocumentModel> = [
	{
		// Both block lists have inline editing enabled, and the document starts with one outer block
		// holding one inner block — so an existing inline block can be compared against a newly added one.
		id: 'b2481aeb-1fc3-4b9c-bdd3-5786771001a5',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: null,
		ancestors: [],
		documentType: {
			id: NESTED_BLOCK_LIST_DOCUMENT_TYPE_ID,
			icon: 'icon-document',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		template: null,
		variants: [
			{
				state: 'Published' as UmbDocumentVariantState,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: null,
				segment: null,
				name: 'Nested Inline Block List',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'd959c601-3e15-4cf6-b790-d066044612a8',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blocks',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: NESTED_BLOCK_LIST_OUTER_ELEMENT_TYPE_ID,
							key: 'c0f1a5e0-1d3b-4f2a-9c1e-2a7b8d4e6f01',
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'text',
									value: 'This is the outer block.',
								},
								{
									editorAlias: 'Umbraco.TextBox',
									culture: null,
									segment: null,
									alias: 'note',
									value: 'This is the outer block note.',
								},
								{
									editorAlias: 'Umbraco.BlockList',
									culture: null,
									segment: null,
									alias: 'innerBlocks',
									value: {
										contentData: [
											{
												contentTypeKey: NESTED_BLOCK_LIST_INNER_ELEMENT_TYPE_ID,
												key: 'd1e2f3a4-5b6c-4d7e-8f90-1a2b3c4d5e02',
												values: [
													{
														editorAlias: 'Umbraco.TextBox',
														culture: null,
														segment: null,
														alias: 'text',
														value: 'This is the inner block.',
													},
													{
														editorAlias: 'Umbraco.TextBox',
														culture: null,
														segment: null,
														alias: 'note',
														value: 'This is the inner block note.',
													},
												],
											},
										],
										settingsData: [],
										expose: [
											{
												contentKey: 'd1e2f3a4-5b6c-4d7e-8f90-1a2b3c4d5e02',
												culture: null,
												segment: null,
											},
										],
										layout: {
											'Umbraco.BlockList': [
												{
													contentKey: 'd1e2f3a4-5b6c-4d7e-8f90-1a2b3c4d5e02',
													settingsKey: null,
												},
											],
										},
									},
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{
							contentKey: 'c0f1a5e0-1d3b-4f2a-9c1e-2a7b8d4e6f01',
							culture: null,
							segment: null,
						},
					],
					layout: {
						'Umbraco.BlockList': [
							{
								contentKey: 'c0f1a5e0-1d3b-4f2a-9c1e-2a7b8d4e6f01',
								settingsKey: null,
							},
						],
					},
				},
			},
		],
		flags: [],
	},
	{
		// Block-level variance. "Shared block" is exposed in both cultures, "English only" only in en-US and
		// "Danish only" only in da, so un-exposing can be tried from either culture. "No editor" has no workspace
		// to open, so it can only be set as draft through its block action.
		id: '2bb5536f-deb6-429c-b6d4-dc2975646a4b',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: null,
		ancestors: [],
		documentType: {
			id: VARIANT_BLOCK_LIST_DOCUMENT_TYPE_ID,
			icon: 'icon-document',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		template: null,
		variants: [
			{
				state: 'Published' as UmbDocumentVariantState,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'en-US',
				segment: null,
				name: 'Variant Block List',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: '5d0f7a1e-3c2b-4e8a-9b6d-1f4a7c9e2b30',
				flags: [],
			},
			{
				state: 'Published' as UmbDocumentVariantState,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'da',
				segment: null,
				name: 'Variant blokliste',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: '8e3b1c6d-7a4f-4d2e-b5c9-0a6f2d8e4b71',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blocks',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: VARIANT_BLOCK_ELEMENT_TYPE_ID,
							key: '06da2eb6-4447-4d2b-9ee3-56bf357eb91c',
							values: [
								{ editorAlias: 'Umbraco.TextBox', culture: 'en-US', segment: null, alias: 'headline', value: 'Shared block' },
								{ editorAlias: 'Umbraco.TextBox', culture: 'da', segment: null, alias: 'headline', value: 'Delt blok' },
							],
						},
						{
							contentTypeKey: VARIANT_BLOCK_ELEMENT_TYPE_ID,
							key: '9ea6e58f-1182-41b7-a67a-5fe3846b2225',
							values: [
								{ editorAlias: 'Umbraco.TextBox', culture: 'en-US', segment: null, alias: 'headline', value: 'English only' },
							],
						},
						{
							contentTypeKey: VARIANT_BLOCK_ELEMENT_TYPE_ID,
							key: '7c1d2e3f-4a5b-4c6d-8e7f-9a0b1c2d3e4f',
							values: [
								{ editorAlias: 'Umbraco.TextBox', culture: 'da', segment: null, alias: 'headline', value: 'Kun dansk' },
							],
						},
						{
							contentTypeKey: VARIANT_HIDDEN_EDITOR_ELEMENT_TYPE_ID,
							key: '3a9c7e21-5b4d-4f60-8c12-7d8e9f0a1b2c',
							values: [
								{ editorAlias: 'Umbraco.TextBox', culture: 'en-US', segment: null, alias: 'headline', value: 'No editor' },
								{ editorAlias: 'Umbraco.TextBox', culture: 'da', segment: null, alias: 'headline', value: 'Ingen editor' },
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: '06da2eb6-4447-4d2b-9ee3-56bf357eb91c', culture: 'en-US', segment: null },
						{ contentKey: '06da2eb6-4447-4d2b-9ee3-56bf357eb91c', culture: 'da', segment: null },
						{ contentKey: '9ea6e58f-1182-41b7-a67a-5fe3846b2225', culture: 'en-US', segment: null },
						{ contentKey: '7c1d2e3f-4a5b-4c6d-8e7f-9a0b1c2d3e4f', culture: 'da', segment: null },
						{ contentKey: '3a9c7e21-5b4d-4f60-8c12-7d8e9f0a1b2c', culture: 'en-US', segment: null },
						{ contentKey: '3a9c7e21-5b4d-4f60-8c12-7d8e9f0a1b2c', culture: 'da', segment: null },
					],
					layout: {
						'Umbraco.BlockList': [
							{ contentKey: '06da2eb6-4447-4d2b-9ee3-56bf357eb91c', settingsKey: null },
							{ contentKey: '9ea6e58f-1182-41b7-a67a-5fe3846b2225', settingsKey: null },
							{ contentKey: '7c1d2e3f-4a5b-4c6d-8e7f-9a0b1c2d3e4f', settingsKey: null },
							{ contentKey: '3a9c7e21-5b4d-4f60-8c12-7d8e9f0a1b2c', settingsKey: null },
						],
					},
				},
			},
		],
		flags: [],
	},
	{
		// Block-level variance with inline editing. Inline blocks open no workspace, so setting one as draft is
		// only possible through its block action.
		id: 'cdef107b-33a9-4514-a0d3-b9000e5c6230',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: null,
		ancestors: [],
		documentType: {
			id: VARIANT_INLINE_BLOCK_LIST_DOCUMENT_TYPE_ID,
			icon: 'icon-document',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		template: null,
		variants: [
			{
				state: 'Published' as UmbDocumentVariantState,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'en-US',
				segment: null,
				name: 'Variant Inline Block List',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'd0e06ee9-eb0d-4314-bbaf-37a4c8b9093d',
				flags: [],
			},
			{
				state: 'Published' as UmbDocumentVariantState,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'da',
				segment: null,
				name: 'Variant inline blokliste',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: '880da2b6-3845-473c-b769-f43c0257a0a6',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blocks',
				culture: null,
				segment: null,
				value: {
					contentData: [
						{
							contentTypeKey: VARIANT_BLOCK_ELEMENT_TYPE_ID,
							key: '6b8d0f42-1c3e-4a57-9d68-0e1f2a3b4c5d',
							values: [
								{ editorAlias: 'Umbraco.TextBox', culture: 'en-US', segment: null, alias: 'headline', value: 'Inline shared block' },
								{ editorAlias: 'Umbraco.TextBox', culture: 'da', segment: null, alias: 'headline', value: 'Delt inline blok' },
							],
						},
						{
							contentTypeKey: VARIANT_BLOCK_ELEMENT_TYPE_ID,
							key: '8f2a4c61-3e5b-4d79-a0b1-2c3d4e5f6a7b',
							values: [
								{ editorAlias: 'Umbraco.TextBox', culture: 'en-US', segment: null, alias: 'headline', value: 'Inline English only' },
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: '6b8d0f42-1c3e-4a57-9d68-0e1f2a3b4c5d', culture: 'en-US', segment: null },
						{ contentKey: '6b8d0f42-1c3e-4a57-9d68-0e1f2a3b4c5d', culture: 'da', segment: null },
						{ contentKey: '8f2a4c61-3e5b-4d79-a0b1-2c3d4e5f6a7b', culture: 'en-US', segment: null },
					],
					layout: {
						'Umbraco.BlockList': [
							{ contentKey: '6b8d0f42-1c3e-4a57-9d68-0e1f2a3b4c5d', settingsKey: null },
							{ contentKey: '8f2a4c61-3e5b-4d79-a0b1-2c3d4e5f6a7b', settingsKey: null },
						],
					},
				},
			},
		],
		flags: [],
	},
];
