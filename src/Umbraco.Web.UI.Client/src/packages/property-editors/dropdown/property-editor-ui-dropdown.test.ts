import { UmbPropertyEditorUIDropdownElement } from './property-editor-ui-dropdown.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIDropdownElement', () => {
	let element: UmbPropertyEditorUIDropdownElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-dropdown></umb-property-editor-ui-dropdown> `);
	});

	// Helper function to reduce code duplication
	function getDropdownInput() {
		return element.shadowRoot?.querySelector('umb-input-dropdown-list');
	}

	// Helper function to get select element for multiple mode
	function getSelectElement() {
		return element.shadowRoot?.querySelector('select');
	}

	// Helper function to get selected values from DOM
	function getSelectedValues() {
		const dropdownInput = getDropdownInput();
		const selectElement = getSelectElement();
		
		if (dropdownInput) {
			// Single mode
			return dropdownInput.value ? [dropdownInput.value] : [];
		} else if (selectElement) {
			// Multiple mode
			const selectedOptions = selectElement.selectedOptions;
			return selectedOptions ? Array.from(selectedOptions).map(option => option.value) : [];
		}
		
		return [];
	}

	// Helper function to verify both selection and DOM state
	function verifySelectionAndDOM(expectedSelection: string[], expectedSelected: string[]) {
		expect(element.value).to.deep.equal(expectedSelection);
		expect(getSelectedValues().sort()).to.deep.equal(expectedSelected.sort());
	}

	// Helper function to setup basic configuration
	function setupBasicConfig(multiple = false) {
		element.config = {
			getValueByAlias: (alias: string) => {
				if (alias === 'items') {
					return ['Red', 'Green', 'Blue'];
				}
				if (alias === 'multiple') {
					return multiple;
				}
				return undefined;
			}
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
			setupBasicConfig(false);
			await element.updateComplete;
		});

		it('should update UI immediately when value is set programmatically with array', async () => {
			element.value = ['Red'];
			await element.updateComplete;

			expect(getDropdownInput()).to.exist;
			verifySelectionAndDOM(['Red'], ['Red']);
		});

		it('should update UI immediately when value is set to empty array', async () => {
			// First set some values
			element.value = ['Green'];
			await element.updateComplete;

			// Then clear them
			element.value = [];
			await element.updateComplete;

			verifySelectionAndDOM([], []);
		});

		it('should update UI immediately when value is set to single string', async () => {
			element.value = 'Blue';
			await element.updateComplete;

			verifySelectionAndDOM(['Blue'], ['Blue']);
		});

		it('should handle undefined value gracefully', async () => {
			element.value = undefined;
			await element.updateComplete;

			verifySelectionAndDOM([], []);
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
			verifySelectionAndDOM(testValue, testValue);
		});

		it('should update multiple times correctly', async () => {
			// Test data for multiple updates
			const updates = [
				{ value: ['Red'], expected: ['Red'] },
				{ value: ['Blue'], expected: ['Blue'] },
				{ value: [], expected: [] }
			];

			for (const update of updates) {
				element.value = update.value;
				await element.updateComplete;
				verifySelectionAndDOM(update.expected, update.expected);
			}
		});
	});

	describe('programmatic value setting - multiple mode', () => {
		beforeEach(async () => {
			setupBasicConfig(true);
			await element.updateComplete;
		});

		it('should update UI immediately when value is set programmatically with array', async () => {
			element.value = ['Red', 'Blue'];
			await element.updateComplete;

			expect(getSelectElement()).to.exist;
			verifySelectionAndDOM(['Red', 'Blue'], ['Red', 'Blue']);
		});

		it('should update UI immediately when value is set to empty array', async () => {
			// First set some values
			element.value = ['Red', 'Green'];
			await element.updateComplete;

			// Then clear them
			element.value = [];
			await element.updateComplete;

			verifySelectionAndDOM([], []);
		});

		it('should handle multiple selections correctly', async () => {
			element.value = ['Red', 'Green', 'Blue'];
			await element.updateComplete;

			verifySelectionAndDOM(['Red', 'Green', 'Blue'], ['Red', 'Green', 'Blue']);
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
				}
			} as any;
			
			element.value = ['Option1'];
			await element.updateComplete;

			verifySelectionAndDOM(['Option1'], ['Option1']);
		});

		it('should handle object array configuration', async () => {
			element.config = {
				getValueByAlias: (alias: string) => {
					if (alias === 'items') {
						return [
							{ name: 'Red Color', value: 'red' },
							{ name: 'Green Color', value: 'green' },
							{ name: 'Blue Color', value: 'blue' }
						];
					}
					if (alias === 'multiple') {
						return false;
					}
					return undefined;
				}
			} as any;
			
			element.value = ['red'];
			await element.updateComplete;

			verifySelectionAndDOM(['red'], ['red']);
		});

		it('should handle empty configuration gracefully', async () => {
			element.config = {
				getValueByAlias: () => undefined
			} as any;
			
			element.value = ['test'];
			await element.updateComplete;

			// Should not throw error
			expect(element.value).to.deep.equal(['test']);
		});

		it('should switch between single and multiple modes correctly', async () => {
			// Start with single mode
			setupBasicConfig(false);
			element.value = ['Red'];
			await element.updateComplete;

			expect(getDropdownInput()).to.exist;
			expect(getSelectElement()).to.not.exist;

			// Switch to multiple mode
			setupBasicConfig(true);
			await element.updateComplete;

			expect(getDropdownInput()).to.not.exist;
			expect(getSelectElement()).to.exist;
		});
	});
});
