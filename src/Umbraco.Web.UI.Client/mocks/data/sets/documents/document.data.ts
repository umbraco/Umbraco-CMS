import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import {
	INVARIANT_DOCUMENT_TYPE_ID,
	VARIANT_DOCUMENT_TYPE_ID,
	BLM_ET_INVARIANT_ID,
	BLM_ET_VARIANT_INVARIANT_TEXT_ID,
	BLM_ET_VARIANT_VARIANT_TEXT_ID,
	BLM_DOC_TYPE_C1_ID,
	BLM_DOC_TYPE_C9_ID,
	BLM_DOC_TYPE_C11_ID,
	BLM_DOC_TYPE_C12_ID,
	BLM_DOC_TYPE_C13_ID,
	BLM_DOC_TYPE_C15_ID,
	BLM_DOC_TYPE_C16_ID,
} from './document-type.data.js';

export const data: Array<UmbMockDocumentModel> = [
	// -------------------------------------------------------------------------
	// Existing documents
	// -------------------------------------------------------------------------
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
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'da',
				segment: null,
				name: 'Varierende Dokument',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'variant-documents-variant-document-da',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'es',
				segment: null,
				name: 'Documento Variante',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'variant-documents-variant-document-es',
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
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'variantText',
				culture: 'es',
				segment: null,
				value: 'Este es el texto variante en español.',
			},
		],
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — parent container document
	// -------------------------------------------------------------------------
	{
		id: 'blm-doc-block-list-matrix',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: null,
		ancestors: [],
		documentType: {
			id: INVARIANT_DOCUMENT_TYPE_ID,
			icon: 'icon-document',
		},
		hasChildren: true,
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
				name: 'Block List Matrix',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-block-list-matrix-variant',
				flags: [],
			},
		],
		values: [],
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — Config #1: Doc=I, BL=I, Block=I, Text=I
	// Everything is shared across all cultures. One version of content.
	// -------------------------------------------------------------------------
	{
		id: 'blm-doc-c1',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: { id: 'blm-doc-block-list-matrix' },
		ancestors: [],
		documentType: {
			id: BLM_DOC_TYPE_C1_ID,
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
				name: '#1: All Invariant',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c1-variant',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: null,
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c1-block-1' }],
					},
					contentData: [
						{
							key: 'blm-c1-block-1',
							contentTypeKey: BLM_ET_INVARIANT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: null,
									segment: null,
									value: 'Invariant block text — shared across all cultures.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c1-block-1', culture: null, segment: null },
					],
				},
			},
		],
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — Config #9: Doc=V, BL=I, Block=I, Text=I
	// Document varies by culture. Block list and its content are fully shared.
	// -------------------------------------------------------------------------
	{
		id: 'blm-doc-c9',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: { id: 'blm-doc-block-list-matrix' },
		ancestors: [],
		documentType: {
			id: BLM_DOC_TYPE_C9_ID,
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
				name: '#9: Variant Doc, Shared Block List (en-US)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c9-en-us',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'da',
				segment: null,
				name: '#9: Varierende Dokument, Delt Blok Liste (da)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c9-da',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'es',
				segment: null,
				name: '#9: Documento Variante, Lista de Bloques Compartida (es)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c9-es',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: null,
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c9-block-1' }],
					},
					contentData: [
						{
							key: 'blm-c9-block-1',
							contentTypeKey: BLM_ET_INVARIANT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: null,
									segment: null,
									value: 'Invariant block text — same for all cultures.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c9-block-1', culture: null, segment: null },
					],
				},
			},
		],
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — Config #11: Doc=V, BL=I, Block=V, Text=I
	// Document varies. Block list is shared. Block element type is variant but
	// text property is invariant — text is the same across all cultures.
	// -------------------------------------------------------------------------
	{
		id: 'blm-doc-c11',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: { id: 'blm-doc-block-list-matrix' },
		ancestors: [],
		documentType: {
			id: BLM_DOC_TYPE_C11_ID,
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
				name: '#11: Shared Block List, Variant Element, Inv Text (en-US)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c11-en-us',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'da',
				segment: null,
				name: '#11: Delt Blok Liste, Varierende Element, Inv Tekst (da)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c11-da',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'es',
				segment: null,
				name: '#11: Lista de Bloques Compartida, Elemento Variante, Texto Inv (es)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c11-es',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: null,
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c11-block-1' }],
					},
					contentData: [
						{
							key: 'blm-c11-block-1',
							contentTypeKey: BLM_ET_VARIANT_INVARIANT_TEXT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: null,
									segment: null,
									value: 'Invariant text inside a variant block — same for all cultures.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c11-block-1', culture: 'en-US', segment: null },
						{ contentKey: 'blm-c11-block-1', culture: 'da', segment: null },
						{ contentKey: 'blm-c11-block-1', culture: 'es', segment: null },
					],
				},
			},
		],
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — Config #12: Doc=V, BL=I, Block=V, Text=V
	// Document varies. Block list is shared. Text inside each block varies by
	// culture — same layout, culture-specific copy.
	// -------------------------------------------------------------------------
	{
		id: 'blm-doc-c12',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: { id: 'blm-doc-block-list-matrix' },
		ancestors: [],
		documentType: {
			id: BLM_DOC_TYPE_C12_ID,
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
				name: '#12: Shared Block List, Variant Text (en-US)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c12-en-us',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'da',
				segment: null,
				name: '#12: Delt Blok Liste, Varierende Tekst (da)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c12-da',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'es',
				segment: null,
				name: '#12: Lista de Bloques Compartida, Texto Variante (es)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c12-es',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: null,
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c12-block-1' }],
					},
					contentData: [
						{
							key: 'blm-c12-block-1',
							contentTypeKey: BLM_ET_VARIANT_VARIANT_TEXT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: 'en-US',
									segment: null,
									value: 'English text inside the shared block.',
								},
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: 'da',
									segment: null,
									value: 'Dansk tekst inde i den delte blok.',
								},
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: 'es',
									segment: null,
									value: 'Texto en español dentro del bloque compartido.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c12-block-1', culture: 'en-US', segment: null },
						{ contentKey: 'blm-c12-block-1', culture: 'da', segment: null },
						{ contentKey: 'blm-c12-block-1', culture: 'es', segment: null },
					],
				},
			},
		],
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — Config #13: Doc=V, BL=V, Block=I, Text=I
	// Document varies. Each culture has its own block list. Text inside blocks
	// is invariant — culture-specific structure, shared block content.
	// -------------------------------------------------------------------------
	{
		id: 'blm-doc-c13',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: { id: 'blm-doc-block-list-matrix' },
		ancestors: [],
		documentType: {
			id: BLM_DOC_TYPE_C13_ID,
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
				name: '#13: Variant Block List, Invariant Element Content (en-US)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c13-en-us',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'da',
				segment: null,
				name: '#13: Varierende Blok Liste, Invariant Element Indhold (da)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c13-da',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'es',
				segment: null,
				name: '#13: Lista de Bloques Variante, Contenido de Elemento Invariante (es)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c13-es',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: 'en-US',
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c13-block-en' }],
					},
					contentData: [
						{
							key: 'blm-c13-block-en',
							contentTypeKey: BLM_ET_INVARIANT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: null,
									segment: null,
									value: 'Block text for the English block list.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c13-block-en', culture: null, segment: null },
					],
				},
			},
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: 'da',
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c13-block-da' }],
					},
					contentData: [
						{
							key: 'blm-c13-block-da',
							contentTypeKey: BLM_ET_INVARIANT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: null,
									segment: null,
									value: 'Blok tekst til den danske blok liste.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c13-block-da', culture: null, segment: null },
					],
				},
			},
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: 'es',
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c13-block-es' }],
					},
					contentData: [
						{
							key: 'blm-c13-block-es',
							contentTypeKey: BLM_ET_INVARIANT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: null,
									segment: null,
									value: 'Texto del bloque para la lista de bloques en español.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c13-block-es', culture: null, segment: null },
					],
				},
			},
		],
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — Config #15: Doc=V, BL=V, Block=V, Text=I
	// Document varies. Each culture has its own block list. Block element type
	// is variant but text is invariant — separate lists, shared block content.
	// -------------------------------------------------------------------------
	{
		id: 'blm-doc-c15',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: { id: 'blm-doc-block-list-matrix' },
		ancestors: [],
		documentType: {
			id: BLM_DOC_TYPE_C15_ID,
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
				name: '#15: Variant Block List, Variant Element, Inv Text (en-US)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c15-en-us',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'da',
				segment: null,
				name: '#15: Varierende Blok Liste, Varierende Element, Inv Tekst (da)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c15-da',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'es',
				segment: null,
				name: '#15: Lista de Bloques Variante, Elemento Variante, Texto Inv (es)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c15-es',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: 'en-US',
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c15-block-en' }],
					},
					contentData: [
						{
							key: 'blm-c15-block-en',
							contentTypeKey: BLM_ET_VARIANT_INVARIANT_TEXT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: null,
									segment: null,
									value: 'Invariant block text in the English block list.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c15-block-en', culture: 'en-US', segment: null },
					],
				},
			},
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: 'da',
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c15-block-da' }],
					},
					contentData: [
						{
							key: 'blm-c15-block-da',
							contentTypeKey: BLM_ET_VARIANT_INVARIANT_TEXT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: null,
									segment: null,
									value: 'Invariant blok tekst i den danske blok liste.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c15-block-da', culture: 'da', segment: null },
					],
				},
			},
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: 'es',
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c15-block-es' }],
					},
					contentData: [
						{
							key: 'blm-c15-block-es',
							contentTypeKey: BLM_ET_VARIANT_INVARIANT_TEXT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: null,
									segment: null,
									value: 'Texto invariante del bloque en la lista de bloques en español.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c15-block-es', culture: 'es', segment: null },
					],
				},
			},
		],
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — Config #16: Doc=V, BL=V, Block=V, Text=V
	// Fully variant. Each culture has its own block list and its own text
	// content inside each block.
	// -------------------------------------------------------------------------
	{
		id: 'blm-doc-c16',
		createDate: '2024-01-15T10:00:00.000Z',
		parent: { id: 'blm-doc-block-list-matrix' },
		ancestors: [],
		documentType: {
			id: BLM_DOC_TYPE_C16_ID,
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
				name: '#16: Fully Variant (en-US)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c16-en-us',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'da',
				segment: null,
				name: '#16: Fuldt Varierende (da)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c16-da',
				flags: [],
			},
			{
				state: DocumentVariantStateModel.PUBLISHED,
				publishDate: '2024-01-15T10:05:00.000Z',
				culture: 'es',
				segment: null,
				name: '#16: Completamente Variante (es)',
				createDate: '2024-01-15T10:00:00.000Z',
				updateDate: '2024-01-15T10:05:00.000Z',
				id: 'blm-doc-c16-es',
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: 'en-US',
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c16-block-en' }],
					},
					contentData: [
						{
							key: 'blm-c16-block-en',
							contentTypeKey: BLM_ET_VARIANT_VARIANT_TEXT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: 'en-US',
									segment: null,
									value: 'English text in the English block list.',
								},
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: 'da',
									segment: null,
									value: 'Dansk tekst i den engelske blok liste.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c16-block-en', culture: 'en-US', segment: null },
					],
				},
			},
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: 'da',
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c16-block-da' }],
					},
					contentData: [
						{
							key: 'blm-c16-block-da',
							contentTypeKey: BLM_ET_VARIANT_VARIANT_TEXT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: 'en-US',
									segment: null,
									value: 'English text in the Danish block list.',
								},
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: 'da',
									segment: null,
									value: 'Dansk tekst i den danske blok liste.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c16-block-da', culture: 'da', segment: null },
					],
				},
			},
			{
				editorAlias: 'Umbraco.BlockList',
				alias: 'blockList',
				culture: 'es',
				segment: null,
				value: {
					layout: {
						'Umbraco.BlockList': [{ contentKey: 'blm-c16-block-es' }],
					},
					contentData: [
						{
							key: 'blm-c16-block-es',
							contentTypeKey: BLM_ET_VARIANT_VARIANT_TEXT_ID,
							values: [
								{
									editorAlias: 'Umbraco.TextBox',
									alias: 'text',
									culture: 'es',
									segment: null,
									value: 'Texto en español en la lista de bloques en español.',
								},
							],
						},
					],
					settingsData: [],
					expose: [
						{ contentKey: 'blm-c16-block-es', culture: 'es', segment: null },
					],
				},
			},
		],
		flags: [],
	},
];
