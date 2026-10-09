/**
 * The 18 ways the levels of a nested block editor can vary by language. A document with a block list (outer list)
 * holds an outer block, which has a block list (inner list), which holds an inner block, which has a text.
 * Every level either varies by language or is shared by all languages.
 * The combinations are the same as the ones in the acceptance tests (BlockEditabilityMatrix).
 */
export interface UmbNestedBlocksScenario {
	/** Two digits, for example "07" */
	number: string;
	/** true = varies by language, false = shared by all languages */
	outerList: boolean;
	outerBlock: boolean;
	innerList: boolean;
	innerBlock: boolean;
	text: boolean;
}

const varies = true;
const shared = false;

// A level can only vary when the level it belongs to varies: the inner list needs a varying outer block, and the text needs a varying inner block.
const combinations: Array<[boolean, boolean, boolean, boolean, boolean]> = [
	// outer list, outer block, inner list, inner block, text
	[varies, varies, varies, varies, varies],
	[varies, varies, varies, varies, shared],
	[varies, varies, varies, shared, shared],
	[varies, varies, shared, varies, varies],
	[varies, varies, shared, varies, shared],
	[varies, varies, shared, shared, shared],
	[varies, shared, shared, varies, varies],
	[varies, shared, shared, varies, shared],
	[varies, shared, shared, shared, shared],
	[shared, varies, varies, varies, varies],
	[shared, varies, varies, varies, shared],
	[shared, varies, varies, shared, shared],
	[shared, varies, shared, varies, varies],
	[shared, varies, shared, varies, shared],
	[shared, varies, shared, shared, shared],
	[shared, shared, shared, varies, varies],
	[shared, shared, shared, varies, shared],
	[shared, shared, shared, shared, shared],
];

export const NESTED_BLOCKS_SCENARIOS: Array<UmbNestedBlocksScenario> = combinations.map(
	([outerList, outerBlock, innerList, innerBlock, text], index) => ({
		number: String(index + 1).padStart(2, '0'),
		outerList,
		outerBlock,
		innerList,
		innerBlock,
		text,
	}),
);

export const describeVariance = (variesByLanguage: boolean) => (variesByLanguage ? 'varies by language' : 'shared');

/**
 * Names of the levels that are shared, in the order they are nested
 * @param scenario
 */
function getSharedLevels(scenario: UmbNestedBlocksScenario): Array<string> {
	return [
		scenario.outerList ? null : 'outer list',
		scenario.outerBlock ? null : 'outer block',
		scenario.innerList ? null : 'inner list',
		scenario.innerBlock ? null : 'inner block',
		scenario.text ? null : 'text',
	].filter((level): level is string => level !== null);
}

/**
 * Names the scenario by what is shared, for example "Nested blocks 12 - shared: outer list, inner block and text".
 * Everything that is not listed varies by language.
 * @param scenario
 */
export function getNestedBlocksTitle(scenario: UmbNestedBlocksScenario): string {
	const sharedLevels = getSharedLevels(scenario);
	const prefix = `Nested blocks ${scenario.number}`;

	if (sharedLevels.length === 0) return `${prefix} - nothing is shared (everything varies by language)`;
	if (sharedLevels.length === 5) return `${prefix} - everything is shared (nothing varies by language)`;

	const last = sharedLevels[sharedLevels.length - 1];
	const listed = sharedLevels.length === 1 ? last : `${sharedLevels.slice(0, -1).join(', ')} and ${last}`;
	return `${prefix} - shared: ${listed}`;
}
