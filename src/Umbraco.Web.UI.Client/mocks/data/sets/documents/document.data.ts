import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import { COLLECTION_DATA_TYPE_ID } from './data-type.data.js';
import {
	COLLECTION_DOCUMENT_TYPE_ID,
	COLLECTION_ITEM_DOCUMENT_TYPE_ID,
	INVARIANT_DOCUMENT_TYPE_ID,
	INVARIANT_DOCUMENT_TYPE_WITH_CULTURE_VARIANT_COMPOSITION_ID,
	INVARIANT_DOCUMENT_TYPE_WITH_SEGMENT_VARIANT_COMPOSITION_ID,
	SEGMENT_VARIANT_DOCUMENT_TYPE_ID,
	VARIANT_DOCUMENT_TYPE_ID,
} from './document-type.data.js';
import type { DocumentVariantResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

export const COLLECTION_DOCUMENT_ID = 'documents-collection-document-id';

const COLLECTION_ITEM_COUNT = 1000;

const collectionItems: Array<UmbMockDocumentModel> = Array.from({ length: COLLECTION_ITEM_COUNT }, (_, index) => {
	const number = index + 1;
	const id = `documents-collection-item-${number}-document-id`;
	// Spread the dates an hour apart, so ordering by create or update date is not the same as the sort order.
	const createDate = new Date(Date.UTC(2024, 0, 15, 10, 0, 0) + number * 3600000).toISOString();
	const updateDate = new Date(
		Date.UTC(2024, 0, 15, 10, 0, 0) + (COLLECTION_ITEM_COUNT - number) * 3600000,
	).toISOString();

	return {
		id,
		createDate,
		parent: { id: COLLECTION_DOCUMENT_ID },
		ancestors: [{ id: COLLECTION_DOCUMENT_ID }],
		documentType: {
			id: COLLECTION_ITEM_DOCUMENT_TYPE_ID,
			icon: 'icon-document',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		template: null,
		variants: [
			{
				state: (number % 4 === 0 ? 'Draft' : 'Published') as UmbDocumentVariantState,
				publishDate: number % 4 === 0 ? null : updateDate,
				culture: null,
				segment: null,
				name: `Collection Item ${number}`,
				createDate,
				updateDate,
				id: `documents-collection-item-${number}-document`,
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'text',
				culture: null,
				segment: null,
				value: `This is collection item ${number}.`,
			},
		],
		flags: [],
	} satisfies UmbMockDocumentModel;
});

export const data: Array<UmbMockDocumentModel> = [
	{
		id: COLLECTION_DOCUMENT_ID,
		createDate: '2024-01-15T10:00:00.000Z',
		parent: null,
		ancestors: [],
		documentType: {
			id: COLLECTION_DOCUMENT_TYPE_ID,
			icon: 'icon-layers',
			collection: { id: COLLECTION_DATA_TYPE_ID },
		},
		hasChildren: true,
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
				name: 'Collection',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'documents-collection-document',
				flags: [],
			},
		],
		values: [],
		flags: [],
	},
	...collectionItems,
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
		id: 'variant-documents-segment-variant-document-id',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: null,
		ancestors: [],
		documentType: {
			id: SEGMENT_VARIANT_DOCUMENT_TYPE_ID,
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
				name: 'Segment Variant Document',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'variant-documents-segment-variant-document-default',
				flags: [],
			},
			{
				state: 'Draft' as UmbDocumentVariantState,
				publishDate: null,
				culture: null,
				segment: 's1',
				name: 'Segment Variant Document',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:00:00.000Z',
				id: 'variant-documents-segment-variant-document-s1',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'text',
				culture: null,
				segment: null,
				value: 'This invariant text is shared across all segments.',
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'segmentText',
				culture: null,
				segment: null,
				value: 'This is the default segment text.',
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'segmentText',
				culture: null,
				segment: 's1',
				value: 'This is the segment 1 text.',
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
			id: INVARIANT_DOCUMENT_TYPE_WITH_CULTURE_VARIANT_COMPOSITION_ID,
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
				name: 'Invariant With Culture Variant Composition',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:00:00.000Z',
				id: 'variant-documents-invariant-with-variant-composition-document',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'compositionCultureInvariantText',
				culture: null,
				segment: null,
				value: 'Initial composition culture invariant text.',
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'compositionCultureVariantText',
				culture: null,
				segment: null,
				value: 'Initial composition variant text.',
			},
		],
		flags: [],
	},
	{
		id: 'variant-documents-invariant-with-segment-variant-composition-document-id',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: null,
		ancestors: [],
		documentType: {
			id: INVARIANT_DOCUMENT_TYPE_WITH_SEGMENT_VARIANT_COMPOSITION_ID,
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
				name: 'Invariant With Segment Variant Composition',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:00:00.000Z',
				id: 'variant-documents-invariant-with-segment-variant-composition-document',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'compositionSegmentInvariantText',
				culture: null,
				segment: null,
				value: 'Initial composition segment invariant text.',
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'compositionSegmentText',
				culture: null,
				segment: null,
				value: 'Initial composition segment text (default segment).',
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'compositionSegmentText',
				culture: null,
				segment: 's1',
				value: 'Initial composition segment text (segment 1).',
			},
		],
		flags: [],
	},
];
