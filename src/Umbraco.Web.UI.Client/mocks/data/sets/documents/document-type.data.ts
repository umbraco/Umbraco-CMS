import type { UmbMockDocumentTypeModel } from '../../mock-data-set.types.js';
import {
	BLM_BL_INVARIANT_ET_DT_ID,
	BLM_BL_VARIANT_ET_INVARIANT_TEXT_DT_ID,
	BLM_BL_VARIANT_ET_VARIANT_TEXT_DT_ID,
} from './data-type.data.js';

export const INVARIANT_DOCUMENT_TYPE_ID = 'variant-documents-invariant-document-type-id';
export const VARIANT_DOCUMENT_TYPE_ID = 'variant-documents-variant-document-type-id';

export const BLM_FOLDER_ID = 'blm-doc-type-folder';
export const BLM_ELEMENTS_FOLDER_ID = 'blm-doc-type-elements-folder';
export const BLM_ET_INVARIANT_ID = 'blm-et-invariant';
export const BLM_ET_VARIANT_INVARIANT_TEXT_ID = 'blm-et-variant-invariant-text';
export const BLM_ET_VARIANT_VARIANT_TEXT_ID = 'blm-et-variant-variant-text';

export const BLM_DOC_TYPE_C1_ID = 'blm-doc-type-c1';
export const BLM_DOC_TYPE_C9_ID = 'blm-doc-type-c9';
export const BLM_DOC_TYPE_C11_ID = 'blm-doc-type-c11';
export const BLM_DOC_TYPE_C12_ID = 'blm-doc-type-c12';
export const BLM_DOC_TYPE_C13_ID = 'blm-doc-type-c13';
export const BLM_DOC_TYPE_C15_ID = 'blm-doc-type-c15';
export const BLM_DOC_TYPE_C16_ID = 'blm-doc-type-c16';

const TEXTSTRING_DT_ID = 'variant-documents-textstring-data-type-id';

