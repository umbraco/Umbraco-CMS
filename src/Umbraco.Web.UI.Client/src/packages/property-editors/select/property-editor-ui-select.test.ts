import { UmbPropertyEditorUISelectElement } from './property-editor-ui-select.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUISelectElement', () => {
	let element: UmbPropertyEditorUISelectElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-property-editor-ui-select></umb-property-editor-ui-select>`);
	});

	// Helper function to reduce code duplication
	function getSelectElement() {
		return element.shadowRoot?.querySelector('uui-select');
	}

	// Helper function to get selected value from DOM
	function getSelectedValue() {
		const selectElement = getSelectElement();
		return selectElement?.value || '';
	}

	// Helper function to verify both value and DOM state
	function verifyValueAndDOM(expectedValue: string, expectedSelected: string) {
		expect(element.value).to.equal(expectedValue);
		expect(getSelectedValue()).to.equal(expectedSelected);
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
		expect(element).to.be.instanceOf(UmbPropertyEditorUISelectElement);
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

		it('should update UI immediately when value is set programmatically', async () => {
			element.value = 'Red';
			await element.updateComplete;

			expect(getSelectElement()).to.exist;
			verifyValueAndDOM('Red', 'Red');
		});

		it('should update UI immediately when value is set to empty string', async () => {
			// First set some value
			element.value = 'Green';
			await element.updateComplete;

			// Then clear it
			element.value = '';
			await element.updateComplete;

			verifyValueAndDOM('', '');
		});

		it('should handle undefined value gracefully', async () => {
			element.value = undefined;
			await element.updateComplete;

			verifyValueAndDOM('', '');
		});

		it('should handle invalid values gracefully', async () => {
			// Set value with invalid option that doesn't exist in the configured list ['Red', 'Green', 'Blue']
			element.value = 'InvalidColor';
			await element.updateComplete;

			// Should preserve the value even if it's not in the options
			expect(element.value).to.equal('InvalidColor');
		});

		it('should maintain value consistency between getter and setter', async () => {
			const testValue = 'Blue';
			element.value = testValue;
			await element.updateComplete;

			expect(element.value).to.equal(testValue);
			verifyValueAndDOM(testValue, testValue);
		});

		it('should update multiple times correctly', async () => {
			// Test data for multiple updates
			const updates = [
				{ value: 'Red', expected: 'Red' },
				{ value: 'Green', expected: 'Green' },
				{ value: 'Blue', expected: 'Blue' },
				{ value: '', expected: '' }
			];

			for (const update of updates) {
				element.value = update.value;
				await element.updateComplete;
				verifyValueAndDOM(update.expected, update.expected);
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
			
			element.value = 'Option2';
			await element.updateComplete;

			verifyValueAndDOM('Option2', 'Option2');
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
			
			element.value = 'green';
			await element.updateComplete;

			verifyValueAndDOM('green', 'green');
		});

		it('should handle empty configuration gracefully', async () => {
			element.config = {
				getValueByAlias: () => undefined
			} as any;
			
			element.value = 'test';
			await element.updateComplete;

			// Should not throw error
			expect(element.value).to.equal('test');

			// Should have no options since configuration is empty
			const selectElement = getSelectElement();
			expect(selectElement?.options).to.have.length(0);
		});

		it('should update options when configuration changes', async () => {
			// Start with initial config
			setupBasicConfig();
			element.value = 'Red';
			await element.updateComplete;

			verifyValueAndDOM('Red', 'Red');

			// Change configuration
			element.config = {
				getValueByAlias: (alias: string) => {
					if (alias === 'items') {
						return ['Yellow', 'Purple', 'Orange'];
					}
					return undefined;
				}
			} as any;
			await element.updateComplete;

			// Value should be preserved even if not in new options
			expect(element.value).to.equal('Red');

			// Set a value from the new options
			element.value = 'Yellow';
			await element.updateComplete;

			verifyValueAndDOM('Yellow', 'Yellow');
		});
	});
});
