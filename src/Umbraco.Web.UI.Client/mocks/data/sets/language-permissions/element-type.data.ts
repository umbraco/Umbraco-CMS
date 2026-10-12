import type { UmbMockDocumentTypeModel } from '../../mock-data-set.types.js';
import { TEXTSTRING_DATA_TYPE_ID } from './data-type.data.js';

export const ELEMENT_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_ELEMENT_TYPE_ID =
	'language-permissions-element-with-text-per-language-and-shared-text-element-type-id';
export const ELEMENT_THAT_DOES_NOT_VARY_BY_LANGUAGE_ELEMENT_TYPE_ID =
	'language-permissions-element-that-does-not-vary-by-language-element-type-id';

const noValidation = {
	mandatory: false,
	mandatoryMessage: null,
	regEx: null,
	regExMessage: null,
};

const elementTypeDefaults = {
	description: null,
	icon: 'icon-brick',
	allowedTemplates: [],
	defaultTemplate: null,
	allowedAsRoot: false,
	allowedInLibrary: true,
	variesBySegment: false,
	isElement: true,
	hasChildren: false,
	parent: null,
	isFolder: false,
	containers: [],
	allowedDocumentTypes: [],
	compositions: [],
	cleanup: {
		preventCleanup: false,
		keepAllVersionsNewerThanDays: null,
		keepLatestVersionPerDayForDays: null,
	},
	flags: [],
	noAccess: false,
};

export const data: Array<UmbMockDocumentTypeModel> = [
	{
		...elementTypeDefaults,
		id: ELEMENT_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_ELEMENT_TYPE_ID,
		alias: 'elementWithTextPerLanguageAndSharedText',
		name: 'Element with text per language and shared text',
		variesByCulture: true,
		properties: [
			{
				id: 'language-permissions-element-text-per-language-property-id',
				container: null,
				alias: 'textPerLanguage',
				name: 'Text per language',
				description: 'Varies by language: every language has its own value.',
				dataType: { id: TEXTSTRING_DATA_TYPE_ID },
				variesByCulture: true,
				variesBySegment: false,
				sortOrder: 0,
				validation: noValidation,
				appearance: { labelOnTop: false },
			},
			{
				id: 'language-permissions-element-shared-text-property-id',
				container: null,
				alias: 'sharedText',
				name: 'Shared text',
				description: 'Does not vary by language: one value that is shared by all languages.',
				dataType: { id: TEXTSTRING_DATA_TYPE_ID },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 1,
				validation: noValidation,
				appearance: { labelOnTop: false },
			},
		],
	},
	{
		...elementTypeDefaults,
		id: ELEMENT_THAT_DOES_NOT_VARY_BY_LANGUAGE_ELEMENT_TYPE_ID,
		alias: 'elementThatDoesNotVaryByLanguage',
		name: 'Element that does not vary by language',
		variesByCulture: false,
		properties: [
			{
				id: 'language-permissions-element-text-of-element-that-does-not-vary-by-language-property-id',
				container: null,
				alias: 'text',
				name: 'Text',
				description: 'This element does not vary by language, so there is only one version of this text.',
				dataType: { id: TEXTSTRING_DATA_TYPE_ID },
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: noValidation,
				appearance: { labelOnTop: false },
			},
		],
	},
];