export const data: Array<UmbMockDocumentTypeModel> = [
	// -------------------------------------------------------------------------
	// Existing document types
	// -------------------------------------------------------------------------
	{
		id: INVARIANT_DOCUMENT_TYPE_ID,
		alias: 'invariantDocumentType',
		name: 'Invariant Document Type',
		description: null,
		icon: 'icon-document',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: true,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: null,
		isFolder: false,
		properties: [
			{
				id: 'variant-documents-invariant-prop-text-id',
				container: null,
				alias: 'text',
				name: 'Text',
				description: null,
				dataType: { id: TEXTSTRING_DT_ID },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
	{
		id: VARIANT_DOCUMENT_TYPE_ID,
		alias: 'variantDocumentType',
		name: 'Variant Document Type',
		description: null,
		icon: 'icon-document',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: true,
		variesByCulture: true,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: null,
		isFolder: false,
		properties: [
			{
				id: 'variant-documents-variant-prop-text-id',
				container: null,
				alias: 'text',
				name: 'Text',
				description: null,
				dataType: { id: TEXTSTRING_DT_ID },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
			{
				id: 'variant-documents-variant-prop-variant-text-id',
				container: null,
				alias: 'variantText',
				name: 'Variant Text',
				description: null,
				dataType: { id: TEXTSTRING_DT_ID },
				variesByCulture: true,
				variesBySegment: false,
				sortOrder: 1,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — folder
	// -------------------------------------------------------------------------
	{
		id: BLM_FOLDER_ID,
		alias: 'blmBlockListMatrix',
		name: 'Block List Matrix',
		description: null,
		icon: 'icon-folder',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: true,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: true,
		parent: null,
		isFolder: true,
		properties: [],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — elements folder
	// -------------------------------------------------------------------------
	{
		id: BLM_ELEMENTS_FOLDER_ID,
		alias: 'blmElements',
		name: 'Elements',
		description: null,
		icon: 'icon-folder',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: true,
		parent: { id: BLM_FOLDER_ID },
		isFolder: true,
		properties: [],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — element types
	// -------------------------------------------------------------------------

	// Invariant element type: variesByCulture=false, text prop invariant
	{
		id: BLM_ET_INVARIANT_ID,
		alias: 'blmInvariantBlock',
		name: 'Invariant Element',
		description: null,
		icon: 'icon-brick',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: true,
		hasChildren: false,
		parent: { id: BLM_ELEMENTS_FOLDER_ID },
		isFolder: false,
		properties: [
			{
				id: 'blm-et-invariant-text-prop',
				container: null,
				alias: 'text',
				name: 'Text',
				description: null,
				dataType: { id: TEXTSTRING_DT_ID },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// Variant element type: variesByCulture=true, text prop invariant
	{
		id: BLM_ET_VARIANT_INVARIANT_TEXT_ID,
		alias: 'blmVariantBlockInvariantText',
		name: 'Variant Element (Invariant Text)',
		description: null,
		icon: 'icon-brick',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: false,
		variesByCulture: true,
		variesBySegment: false,
		isElement: true,
		hasChildren: false,
		parent: { id: BLM_ELEMENTS_FOLDER_ID },
		isFolder: false,
		properties: [
			{
				id: 'blm-et-variant-invariant-text-prop',
				container: null,
				alias: 'text',
				name: 'Text',
				description: null,
				dataType: { id: TEXTSTRING_DT_ID },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// Variant element type: variesByCulture=true, text prop variant
	{
		id: BLM_ET_VARIANT_VARIANT_TEXT_ID,
		alias: 'blmVariantBlockVariantText',
		name: 'Variant Element (Variant Text)',
		description: null,
		icon: 'icon-brick',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: false,
		variesByCulture: true,
		variesBySegment: false,
		isElement: true,
		hasChildren: false,
		parent: { id: BLM_ELEMENTS_FOLDER_ID },
		isFolder: false,
		properties: [
			{
				id: 'blm-et-variant-variant-text-prop',
				container: null,
				alias: 'text',
				name: 'Text',
				description: null,
				dataType: { id: TEXTSTRING_DT_ID },
				variesByCulture: true,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// -------------------------------------------------------------------------
	// Block list matrix — document types (one per valid config)
	// -------------------------------------------------------------------------

	// Config #1: Doc=I, BL=I, Block=I, Text=I
	{
		id: BLM_DOC_TYPE_C1_ID,
		alias: 'blmConfigC1',
		name: '#1: Inv Doc / Inv BL / Inv Element / Inv Text',
		description: null,
		icon: 'icon-document',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: { id: BLM_FOLDER_ID },
		isFolder: false,
		properties: [
			{
				id: 'blm-dt-c1-bl-prop',
				container: null,
				alias: 'blockList',
				name: 'Block List',
				description: null,
				dataType: { id: BLM_BL_INVARIANT_ET_DT_ID },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// Config #9: Doc=V, BL=I, Block=I, Text=I
	{
		id: BLM_DOC_TYPE_C9_ID,
		alias: 'blmConfigC9',
		name: '#9: Var Doc / Inv BL / Inv Element / Inv Text',
		description: null,
		icon: 'icon-document',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: false,
		variesByCulture: true,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: { id: BLM_FOLDER_ID },
		isFolder: false,
		properties: [
			{
				id: 'blm-dt-c9-bl-prop',
				container: null,
				alias: 'blockList',
				name: 'Block List',
				description: null,
				dataType: { id: BLM_BL_INVARIANT_ET_DT_ID },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// Config #11: Doc=V, BL=I, Block=V, Text=I
	{
		id: BLM_DOC_TYPE_C11_ID,
		alias: 'blmConfigC11',
		name: '#11: Var Doc / Inv BL / Var Element / Inv Text',
		description: null,
		icon: 'icon-document',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: false,
		variesByCulture: true,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: { id: BLM_FOLDER_ID },
		isFolder: false,
		properties: [
			{
				id: 'blm-dt-c11-bl-prop',
				container: null,
				alias: 'blockList',
				name: 'Block List',
				description: null,
				dataType: { id: BLM_BL_VARIANT_ET_INVARIANT_TEXT_DT_ID },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// Config #12: Doc=V, BL=I, Block=V, Text=V
	{
		id: BLM_DOC_TYPE_C12_ID,
		alias: 'blmConfigC12',
		name: '#12: Var Doc / Inv BL / Var Element / Var Text',
		description: null,
		icon: 'icon-document',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: false,
		variesByCulture: true,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: { id: BLM_FOLDER_ID },
		isFolder: false,
		properties: [
			{
				id: 'blm-dt-c12-bl-prop',
				container: null,
				alias: 'blockList',
				name: 'Block List',
				description: null,
				dataType: { id: BLM_BL_VARIANT_ET_VARIANT_TEXT_DT_ID },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// Config #13: Doc=V, BL=V, Block=I, Text=I
	{
		id: BLM_DOC_TYPE_C13_ID,
		alias: 'blmConfigC13',
		name: '#13: Var Doc / Var BL / Inv Element / Inv Text',
		description: null,
		icon: 'icon-document',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: false,
		variesByCulture: true,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: { id: BLM_FOLDER_ID },
		isFolder: false,
		properties: [
			{
				id: 'blm-dt-c13-bl-prop',
				container: null,
				alias: 'blockList',
				name: 'Block List',
				description: null,
				dataType: { id: BLM_BL_INVARIANT_ET_DT_ID },
				variesByCulture: true,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// Config #15: Doc=V, BL=V, Block=V, Text=I
	{
		id: BLM_DOC_TYPE_C15_ID,
		alias: 'blmConfigC15',
		name: '#15: Var Doc / Var BL / Var Element / Inv Text',
		description: null,
		icon: 'icon-document',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: false,
		variesByCulture: true,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: { id: BLM_FOLDER_ID },
		isFolder: false,
		properties: [
			{
				id: 'blm-dt-c15-bl-prop',
				container: null,
				alias: 'blockList',
				name: 'Block List',
				description: null,
				dataType: { id: BLM_BL_VARIANT_ET_INVARIANT_TEXT_DT_ID },
				variesByCulture: true,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},

	// Config #16: Doc=V, BL=V, Block=V, Text=V
	{
		id: BLM_DOC_TYPE_C16_ID,
		alias: 'blmConfigC16',
		name: '#16: Var Doc / Var BL / Var Element / Var Text',
		description: null,
		icon: 'icon-document',
		allowedTemplates: [],
		defaultTemplate: null,
		allowedAsRoot: false,
		variesByCulture: true,
		variesBySegment: false,
		isElement: false,
		hasChildren: false,
		parent: { id: BLM_FOLDER_ID },
		isFolder: false,
		properties: [
			{
				id: 'blm-dt-c16-bl-prop',
				container: null,
				alias: 'blockList',
				name: 'Block List',
				description: null,
				dataType: { id: BLM_BL_VARIANT_ET_VARIANT_TEXT_DT_ID },
				variesByCulture: true,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
			},
		],
		containers: [],
		allowedDocumentTypes: [],
		compositions: [],
		cleanup: {
			preventCleanup: false,
			keepAllVersionsNewerThanDays: null,
			keepLatestVersionPerDayForDays: null,
		},
		flags: [],
	},
];
