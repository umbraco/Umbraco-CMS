import { UmbPropertyEditorUIDropdownElement } from './property-editor-ui-dropdown.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import {
	setupBasicStringConfig,
	setupObjectConfig,
	setupEmptyConfig,
	MULTI_SELECT_TEST_DATA,
} from '../utils/property-editor-test-utils.js';

describe('UmbPropertyEditorUIDropdownElement', () => {
	let element: UmbPropertyEditorUIDropdownElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-dropdown></umb-property-editor-ui-dropdown> `);
	});

	// Local helper functions to avoid conflicts with shared utilities
	function getLocalDropdownInput() {
		return element.shadowRoot?.querySelector('umb-input-dropdown-list');
	}

	function getNativeSelectElement() {
		return element.shadowRoot?.querySelector('select');
	}

	function getLocalSelectedValues() {
		const dropdownInput = getLocalDropdownInput();
		const selectElement = getNativeSelectElement();

		if (dropdownInput) {
			// Single mode - the dropdown input value might be a string or comma-separated string
			const value = dropdownInput.value;
			if (!value) return [];
			// Handle both single values and comma-separated values
			return typeof value === 'string' ? value.split(', ').filter((v) => v.length > 0) : [value];
		} else if (selectElement) {
			// Multiple mode
			const selectedOptions = selectElement.selectedOptions;
			return selectedOptions ? Array.from(selectedOptions).map((option) => option.value) : [];
		}

		return [];
	}

	function verifyLocalSelectionAndDOM(expectedSelection: string[], expectedSelected: string[]) {
		expect(element.value).to.deep.equal(expectedSelection);
		expect(getLocalSelectedValues().sort()).to.deep.equal(expectedSelected.sort());
	}

	function setupBasicConfigWithMultiple(multiple = false) {
		element.config = {
			getValueByAlias: (alias: string) => {
				if (alias === 'items') {
					return ['Red', 'Green', 'Blue'];
				}
				if (alias === 'multiple') {
					return multiple;
				}
				return undefined;
			},
		} as any;
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIDropdownElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('programmatic value setting - single mode', () => {
		beforeEach(async () => {
			setupBasicConfigWithMultiple(false);
			await element.updateComplete;
		});

		it('should update UI immediately when value is set programmatically with array', async () => {
			element.value = ['Red'];
			await element.updateComplete;

			expect(getLocalDropdownInput()).to.exist;
			verifyLocalSelectionAndDOM(['Red'], ['Red']);
		});

		it('should update UI immediately when value is set to empty array', async () => {
			// First set some values
			element.value = ['Green'];
			await element.updateComplete;

			// Then clear them
			element.value = [];
			await element.updateComplete;

			verifyLocalSelectionAndDOM([], []);
		});

		it('should update UI immediately when value is set to single string', async () => {
			element.value = 'Blue';
			await element.updateComplete;

			verifyLocalSelectionAndDOM(['Blue'], ['Blue']);
		});

		it('should handle undefined value gracefully', async () => {
			element.value = undefined;
			await element.updateComplete;

			verifyLocalSelectionAndDOM([], []);
		});

		it('should handle invalid values gracefully', async () => {
			// Set value with invalid option that doesn't exist in the configured list ['Red', 'Green', 'Blue']
			element.value = ['InvalidColor'];
			await element.updateComplete;

			// Should preserve all values in selection
			expect(element.value).to.deep.equal(['InvalidColor']);
		});

		it('should maintain value consistency between getter and setter', async () => {
			const testValue = ['Green'];
			element.value = testValue;
			await element.updateComplete;

			expect(element.value).to.deep.equal(testValue);
			verifyLocalSelectionAndDOM(testValue, testValue);
		});

		it('should update multiple times correctly', async () => {
			for (const update of MULTI_SELECT_TEST_DATA) {
				element.value = update.value;
				await element.updateComplete;
				verifyLocalSelectionAndDOM(update.expected, update.expected);
			}
		});
	});

	describe('programmatic value setting - multiple mode', () => {
		beforeEach(async () => {
			setupBasicConfigWithMultiple(true);
			await element.updateComplete;
		});

		it('should update UI immediately when value is set programmatically with array', async () => {
			element.value = ['Red', 'Blue'];
			await element.updateComplete;

			expect(getNativeSelectElement()).to.exist;
			verifyLocalSelectionAndDOM(['Red', 'Blue'], ['Red', 'Blue']);
		});

		it('should update UI immediately when value is set to empty array', async () => {
			// First set some values
			element.value = ['Red', 'Green'];
			await element.updateComplete;

			// Then clear them
			element.value = [];
			await element.updateComplete;

			verifyLocalSelectionAndDOM([], []);
		});

		it('should handle multiple selections correctly', async () => {
			element.value = ['Red', 'Green', 'Blue'];
			await element.updateComplete;

			verifyLocalSelectionAndDOM(['Red', 'Green', 'Blue'], ['Red', 'Green', 'Blue']);
		});

		it('should handle invalid values gracefully', async () => {
			// Set value with invalid option that doesn't exist in the configured list ['Red', 'Green', 'Blue']
			element.value = ['Red', 'InvalidColor', 'Blue'];
			await element.updateComplete;

			// Should preserve all values in selection
			expect(element.value).to.deep.equal(['Red', 'InvalidColor', 'Blue']);
		});
	});

	describe('configuration handling', () => {
		it('should handle string array configuration', async () => {
			element.config = {
				getValueByAlias: (alias: string) => {
					if (alias === 'items') {
						return ['Option1', 'Option2', 'Option3'];
					}
					if (alias === 'multiple') {
						return false;
					}
					return undefined;
				},
			} as any;

			element.value = ['Option1'];
			await element.updateComplete;

			verifyLocalSelectionAndDOM(['Option1'], ['Option1']);
		});

		it('should handle object array configuration', async () => {
			element.config = {
				getValueByAlias: (alias: string) => {
					if (alias === 'items') {
						return [
							{ name: 'Red Color', value: 'red' },
							{ name: 'Green Color', value: 'green' },
							{ name: 'Blue Color', value: 'blue' },
						];
					}
					if (alias === 'multiple') {
						return false;
					}
					return undefined;
				},
			} as any;

			element.value = ['red'];
			await element.updateComplete;

			verifyLocalSelectionAndDOM(['red'], ['red']);
		});

		it('should handle empty configuration gracefully', async () => {
			element.config = {
				getValueByAlias: () => undefined,
			} as any;

			element.value = ['test'];
			await element.updateComplete;

			// Should not throw error
			expect(element.value).to.deep.equal(['test']);
		});

		it('should switch between single and multiple modes correctly', async () => {
			// Start with single mode
			setupBasicConfigWithMultiple(false);
			element.value = ['Red'];
			await element.updateComplete;

			expect(getLocalDropdownInput()).to.exist;
			expect(getNativeSelectElement()).to.not.exist;

			// Switch to multiple mode
			setupBasicConfigWithMultiple(true);
			await element.updateComplete;

			expect(getLocalDropdownInput()).to.not.exist;
			expect(getNativeSelectElement()).to.exist;
		});
	});
});
