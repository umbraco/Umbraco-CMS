import type { UmbMockDocumentModel } from '../document.data.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

const permissionsTestDocument = {
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
			name: 'Permissions',
			createDate: '2023-02-06T15:32:05.350038',
			updateDate: '2023-02-06T15:32:24.957009',
			id: 'permissions',
			flags: [],
		},
	],
	flags: [],
};

export const data: Array<UmbMockDocumentModel> = [
	permissionsTestDocument,
	{
		...permissionsTestDocument,
		ancestors: [{ id: 'permissions-document-id' }],
		hasChildren: false,
		id: 'permissions-0-document-id',
		parent: { id: 'permissions-document-id' },
		variants: permissionsTestDocument.variants.map((variant) => ({
			...variant,
			name: 'Permissions 0 - No Access',
			id: 'permissions-0',
		})),
		flags: [],
		noAccess: true,
	},
	{
		...permissionsTestDocument,
		ancestors: [{ id: 'permissions-0-document-id' }],
		hasChildren: false,
		id: 'permissions-0-1-document-id',
		parent: { id: 'permissions-0-document-id' },
		variants: permissionsTestDocument.variants.map((variant) => ({
			...variant,
			name: 'Permissions 0.1 - Has access',
			id: 'permissions-0-1',
		})),
		flags: [],
		noAccess: false,
	},
	{
		...permissionsTestDocument,
		ancestors: [{ id: 'permissions-document-id' }],
		hasChildren: false,
		id: 'permissions-1-document-id',
		parent: { id: 'permissions-document-id' },
		variants: permissionsTestDocument.variants.map((variant) => ({
			...variant,
			name: 'Permissions 1',
			id: 'permissions-1',
		})),
		flags: [],
	},
	{
		...permissionsTestDocument,
		ancestors: [{ id: 'permissions-document-id' }],
		hasChildren: true,
		id: 'permissions-2-document-id',
		parent: { id: 'permissions-document-id' },
		variants: permissionsTestDocument.variants.map((variant) => ({
			...variant,
			name: 'Permissions 2',
			id: 'permissions-2',
		})),
		flags: [],
	},
	{
		...permissionsTestDocument,
		ancestors: [{ id: 'permissions-document-id' }, { id: 'permissions-2-document-id' }],
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
		flags: [],
	},
	{
		...permissionsTestDocument,
		ancestors: [{ id: 'permissions-document-id' }, { id: 'permissions-2-document-id' }],
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
		flags: [],
	},
	{
		...permissionsTestDocument,
		ancestors: [
			{ id: 'permissions-document-id' },
			{ id: 'permissions-2-document-id' },
			{ id: 'permissions-2-2-document-id' },
		],
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
		flags: [],
	},
];
