import type { UmbMockDocumentModel } from '../../types/mock-data-set.types.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

const baseDocument = {
	ancestors: [],
	urls: [
		{
			culture: null,
			url: '/',
		},
	],
	template: null,
	id: 'permissions-document-id',
	createDate: '2023-02-06T15:32:05.350038',
	parent: null,
	documentType: {
		id: 'the-simplest-document-type-id',
		icon: 'icon-document',
	},
	hasChildren: false,
	noAccess: false,
	isProtected: false,
	isTrashed: false,
	values: [],
	variants: [
		{
			state: DocumentVariantStateModel.PUBLISHED,
			publishDate: '2023-02-06T15:32:24.957009',
			culture: null,
			segment: null,
			name: 'Permissions Root',
			createDate: '2023-02-06T15:32:05.350038',
			updateDate: '2023-02-06T15:32:24.957009',
			id: 'permissions',
			flags: [],
		},
	],
	flags: [],
};

export const data: Array<UmbMockDocumentModel> = [
	baseDocument,
	{
		...baseDocument,
		ancestors: [{ id: 'permissions-document-id' }],
		id: 'permissions-0-document-id',
		parent: { id: 'permissions-document-id' },
		variants: baseDocument.variants.map((variant) => ({
			...variant,
			name: 'No Access Document',
			id: 'permissions-0',
		})),
		flags: [],
		noAccess: true,
	},
	{
		...baseDocument,
		ancestors: [{ id: 'permissions-0-document-id' }],
		id: 'permissions-0-1-document-id',
		parent: { id: 'permissions-0-document-id' },
		variants: baseDocument.variants.map((variant) => ({
			...variant,
			name: 'Has Access Document',
			id: 'permissions-0-1',
		})),
		flags: [],
		noAccess: false,
	},
	{
		...baseDocument,
		ancestors: [{ id: 'permissions-document-id' }],
		id: 'permissions-1-document-id',
		parent: { id: 'permissions-document-id' },
		variants: baseDocument.variants.map((variant) => ({
			...variant,
			name: 'Child Document 1',
			id: 'permissions-1',
		})),
		flags: [],
	},
	{
		...baseDocument,
		ancestors: [{ id: 'permissions-document-id' }],
		hasChildren: true,
		id: 'permissions-2-document-id',
		parent: { id: 'permissions-document-id' },
		variants: baseDocument.variants.map((variant) => ({
			...variant,
			name: 'Child Document 2',
			id: 'permissions-2',
		})),
		flags: [],
	},
	{
		...baseDocument,
		ancestors: [{ id: 'permissions-document-id' }, { id: 'permissions-2-document-id' }],
		hasChildren: true,
		id: 'permission-2-1-document-id',
		parent: { id: 'permissions-2-document-id' },
		urls: [
			{
				culture: null,
				url: '/permissions-root/child-document-2-1',
			},
		],
		variants: baseDocument.variants.map((variant) => ({
			...variant,
			name: 'Child Document 2.1',
		})),
		flags: [],
	},
	{
		...baseDocument,
		ancestors: [{ id: 'permissions-document-id' }, { id: 'permissions-2-document-id' }],
		id: 'permissions-2-2-document-id',
		parent: { id: 'permissions-2-document-id' },
		urls: [
			{
				culture: null,
				url: '/permissions-root/child-document-2-2',
			},
		],
		variants: baseDocument.variants.map((variant) => ({
			...variant,
			name: 'Child Document 2.2',
		})),
		flags: [],
	},
	{
		...baseDocument,
		ancestors: [
			{ id: 'permissions-document-id' },
			{ id: 'permissions-2-document-id' },
			{ id: 'permissions-2-2-document-id' },
		],
		id: 'permission-2-2-1-document-id',
		parent: { id: 'permissions-2-2-document-id' },
		urls: [
			{
				culture: null,
				url: '/permissions-root/child-document-2-2/child-document-2-2-1',
			},
		],
		variants: baseDocument.variants.map((variant) => ({
			...variant,
			name: 'Child Document 2.2.1',
		})),
		flags: [],
	},
];
