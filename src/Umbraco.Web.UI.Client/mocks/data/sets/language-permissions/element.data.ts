import type { UmbMockElementModel } from '../../mock-data-set.types.js';
import {
	DANISH_ISO_CODE,
	ENGLISH_ISO_CODE,
	VIETNAMESE_ISO_CODE,
} from './language.data.js';
import {
	ELEMENT_THAT_DOES_NOT_VARY_BY_LANGUAGE_ELEMENT_TYPE_ID,
	ELEMENT_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_ELEMENT_TYPE_ID,
} from './element-type.data.js';
import { UmbElementVariantState } from '@umbraco-cms/backoffice/element';

export const ELEMENT_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_ELEMENT_ID =
	'language-permissions-element-with-text-per-language-and-shared-text-element-id';
export const ELEMENT_THAT_DOES_NOT_VARY_BY_LANGUAGE_ELEMENT_ID =
	'language-permissions-element-that-does-not-vary-by-language-element-id';

const createDate = '2024-01-15T10:00:00.000Z';
const publishDate = '2024-01-15T10:05:00.000Z';

const elementDefaults = {
	createDate,
	parent: null,
	ancestors: [],
	hasChildren: false,
	isTrashed: false,
	isFolder: false,
	flags: [],
	noAccess: false,
};

const elementName = 'Element with text per language and shared text';

/**
 *
 * @param languageName
 * @param culture
 */
function createLanguageVariant(languageName: string, culture: string) {
	return {
		state: UmbElementVariantState.PUBLISHED,
		publishDate,
		culture,
		name: `${elementName} - ${languageName}`,
		createDate,
		updateDate: publishDate,
		id: `${ELEMENT_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_ELEMENT_ID}-${culture}`,
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

export const data: Array<UmbMockElementModel> = [
	{
		...elementDefaults,
		id: ELEMENT_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_ELEMENT_ID,
		name: elementName,
		documentType: { id: ELEMENT_WITH_TEXT_PER_LANGUAGE_AND_SHARED_TEXT_ELEMENT_TYPE_ID, icon: 'icon-brick' },
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
		...elementDefaults,
		id: ELEMENT_THAT_DOES_NOT_VARY_BY_LANGUAGE_ELEMENT_ID,
		name: 'Element that does not vary by language',
		documentType: { id: ELEMENT_THAT_DOES_NOT_VARY_BY_LANGUAGE_ELEMENT_TYPE_ID, icon: 'icon-brick' },
		variants: [
			{
				state: UmbElementVariantState.PUBLISHED,
				publishDate,
				culture: null,
				name: 'Element that does not vary by language',
				createDate,
				updateDate: publishDate,
				id: `${ELEMENT_THAT_DOES_NOT_VARY_BY_LANGUAGE_ELEMENT_ID}-invariant`,
				flags: [],
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'text',
				culture: null,
				segment: null,
				value: 'Text (one value for the whole element)',
			},
		],
	},
];
