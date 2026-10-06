import type { UmbMockDocumentTypeModel } from '../../mock-data-set.types.js';
import { TEXTSTRING_DATA_TYPE_ID } from './data-type.data.js';

export const PAGE_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_DOCUMENT_TYPE_ID =
	'document-language-permissions-page-with-text-per-language-and-shared-text-document-type-id';
export const PAGE_THAT_DOES_NOT_VARY_BY_LANGUAGE_DOCUMENT_TYPE_ID =
	'document-language-permissions-page-that-does-not-vary-by-language-document-type-id';

const noValidation = {
	mandatory: false,
	mandatoryMessage: null,
	regEx: null,
	regExMessage: null,
};

const documentTypeDefaults = {
	description: null,
	icon: 'icon-document',
	allowedTemplates: [],
	defaultTemplate: null,
	allowedAsRoot: true,
	allowedInLibrary: false,
	variesBySegment: false,
	isElement: false,
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
		...documentTypeDefaults,
		id: PAGE_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_DOCUMENT_TYPE_ID,
		alias: 'pageWithTextPerLanguageAndSharedText',
		name: 'Page with text per language and shared text',
		variesByCulture: true,
		properties: [
			{
				id: 'document-language-permissions-text-per-language-property-id',
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
				id: 'document-language-permissions-shared-text-property-id',
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
		...documentTypeDefaults,
		id: PAGE_THAT_DOES_NOT_VARY_BY_LANGUAGE_DOCUMENT_TYPE_ID,
		alias: 'pageThatDoesNotVaryByLanguage',
		name: 'Page that does not vary by language',
		variesByCulture: false,
		properties: [
			{
				id: 'document-language-permissions-text-of-page-that-does-not-vary-by-language-property-id',
				container: null,
				alias: 'text',
				name: 'Text',
				description: 'This page does not vary by language, so there is only one version of this text.',
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
