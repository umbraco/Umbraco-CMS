import type { UmbMockDocumentModel } from '../document.data.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

const permissionsTestDocument = {
	ancestorIds: [],
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
			name: 'Permissions',
			createDate: '2023-02-06T15:32:05.350038',
			updateDate: '2023-02-06T15:32:24.957009',
		},
	],
};

export const data: Array<UmbMockDocumentModel> = [
	permissionsTestDocument,
	{
		...permissionsTestDocument,
		ancestorIds: ['permissions-document-id'],
		hasChildren: false,
		id: 'permissions-1-document-id',
		parent: { id: 'permissions-document-id' },
		urls: [
			{
				culture: null,
				url: '/permission-1',
			},
		],
		variants: permissionsTestDocument.variants.map((variant) => ({
			...variant,
			name: 'Permissions 1',
		})),
	},
	{
		...permissionsTestDocument,
		ancestorIds: ['permissions-document-id'],
		hasChildren: true,
		id: 'permissions-2-document-id',
		parent: { id: 'permissions-document-id' },
		urls: [
			{
				culture: null,
				url: '/permissions-2',
			},
		],
		variants: permissionsTestDocument.variants.map((variant) => ({
			...variant,
			name: 'Permissions 2',
		})),
	},
	{
		...permissionsTestDocument,
		ancestorIds: ['permissions-document-id', 'permissions-2-document-id'],
		hasChildren: true,
		id: 'permission-2-1-document-id',
		parent: { id: 'permissions-2-document-id' },
		urls: [
			{
				culture: null,
				url: '/permissions-1/permissions-2-1',
			},
		],
		variants: permissionsTestDocument.variants.map((variant) => ({
			...variant,
			name: 'Permissions 2.1',
		})),
	},
	{
		...permissionsTestDocument,
		ancestorIds: ['permissions-document-id', 'permissions-2-document-id'],
		hasChildren: false,
		id: 'permissions-2-2-document-id',
		parent: { id: 'permissions-2-document-id' },
		urls: [
			{
				culture: null,
				url: '/permissions-1/permissions-2-2',
			},
		],
		variants: permissionsTestDocument.variants.map((variant) => ({
			...variant,
			name: 'Permissions 2.2',
		})),
	},
	{
		...permissionsTestDocument,
		ancestorIds: ['permissions-document-id', 'permissions-2-document-id', 'permissions-2-2-document-id'],
		hasChildren: false,
		id: 'permission-2-2-1-document-id',
		parent: { id: 'permissions-2-2-document-id' },
		urls: [
			{
				culture: null,
				url: '/permissions-1/permissions-2-2/permissions-2-2-1',
			},
		],
		variants: permissionsTestDocument.variants.map((variant) => ({
			...variant,
			name: 'Permissions 2.2.1',
		})),
	},
];
