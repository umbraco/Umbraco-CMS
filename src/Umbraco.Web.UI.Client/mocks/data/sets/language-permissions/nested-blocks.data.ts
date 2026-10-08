import type {
	UmbMockDataTypeModel,
	UmbMockDocumentModel,
	UmbMockDocumentTypeModel,
	UmbMockElementModel,
} from '../../mock-data-set.types.js';
import { TEXTSTRING_DATA_TYPE_ID } from './data-type.data.js';
import { DANISH_ISO_CODE, ENGLISH_ISO_CODE, VIETNAMESE_ISO_CODE } from './language.data.js';
import {
	describeVariance,
	getNestedBlocksTitle,
	NESTED_BLOCKS_SCENARIOS,
	type UmbNestedBlocksScenario,
} from './nested-blocks-scenarios.js';
import { UmbElementVariantState } from '@umbraco-cms/backoffice/element';
import type { DocumentVariantResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

const languages = [
	{ isoCode: ENGLISH_ISO_CODE, name: 'English' },
	{ isoCode: DANISH_ISO_CODE, name: 'Danish' },
	{ isoCode: VIETNAMESE_ISO_CODE, name: 'Vietnamese' },
];

const SHARED_TEXT = 'Shared text (the same in every language)';

// ---------------------------------------------------------------------------------------------------------------------
// Ids and names
// ---------------------------------------------------------------------------------------------------------------------

const idOf = (scenario: UmbNestedBlocksScenario, what: string) =>
	`language-permissions-nested-blocks-${scenario.number}-${what}-id`;

const keyOf = (scenario: UmbNestedBlocksScenario, block: 'outer-block' | 'inner-block', culture: string | null) =>
	`language-permissions-nested-blocks-${scenario.number}-${block}-${culture ?? 'shared'}-key`;

const languageName = (isoCode: string) => languages.find((language) => language.isoCode === isoCode)?.name ?? isoCode;

// ---------------------------------------------------------------------------------------------------------------------
// Data types: one outer and one inner block list per scenario
// ---------------------------------------------------------------------------------------------------------------------

/**
 *
 * @param id
 * @param name
 * @param elementTypeId
 * @param blockLabel
 */
function createBlockListDataType(
	id: string,
	name: string,
	elementTypeId: string,
	blockLabel: string,
): UmbMockDataTypeModel {
	return {
		id,
		parent: null,
		name,
		editorAlias: 'Umbraco.BlockList',
		editorUiAlias: 'Umb.PropertyEditorUi.BlockList',
		hasChildren: false,
		noAccess: false,
		isFolder: false,
		isDeletable: true,
		canIgnoreStartNodes: false,
		flags: [],
		values: [
			{
				alias: 'blocks',
				value: [
					{
						contentElementTypeKey: elementTypeId,
						label: blockLabel,
						editorSize: 'medium',
						forceHideContentEditorInOverlay: false,
					},
				],
			},
			{ alias: 'validationLimit', value: {} },
			{ alias: 'useSingleBlockMode', value: false },
			{ alias: 'useLiveEditing', value: false },
			{ alias: 'useInlineEditingAsDefault', value: false },
		],
	};
}

// ---------------------------------------------------------------------------------------------------------------------
// Document types: the document type and the two element types of every scenario
// ---------------------------------------------------------------------------------------------------------------------

const noValidation = { mandatory: false, mandatoryMessage: null, regEx: null, regExMessage: null };

const typeDefaults = {
	description: null,
	allowedTemplates: [],
	defaultTemplate: null,
	allowedInLibrary: false,
	variesBySegment: false,
	hasChildren: false,
	parent: null,
	isFolder: false,
	containers: [],
	allowedDocumentTypes: [],
	compositions: [],
	cleanup: { preventCleanup: false, keepAllVersionsNewerThanDays: null, keepLatestVersionPerDayForDays: null },
	flags: [],
	noAccess: false,
};

/**
 *
 * @param args
 * @param args.id
 * @param args.alias
 * @param args.label
 * @param args.dataTypeId
 * @param args.variesByLanguage
 */
function createProperty(args: {
	id: string;
	alias: string;
	label: string;
	dataTypeId: string;
	variesByLanguage: boolean;
}): UmbMockDocumentTypeModel['properties'][number] {
	return {
		id: args.id,
		container: null,
		alias: args.alias,
		name: `${args.label} (${describeVariance(args.variesByLanguage)})`,
		description: args.variesByLanguage
			? 'Varies by language: every language has its own value.'
			: 'Does not vary by language: one value that is shared by all languages.',
		dataType: { id: args.dataTypeId },
		variesByCulture: args.variesByLanguage,
		variesBySegment: false,
		sortOrder: 0,
		validation: noValidation,
		appearance: { labelOnTop: false },
	};
}

/**
 *
 * @param scenario
 */
function createDocumentTypes(scenario: UmbNestedBlocksScenario): Array<UmbMockDocumentTypeModel> {
	const title = getNestedBlocksTitle(scenario);

	const innerBlockElementType: UmbMockDocumentTypeModel = {
		...typeDefaults,
		id: idOf(scenario, 'inner-block-element-type'),
		alias: `nestedBlocks${scenario.number}InnerBlock`,
		name: `Nested blocks ${scenario.number} - inner block (${describeVariance(scenario.innerBlock)})`,
		icon: 'icon-science',
		allowedAsRoot: false,
		isElement: true,
		variesByCulture: scenario.innerBlock,
		properties: [
			createProperty({
				id: idOf(scenario, 'text-property'),
				alias: 'text',
				label: 'Text',
				dataTypeId: TEXTSTRING_DATA_TYPE_ID,
				variesByLanguage: scenario.text,
			}),
		],
	};

	const outerBlockElementType: UmbMockDocumentTypeModel = {
		...typeDefaults,
		id: idOf(scenario, 'outer-block-element-type'),
		alias: `nestedBlocks${scenario.number}OuterBlock`,
		name: `Nested blocks ${scenario.number} - outer block (${describeVariance(scenario.outerBlock)})`,
		icon: 'icon-science',
		allowedAsRoot: false,
		isElement: true,
		variesByCulture: scenario.outerBlock,
		properties: [
			createProperty({
				id: idOf(scenario, 'inner-list-property'),
				alias: 'innerBlockList',
				label: 'Inner list',
				dataTypeId: idOf(scenario, 'inner-list-data-type'),
				variesByLanguage: scenario.innerList,
			}),
		],
	};

	const documentType: UmbMockDocumentTypeModel = {
		...typeDefaults,
		id: idOf(scenario, 'document-type'),
		alias: `nestedBlocks${scenario.number}`,
		name: title,
		icon: 'icon-document',
		allowedAsRoot: true,
		isElement: false,
		variesByCulture: true,
		properties: [
			createProperty({
				id: idOf(scenario, 'outer-list-property'),
				alias: 'outerBlockList',
				label: 'Outer list',
				dataTypeId: idOf(scenario, 'outer-list-data-type'),
				variesByLanguage: scenario.outerList,
			}),
		],
	};

	return [documentType, outerBlockElementType, innerBlockElementType];
}

// ---------------------------------------------------------------------------------------------------------------------
// Documents: every scenario has one block in every level, in every language
// ---------------------------------------------------------------------------------------------------------------------

/**
 * The cultures a property has a value for. A property that does not vary has one value without a culture.
 * A property that varies has a value for every language, or only for the language of the list it is in.
 * @param variesByLanguage
 * @param listCulture
 */
function getValueCultures(variesByLanguage: boolean, listCulture: string | null): Array<string | null> {
	if (!variesByLanguage) return [null];
	return listCulture ? [listCulture] : languages.map((language) => language.isoCode);
}

/**
 * An element that varies is exposed in every language it is used in. An element that does not vary is exposed once.
 * @param contentKey
 * @param elementVaries
 * @param listCulture
 */
function createExposures(contentKey: string, elementVaries: boolean, listCulture: string | null) {
	return getValueCultures(elementVaries, listCulture).map((culture) => ({ contentKey, culture, segment: null }));
}

/**
 *
 * @param args
 * @param args.elementTypeId
 * @param args.key
 * @param args.elementVaries
 * @param args.listCulture
 * @param args.values
 */
function createBlockListValue(args: {
	elementTypeId: string;
	key: string;
	elementVaries: boolean;
	listCulture: string | null;
	values: Array<unknown>;
}) {
	return {
		contentData: [{ contentTypeKey: args.elementTypeId, key: args.key, values: args.values }],
		settingsData: [],
		expose: createExposures(args.key, args.elementVaries, args.listCulture),
		layout: { 'Umbraco.BlockList': [{ key: args.key, contentKey: args.key, settingsKey: null }] },
	};
}

/**
 *
 * @param scenario
 * @param listCulture
 */
function createTextValues(scenario: UmbNestedBlocksScenario, listCulture: string | null) {
	return getValueCultures(scenario.text, listCulture).map((culture) => ({
		editorAlias: 'Umbraco.TextBox',
		alias: 'text',
		culture,
		segment: null,
		value: culture ? `${languageName(culture)} text (only used in ${languageName(culture)})` : SHARED_TEXT,
	}));
}

/**
 * The values of the inner list in the outer block. `outerListCulture` is the language of the outer list it is in, if it varies.
 * @param scenario
 * @param outerListCulture
 */
function createInnerListValues(scenario: UmbNestedBlocksScenario, outerListCulture: string | null) {
	return getValueCultures(scenario.innerList, outerListCulture).map((culture) => {
		const innerListCulture = culture ?? outerListCulture;
		return {
			editorAlias: 'Umbraco.BlockList',
			alias: 'innerBlockList',
			culture,
			segment: null,
			value: createBlockListValue({
				elementTypeId: idOf(scenario, 'inner-block-element-type'),
				key: keyOf(scenario, 'inner-block', innerListCulture),
				elementVaries: scenario.innerBlock,
				listCulture: innerListCulture,
				values: createTextValues(scenario, innerListCulture),
			}),
		};
	});
}

/**
 *
 * @param scenario
 */
function createOuterListValues(scenario: UmbNestedBlocksScenario) {
	return getValueCultures(scenario.outerList, null).map((culture) => ({
		editorAlias: 'Umbraco.BlockList',
		alias: 'outerBlockList',
		culture,
		segment: null,
		value: createBlockListValue({
			elementTypeId: idOf(scenario, 'outer-block-element-type'),
			key: keyOf(scenario, 'outer-block', culture),
			elementVaries: scenario.outerBlock,
			listCulture: culture,
			values: createInnerListValues(scenario, culture),
		}),
	}));
}

const createDate = '2024-01-15T10:00:00.000Z';
const publishDate = '2024-01-15T10:05:00.000Z';

/**
 *
 * @param scenario
 */
function createDocument(scenario: UmbNestedBlocksScenario): UmbMockDocumentModel {
	const documentId = idOf(scenario, 'document');
	return {
		id: documentId,
		createDate,
		parent: null,
		ancestors: [],
		documentType: { id: idOf(scenario, 'document-type'), icon: 'icon-document' },
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		template: null,
		flags: [],
		variants: languages.map((language) => ({
			state: 'Published' as UmbDocumentVariantState,
			publishDate,
			culture: language.isoCode,
			segment: null,
			name: `${getNestedBlocksTitle(scenario)} - ${language.name}`,
			createDate,
			updateDate: publishDate,
			id: `${documentId}-${language.isoCode}`,
			flags: [],
		})),
		values: createOuterListValues(scenario),
	} as UmbMockDocumentModel;
}

// ---------------------------------------------------------------------------------------------------------------------

export const dataTypes: Array<UmbMockDataTypeModel> = NESTED_BLOCKS_SCENARIOS.flatMap((scenario) => [
	createBlockListDataType(
		idOf(scenario, 'outer-list-data-type'),
		`Nested blocks ${scenario.number} - outer list`,
		idOf(scenario, 'outer-block-element-type'),
		`Outer block (${describeVariance(scenario.outerBlock)})`,
	),
	createBlockListDataType(
		idOf(scenario, 'inner-list-data-type'),
		`Nested blocks ${scenario.number} - inner list`,
		idOf(scenario, 'inner-block-element-type'),
		`Inner block (${describeVariance(scenario.innerBlock)}): {umbValue:text}`,
	),
]);

export const documentTypes: Array<UmbMockDocumentTypeModel> = NESTED_BLOCKS_SCENARIOS.flatMap(createDocumentTypes);

export const documents: Array<UmbMockDocumentModel> = NESTED_BLOCKS_SCENARIOS.map(createDocument);

// ---------------------------------------------------------------------------------------------------------------------
// Elements: the same nested blocks, with an element (in the Library) as the owner instead of a document
// ---------------------------------------------------------------------------------------------------------------------

const elementTitle = (scenario: UmbNestedBlocksScenario) =>
	getNestedBlocksTitle(scenario).replace('Nested blocks', 'Element with nested blocks');

/**
 *
 * @param scenario
 */
function createElementType(scenario: UmbNestedBlocksScenario): UmbMockDocumentTypeModel {
	return {
		...typeDefaults,
		id: idOf(scenario, 'element-owner-element-type'),
		alias: `elementWithNestedBlocks${scenario.number}`,
		name: elementTitle(scenario),
		icon: 'icon-brick',
		allowedAsRoot: false,
		allowedInLibrary: true,
		isElement: true,
		variesByCulture: true,
		properties: [
			createProperty({
				id: idOf(scenario, 'element-owner-outer-list-property'),
				alias: 'outerBlockList',
				label: 'Outer list',
				dataTypeId: idOf(scenario, 'outer-list-data-type'),
				variesByLanguage: scenario.outerList,
			}),
		],
	};
}

/**
 *
 * @param scenario
 */
function createElement(scenario: UmbNestedBlocksScenario): UmbMockElementModel {
	const elementId = idOf(scenario, 'element-owner');
	return {
		id: elementId,
		name: elementTitle(scenario),
		createDate,
		parent: null,
		ancestors: [],
		documentType: { id: idOf(scenario, 'element-owner-element-type'), icon: 'icon-brick' },
		hasChildren: false,
		isTrashed: false,
		isFolder: false,
		flags: [],
		noAccess: false,
		variants: languages.map((language) => ({
			state: UmbElementVariantState.PUBLISHED,
			publishDate,
			culture: language.isoCode,
			name: `${elementTitle(scenario)} - ${language.name}`,
			createDate,
			updateDate: publishDate,
			id: `${elementId}-${language.isoCode}`,
			flags: [],
		})),
		values: createOuterListValues(scenario),
	} as UmbMockElementModel;
}

export const elementTypes: Array<UmbMockDocumentTypeModel> = NESTED_BLOCKS_SCENARIOS.map(createElementType);

export const elements: Array<UmbMockElementModel> = NESTED_BLOCKS_SCENARIOS.map(createElement);
