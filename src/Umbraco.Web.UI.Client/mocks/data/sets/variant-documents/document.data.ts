import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import { INVARIANT_DOCUMENT_TYPE_ID, VARIANT_DOCUMENT_TYPE_ID } from './document-type.data.js';

export const data: Array<UmbMockDocumentModel> = [
	{
		id: 'variant-documents-invariant-document-id',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: null,
		ancestors: [],
		documentType: {
			id: INVARIANT_DOCUMENT_TYPE_ID,
			icon: 'icon-document',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		template: null,
		variants: [
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: null,
				segment: null,
				name: 'Invariant Document',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'variant-documents-invariant-document',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'text',
				culture: null,
				segment: null,
				value: 'This is the invariant text value.',
			},
		],
		flags: [],
	},
	{
		id: 'variant-documents-variant-document-id',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: null,
		ancestors: [],
		documentType: {
			id: VARIANT_DOCUMENT_TYPE_ID,
			icon: 'icon-document',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		template: null,
		variants: [
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'en-US',
				segment: null,
				name: 'Variant Document',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'variant-documents-variant-document-en-us',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.DRAFT,
				publishDate: null,
				culture: 'da',
				segment: null,
				name: 'Variant Dokument',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:00:00.000Z',
				id: 'variant-documents-variant-document-da',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'text',
				culture: null,
				segment: null,
				value: 'This invariant text is shared across all cultures.',
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'variantText',
				culture: 'en-US',
				segment: null,
				value: 'This is the English variant text.',
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'variantText',
				culture: 'da',
				segment: null,
				value: 'Dette er den danske varianttekst.',
			},
		],
		flags: [],
	},
];
