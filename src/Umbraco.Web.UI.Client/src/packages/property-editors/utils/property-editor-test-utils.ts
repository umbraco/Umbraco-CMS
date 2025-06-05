import { expect } from '@open-wc/testing';

/**
 * Shared test utilities for property editor components
 */

/**
 * Helper function to setup basic string array configuration
 * @param {{ config?: unknown }} element - The property editor element to configure
 * @param {unknown} element.config - Configuration object to set
 * @param {string[]} items - Array of string items for configuration
 */
export function setupBasicStringConfig(element: { config?: unknown }, items: string[] = ['Red', 'Green', 'Blue']) {
	element.config = {
		getValueByAlias: (alias: string) => {
			if (alias === 'items') {
				return items;
			}
			return undefined;
		},
	} as { getValueByAlias: (alias: string) => unknown };
}

/**
 * Helper function to setup object array configuration
 * @param {{ config?: unknown }} element - The property editor element to configure
 * @param {unknown} element.config - Configuration object to set
 * @param {Array<{ name: string; value: string }>} items - Array of object items for configuration
 */
export function setupObjectConfig(
	element: { config?: unknown },
	items: Array<{ name: string; value: string }> = [
		{ name: 'Red Color', value: 'red' },
		{ name: 'Green Color', value: 'green' },
		{ name: 'Blue Color', value: 'blue' },
	],
) {
	element.config = {
		getValueByAlias: (alias: string) => {
			if (alias === 'items') {
				return items;
			}
			return undefined;
		},
	} as { getValueByAlias: (alias: string) => unknown };
}

/**
 * Helper function to setup empty configuration
 * @param {{ config?: unknown }} element - The property editor element to configure
 * @param {unknown} element.config - Configuration object to set
 */
export function setupEmptyConfig(element: { config?: unknown }) {
	element.config = {
		getValueByAlias: () => undefined,
	} as { getValueByAlias: (alias: string) => unknown };
}

/**
 * Helper function to get select element from shadow DOM
 * @param {{ shadowRoot?: ShadowRoot | null }} element - The property editor element
 * @param {ShadowRoot | null} element.shadowRoot - Shadow DOM root for querying
 * @returns {Element | null} The UUI select element or null
 */
export function getSelectElement(element: { shadowRoot?: ShadowRoot | null }) {
	return element.shadowRoot?.querySelector('uui-select');
}

/**
 * Helper function to get checkbox list element from shadow DOM
 * @param {{ shadowRoot?: ShadowRoot | null }} element - The property editor element
 * @param {ShadowRoot | null} element.shadowRoot - Shadow DOM root for querying
 * @returns {Element | null} The checkbox list element or null
 */
export function getCheckboxListElement(element: { shadowRoot?: ShadowRoot | null }) {
	return element.shadowRoot?.querySelector('umb-input-checkbox-list');
}

/**
 * Helper function to get dropdown element from shadow DOM
 * @param {{ shadowRoot?: ShadowRoot | null }} element - The property editor element
 * @param {ShadowRoot | null} element.shadowRoot - Shadow DOM root for querying
 * @returns {Element | null} The dropdown element or null
 */
export function getDropdownElement(element: { shadowRoot?: ShadowRoot | null }) {
	return element.shadowRoot?.querySelector('umb-input-dropdown-list');
}

/**
 * Helper function to get selected value from select DOM
 * @param {{ shadowRoot?: ShadowRoot | null }} element - The property editor element
 * @param {ShadowRoot | null} element.shadowRoot - Shadow DOM root for querying
 * @returns {string} The selected value string
 */
export function getSelectedValue(element: { shadowRoot?: ShadowRoot | null }) {
	const selectElement = getSelectElement(element);
	return selectElement?.value || '';
}

/**
 * Helper function to get selection from checkbox list DOM
 * @param {{ shadowRoot?: ShadowRoot | null }} element - The property editor element
 * @param {ShadowRoot | null} element.shadowRoot - Shadow DOM root for querying
 * @returns {string[]} Array of selected values
 */
export function getCheckboxSelection(element: { shadowRoot?: ShadowRoot | null }) {
	const checkboxElement = getCheckboxListElement(element);
	return checkboxElement?.selection || [];
}

/**
 * Helper function to get selection from dropdown DOM
 * @param {{ shadowRoot?: ShadowRoot | null }} element - The property editor element
 * @param {ShadowRoot | null} element.shadowRoot - Shadow DOM root for querying
 * @returns {string[]} Array of selected values
 */
export function getDropdownSelection(element: { shadowRoot?: ShadowRoot | null }) {
	const dropdownElement = getDropdownElement(element) as { value?: string } | null;
	return dropdownElement?.value ? dropdownElement.value.split(', ') : [];
}

/**
 * Helper function to verify both value and DOM state for single select
 * @param {{ value: string; shadowRoot?: ShadowRoot | null }} element - The property editor element
 * @param {string} element.value - Current value of the element
 * @param {ShadowRoot | null} element.shadowRoot - Shadow DOM root for querying
 * @param {string} expectedValue - Expected element value
 * @param {string} expectedSelected - Expected selected value in DOM
 */
export function verifySelectValueAndDOM(
	element: { value: string; shadowRoot?: ShadowRoot | null },
	expectedValue: string,
	expectedSelected: string,
) {
	expect(element.value).to.equal(expectedValue);
	expect(getSelectedValue(element)).to.equal(expectedSelected);
}

/**
 * Helper function to verify both value and DOM state for multi-select
 * @param {{ value: string[]; shadowRoot?: ShadowRoot | null }} element - The property editor element
 * @param {string[]} element.value - Current value array of the element
 * @param {ShadowRoot | null} element.shadowRoot - Shadow DOM root for querying
 * @param {string[]} expectedValue - Expected element value array
 * @param {string[]} expectedSelection - Expected selection array in DOM
 */
export function verifyMultiSelectValueAndDOM(
	element: { value: string[]; shadowRoot?: ShadowRoot | null },
	expectedValue: string[],
	expectedSelection: string[],
) {
	expect(element.value).to.deep.equal(expectedValue);
	expect(getCheckboxSelection(element)).to.deep.equal(expectedSelection);
}

/**
 * Helper function to verify both value and DOM state for dropdown
 * @param {{ value: string[]; shadowRoot?: ShadowRoot | null }} element - The property editor element
 * @param {string[]} element.value - Current value array of the element
 * @param {ShadowRoot | null} element.shadowRoot - Shadow DOM root for querying
 * @param {string[]} expectedValue - Expected element value array
 * @param {string[]} expectedSelection - Expected selection array in DOM
 */
export function verifyDropdownValueAndDOM(
	element: { value: string[]; shadowRoot?: ShadowRoot | null },
	expectedValue: string[],
	expectedSelection: string[],
) {
	expect(element.value).to.deep.equal(expectedValue);
	expect(getDropdownSelection(element)).to.deep.equal(expectedSelection);
}

/**
 * Common test data for multiple updates
 */
export const SINGLE_SELECT_TEST_DATA = [
	{ value: 'Red', expected: 'Red' },
	{ value: 'Green', expected: 'Green' },
	{ value: 'Blue', expected: 'Blue' },
	{ value: '', expected: '' },
];

export const MULTI_SELECT_TEST_DATA = [
	{ value: ['Red'], expected: ['Red'] },
	{ value: ['Red', 'Blue'], expected: ['Red', 'Blue'] },
	{ value: ['Green'], expected: ['Green'] },
	{ value: [], expected: [] },
];

// Re-export expect for convenience
export { expect };
