import type { UmbMockDocumentBlueprintModel } from '../../mock-data-set.types.js';
import { SEGMENT_VARIANT_DOCUMENT_TYPE_ID } from './document-type.data.js';
import type { DocumentVariantResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

export const data: Array<UmbMockDocumentBlueprintModel> = [
	{
		id: 'variant-documents-segment-variant-document-blueprint-id',
		documentType: {
			id: SEGMENT_VARIANT_DOCUMENT_TYPE_ID,
			icon: 'icon-document',
		},
		hasChildren: false,
		isFolder: false,
		parent: null,
		name: 'Segment Variant Document Blueprint',
		variants: [
			{
				state: 'Draft' as UmbDocumentVariantState,
				publishDate: null,
				culture: null,
				segment: null,
				name: 'Segment Variant Document Blueprint',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:00:00.000Z',
				id: 'variant-documents-segment-variant-document-blueprint-default',
				flags: [],
			},
			{
				state: 'Draft' as UmbDocumentVariantState,
				publishDate: null,
				culture: null,
				segment: 's1',
				name: 'Segment Variant Document Blueprint',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:00:00.000Z',
				id: 'variant-documents-segment-variant-document-blueprint-s1',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'segmentText',
				culture: null,
				segment: null,
				value: 'This is the default segment blueprint text.',
			},
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'segmentText',
				culture: null,
				segment: 's1',
				value: 'This is the segment 1 blueprint text.',
			},
		],
		flags: [],
	},
];
