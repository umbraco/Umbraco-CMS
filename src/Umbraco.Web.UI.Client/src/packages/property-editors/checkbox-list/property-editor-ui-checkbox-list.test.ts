import { UmbPropertyEditorUICheckboxListElement } from './property-editor-ui-checkbox-list.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUICheckboxListElement', () => {
	let element: UmbPropertyEditorUICheckboxListElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-property-editor-ui-checkbox-list></umb-property-editor-ui-checkbox-list>`);
	});

	// Helper function to reduce code duplication
	function getCheckboxListInput() {
		return element.shadowRoot?.querySelector('umb-input-checkbox-list');
	}

	// Helper function to get checked values from DOM
	function getCheckedValues() {
		const checkboxListInput = getCheckboxListInput();
		const checkboxElements = checkboxListInput?.shadowRoot?.querySelectorAll('uui-checkbox') || [];
		const checkedValues: string[] = [];
		
		checkboxElements.forEach((checkbox: Element) => {
			const uuiCheckbox = checkbox as any;
			if (uuiCheckbox.checked) {
				checkedValues.push(uuiCheckbox.value);
			}
		});
		
		return checkedValues;
	}

	// Helper function to verify both selection and DOM state
	function verifySelectionAndDOM(expectedSelection: string[], expectedChecked: string[]) {
		const checkboxListInput = getCheckboxListInput();
		expect(checkboxListInput?.selection).to.deep.equal(expectedSelection);
		expect(getCheckedValues().sort()).to.deep.equal(expectedChecked.sort());
	}

	// Helper function to setup basic configuration
	function setupBasicConfig() {
		element.config = {
			getValueByAlias: (alias: string) => {
				if (alias === 'items') {
					return ['Red', 'Green', 'Blue'];
				}
				return undefined;
			}
		} as any;
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUICheckboxListElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('programmatic value setting', () => {
		beforeEach(async () => {
			setupBasicConfig();
			await element.updateComplete;
		});

		it('should update UI immediately when value is set programmatically with array', async () => {
			element.value = ['Red', 'Blue'];
			await element.updateComplete;

			expect(getCheckboxListInput()).to.exist;
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

		it('should update UI immediately when value is set to single string', async () => {
			element.value = 'Green';
			await element.updateComplete;

			verifySelectionAndDOM(['Green'], ['Green']);
		});

		it('should handle undefined value gracefully', async () => {
			element.value = undefined;
			await element.updateComplete;

			verifySelectionAndDOM([], []);
		});

		it('should handle invalid values gracefully', async () => {
			// Set value with invalid option that doesn't exist in the configured list ['Red', 'Green', 'Blue']
			element.value = ['Red', 'InvalidColor', 'Blue'];
			await element.updateComplete;

			// Should preserve all values in selection but only check valid ones in DOM
			verifySelectionAndDOM(['Red', 'InvalidColor', 'Blue'], ['Red', 'Blue']);
		});

		it('should maintain value consistency between getter and setter', async () => {
			const testValue = ['Red', 'Green'];
			element.value = testValue;
			await element.updateComplete;

			expect(element.value).to.deep.equal(testValue);
			verifySelectionAndDOM(testValue, testValue);
		});

		it('should update multiple times correctly', async () => {
			// Test data for multiple updates
			const updates = [
				{ value: ['Red'], expected: ['Red'] },
				{ value: ['Green', 'Blue'], expected: ['Green', 'Blue'] },
				{ value: [], expected: [] }
			];

			for (const update of updates) {
				element.value = update.value;
				await element.updateComplete;
				verifySelectionAndDOM(update.expected, update.expected);
			}
		});
	});

	describe('configuration handling', () => {
		it('should handle string array configuration', async () => {
			element.config = {
				getValueByAlias: (alias: string) => {
					if (alias === 'items') {
						return ['Option1', 'Option2', 'Option3'];
					}
					return undefined;
				}
			} as any;
			
			element.value = ['Option1', 'Option3'];
			await element.updateComplete;

			verifySelectionAndDOM(['Option1', 'Option3'], ['Option1', 'Option3']);
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
					return undefined;
				}
			} as any;
			
			element.value = ['red', 'blue'];
			await element.updateComplete;

			verifySelectionAndDOM(['red', 'blue'], ['red', 'blue']);
		});

		it('should handle empty configuration gracefully', async () => {
			element.config = {
				getValueByAlias: () => undefined
			} as any;
			
			element.value = ['test'];
			await element.updateComplete;

			// Should not throw error
			expect(element.value).to.deep.equal(['test']);

			// Should have no uui-checkboxes since configuration is empty
			const checkboxListInput = getCheckboxListInput();
			const checkboxElements = checkboxListInput?.shadowRoot?.querySelectorAll('uui-checkbox') || [];
			expect(checkboxElements).to.have.length(0);
		});
	});
});
