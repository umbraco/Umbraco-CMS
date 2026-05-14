import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import {
	INVARIANT_DOCUMENT_TYPE_ID,
	INVARIANT_DOCUMENT_TYPE_WITH_VARIANT_COMPOSITION_ID,
	VARIANT_DOCUMENT_TYPE_ID,
} from './document-type.data.js';
import type { DocumentVariantResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

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
				state: 'Published' as UmbDocumentVariantState,
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
				state: 'Published' as UmbDocumentVariantState,
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
				state: 'Draft' as UmbDocumentVariantState,
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
	{
		id: 'variant-documents-invariant-with-variant-composition-document-id',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: null,
		ancestors: [],
		documentType: {
			id: INVARIANT_DOCUMENT_TYPE_WITH_VARIANT_COMPOSITION_ID,
			icon: 'icon-document',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		template: null,
		variants: [
			{
				state: 'Draft' as UmbDocumentVariantState,
				publishDate: null,
				culture: null,
				segment: null,
				name: 'Invariant With Variant Composition',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:00:00.000Z',
				id: 'variant-documents-invariant-with-variant-composition-document',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'compositionVariantText',
				culture: null,
				segment: null,
				value: 'Initial composition value.',
			},
		],
		flags: [],
	},
];
