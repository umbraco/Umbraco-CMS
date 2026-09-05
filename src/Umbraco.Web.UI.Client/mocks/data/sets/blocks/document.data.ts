import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import {
	NESTED_BLOCK_LIST_DOCUMENT_TYPE_ID,
	NESTED_BLOCK_LIST_INNER_ELEMENT_TYPE_ID,
	NESTED_BLOCK_LIST_OUTER_ELEMENT_TYPE_ID,
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
];
