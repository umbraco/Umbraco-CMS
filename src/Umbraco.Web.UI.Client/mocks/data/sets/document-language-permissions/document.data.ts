import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import {
	PAGE_THAT_DOES_NOT_VARY_BY_LANGUAGE_DOCUMENT_TYPE_ID,
	PAGE_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_DOCUMENT_TYPE_ID,
} from './document-type.data.js';
import { DANISH_ISO_CODE, ENGLISH_ISO_CODE, VIETNAMESE_ISO_CODE } from './language.data.js';
import type { DocumentVariantResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

export const PAGE_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_DOCUMENT_ID =
	'document-language-permissions-page-with-text-per-language-and-shared-text-document-id';
export const PAGE_THAT_DOES_NOT_VARY_BY_LANGUAGE_DOCUMENT_ID =
	'document-language-permissions-page-that-does-not-vary-by-language-document-id';

const createDate = '2024-01-15T10:00:00.000Z';
const publishDate = '2024-01-15T10:05:00.000Z';

const documentDefaults = {
	createDate,
	parent: null,
	ancestors: [],
	hasChildren: false,
	noAccess: false,
	isProtected: false,
	isTrashed: false,
	template: null,
	flags: [],
};

const documentName = 'Page with text per language and shared text';

/**
 *
 * @param languageName
 * @param culture
 */
function createLanguageVariant(languageName: string, culture: string) {
	return {
		state: 'Published' as UmbDocumentVariantState,
		publishDate,
		culture,
		segment: null,
		name: `${documentName} - ${languageName}`,
		createDate,
		updateDate: publishDate,
		id: `${PAGE_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_DOCUMENT_ID}-${culture}`,
		flags: [],
	};
}

/**
 *
 * @param languageName
 * @param culture
 */
function createTextPerLanguageValue(languageName: string, culture: string) {
	return {
		editorAlias: 'Umbraco.TextBox',
		alias: 'textPerLanguage',
		culture,
		segment: null,
		value: `${languageName} text (only used in ${languageName})`,
	};
}

export const data: Array<UmbMockDocumentModel> = [
	{
		...documentDefaults,
		id: PAGE_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_DOCUMENT_ID,
		documentType: { id: PAGE_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_DOCUMENT_TYPE_ID, icon: 'icon-document' },
		variants: [
			createLanguageVariant('English', ENGLISH_ISO_CODE),
			createLanguageVariant('Danish', DANISH_ISO_CODE),
			createLanguageVariant('Vietnamese', VIETNAMESE_ISO_CODE),
		],
		values: [
			createTextPerLanguageValue('English', ENGLISH_ISO_CODE),
			createTextPerLanguageValue('Danish', DANISH_ISO_CODE),
			createTextPerLanguageValue('Vietnamese', VIETNAMESE_ISO_CODE),
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'sharedText',
				culture: null,
				segment: null,
				value: 'Shared text (the same in every language)',
			},
		],
	},
	{
		...documentDefaults,
		id: PAGE_THAT_DOES_NOT_VARY_BY_LANGUAGE_DOCUMENT_ID,
		documentType: { id: PAGE_THAT_DOES_NOT_VARY_BY_LANGUAGE_DOCUMENT_TYPE_ID, icon: 'icon-document' },
		variants: [
			{
				state: 'Published' as UmbDocumentVariantState,
				publishDate,
				culture: null,
				segment: null,
				name: 'Page that does not vary by language',
				createDate,
				updateDate: publishDate,
				id: `${PAGE_THAT_DOES_NOT_VARY_BY_LANGUAGE_DOCUMENT_ID}-invariant`,
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'text',
				culture: null,
				segment: null,
				value: 'Text (one value for the whole page)',
			},
		],
	},
];
