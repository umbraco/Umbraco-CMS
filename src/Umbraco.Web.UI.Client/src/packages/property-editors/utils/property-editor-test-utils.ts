import { expect } from '@open-wc/testing';

/**
 * Shared test utilities for property editor components
 */

/**
 * Type definitions for better domain modeling
 */
export type UmbPropertyEditorElement = { config?: unknown };
export type UmbShadowDOMElement = { shadowRoot?: ShadowRoot | null };
export type UmbSingleSelectElement = { value: string } & UmbShadowDOMElement;
export type UmbMultiSelectElement = { value: string[] } & UmbShadowDOMElement;

export interface UmbConfigItem {
	name: string;
	value: string;
}

export interface UmbTestDataEntry<T> {
	value: T;
	expected: T;
}

export type UmbConfigAlias = 'items' | 'multiple';
export type UmbCSSSelector = 'uui-select' | 'umb-input-checkbox-list' | 'umb-input-dropdown-list';

/**
 * Helper function to setup basic string array configuration
 * @param {UmbPropertyEditorElement} element - The property editor element to configure
 * @param {string[]} items - Array of string items for configuration
 */
export function setupBasicStringConfig(element: UmbPropertyEditorElement, items: string[] = ['Red', 'Green', 'Blue']) {
	element.config = {
		getValueByAlias: (alias: UmbConfigAlias) => {
			if (alias === 'items') {
				return items;
			}
			return undefined;
		},
	} as { getValueByAlias: (alias: UmbConfigAlias) => unknown };
}

/**
 * Helper function to setup object array configuration
 * @param {UmbPropertyEditorElement} element - The property editor element to configure
 * @param {UmbConfigItem[]} items - Array of object items for configuration
 */
export function setupObjectConfig(
	element: UmbPropertyEditorElement,
	items: UmbConfigItem[] = [
		{ name: 'Red Color', value: 'red' },
		{ name: 'Green Color', value: 'green' },
		{ name: 'Blue Color', value: 'blue' },
	],
) {
	element.config = {
		getValueByAlias: (alias: UmbConfigAlias) => {
			if (alias === 'items') {
				return items;
			}
			return undefined;
		},
	} as { getValueByAlias: (alias: UmbConfigAlias) => unknown };
}

/**
 * Helper function to setup empty configuration
 * @param {UmbPropertyEditorElement} element - The property editor element to configure
 */
export function setupEmptyConfig(element: UmbPropertyEditorElement) {
	element.config = {
		getValueByAlias: () => undefined,
	} as { getValueByAlias: (alias: UmbConfigAlias) => unknown };
}

/**
 * Helper function to get select element from shadow DOM
 * @param {UmbShadowDOMElement} element - The property editor element
 * @returns {Element | null} The UUI select element or null
 */
export function getSelectElement(element: UmbShadowDOMElement) {
	return element.shadowRoot?.querySelector('uui-select' as UmbCSSSelector);
}

/**
 * Helper function to get checkbox list element from shadow DOM
 * @param {UmbShadowDOMElement} element - The property editor element
 * @returns {Element | null} The checkbox list element or null
 */
export function getCheckboxListElement(element: UmbShadowDOMElement) {
	return element.shadowRoot?.querySelector('umb-input-checkbox-list' as UmbCSSSelector);
}

/**
 * Helper function to get dropdown element from shadow DOM
 * @param {UmbShadowDOMElement} element - The property editor element
 * @returns {Element | null} The dropdown element or null
 */
export function getDropdownElement(element: UmbShadowDOMElement) {
	return element.shadowRoot?.querySelector('umb-input-dropdown-list' as UmbCSSSelector);
}

/**
 * Helper function to get selected value from select DOM
 * @param {UmbShadowDOMElement} element - The property editor element
 * @returns {string} The selected value string
 */
export function getSelectedValue(element: UmbShadowDOMElement): string {
	const selectElement = getSelectElement(element) as { value?: string } | null;
	return selectElement?.value || '';
}

/**
 * Helper function to get selection from checkbox list DOM
 * @param {UmbShadowDOMElement} element - The property editor element
 * @returns {string[]} Array of selected values
 */
export function getCheckboxSelection(element: UmbShadowDOMElement): string[] {
	const checkboxElement = getCheckboxListElement(element) as { selection?: string[] } | null;
	return checkboxElement?.selection || [];
}

/**
 * Helper function to get selection from dropdown DOM
 * @param {UmbShadowDOMElement} element - The property editor element
 * @returns {string[]} Array of selected values
 */
export function getDropdownSelection(element: UmbShadowDOMElement): string[] {
	const dropdownElement = getDropdownElement(element) as { value?: string; selection?: string[] } | null;
	// Prefer selection array if available, fallback to parsing value string
	if (dropdownElement?.selection) {
		return dropdownElement.selection;
	}
	// Fallback: parse comma-separated string (note: assumes values don't contain commas)
	return dropdownElement?.value ? dropdownElement.value.split(', ').filter((v) => v.trim().length > 0) : [];
}

/**
 * Helper function to verify both value and DOM state for single select
 * @param {UmbSingleSelectElement} element - The property editor element
 * @param {string} expectedValue - Expected element value
 * @param {string} expectedSelected - Expected selected value in DOM
 */
export function verifySelectValueAndDOM(
	element: UmbSingleSelectElement,
	expectedValue: string,
	expectedSelected: string,
) {
	expect(element.value).to.equal(expectedValue);
	expect(getSelectedValue(element)).to.equal(expectedSelected);
}

/**
 * Helper function to verify both value and DOM state for multi-select
 * @param {UmbMultiSelectElement} element - The property editor element
 * @param {string[]} expectedValue - Expected element value array
 * @param {string[]} expectedSelection - Expected selection array in DOM
 */
export function verifyMultiSelectValueAndDOM(
	element: UmbMultiSelectElement,
	expectedValue: string[],
	expectedSelection: string[],
) {
	expect(element.value).to.deep.equal(expectedValue);
	expect(getCheckboxSelection(element)).to.deep.equal(expectedSelection);
}

/**
 * Helper function to verify both value and DOM state for dropdown
 * @param {UmbMultiSelectElement} element - The property editor element
 * @param {string[]} expectedValue - Expected element value array
 * @param {string[]} expectedSelection - Expected selection array in DOM
 */
export function verifyDropdownValueAndDOM(
	element: UmbMultiSelectElement,
	expectedValue: string[],
	expectedSelection: string[],
) {
	expect(element.value).to.deep.equal(expectedValue);
	expect(getDropdownSelection(element)).to.deep.equal(expectedSelection);
}

/**
 * Common test data for multiple updates
 */
export const SINGLE_SELECT_TEST_DATA: UmbTestDataEntry<string>[] = [
	{ value: 'Red', expected: 'Red' },
	{ value: 'Green', expected: 'Green' },
	{ value: 'Blue', expected: 'Blue' },
	{ value: '', expected: '' },
];

export const MULTI_SELECT_TEST_DATA: UmbTestDataEntry<string[]>[] = [
	{ value: ['Red'], expected: ['Red'] },
	{ value: ['Red', 'Blue'], expected: ['Red', 'Blue'] },
	{ value: ['Green'], expected: ['Green'] },
	{ value: [], expected: [] },
];

// Re-export expect for convenience
export { expect };
