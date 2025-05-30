import { UmbPropertyEditorUICheckboxListElement } from './property-editor-ui-checkbox-list.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUICheckboxListElement', () => {
	let element: UmbPropertyEditorUICheckboxListElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-property-editor-ui-checkbox-list></umb-property-editor-ui-checkbox-list>`);
	});

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
			// Set up configuration with test items
			element.config = {
				getValueByAlias: (alias: string) => {
					if (alias === 'items') {
						return ['Red', 'Green', 'Blue'];
					}
					return undefined;
				}
			} as any;
			await element.updateComplete;
		});

		it('should update UI immediately when value is set programmatically with array', async () => {
			// Set value programmatically
			element.value = ['Red', 'Blue'];
			await element.updateComplete;

			// Get the checkbox list input element
			const checkboxListInput = element.shadowRoot?.querySelector('umb-input-checkbox-list');
			expect(checkboxListInput).to.exist;

			// Check that the selection property is updated
			expect(checkboxListInput?.selection).to.deep.equal(['Red', 'Blue']);
		});

		it('should update UI immediately when value is set to empty array', async () => {
			// First set some values
			element.value = ['Red', 'Green'];
			await element.updateComplete;

			// Then clear them
			element.value = [];
			await element.updateComplete;

			const checkboxListInput = element.shadowRoot?.querySelector('umb-input-checkbox-list');
			expect(checkboxListInput?.selection).to.deep.equal([]);
		});

		it('should update UI immediately when value is set to single string', async () => {
			// Set single string value
			element.value = 'Green';
			await element.updateComplete;

			const checkboxListInput = element.shadowRoot?.querySelector('umb-input-checkbox-list');
			expect(checkboxListInput?.selection).to.deep.equal(['Green']);
		});

		it('should handle undefined value gracefully', async () => {
			// Set undefined value
			element.value = undefined;
			await element.updateComplete;

			const checkboxListInput = element.shadowRoot?.querySelector('umb-input-checkbox-list');
			expect(checkboxListInput?.selection).to.deep.equal([]);
		});

		it('should handle invalid values gracefully', async () => {
			// Set value with invalid option
			element.value = ['Red', 'InvalidColor', 'Blue'];
			await element.updateComplete;

			// Should still include the invalid value in selection
			const checkboxListInput = element.shadowRoot?.querySelector('umb-input-checkbox-list');
			expect(checkboxListInput?.selection).to.deep.equal(['Red', 'InvalidColor', 'Blue']);
		});

		it('should maintain value consistency between getter and setter', async () => {
			const testValue = ['Red', 'Green'];
			element.value = testValue;
			await element.updateComplete;

			expect(element.value).to.deep.equal(testValue);
		});

		it('should update multiple times correctly', async () => {
			// First update
			element.value = ['Red'];
			await element.updateComplete;
			
			let checkboxListInput = element.shadowRoot?.querySelector('umb-input-checkbox-list');
			expect(checkboxListInput?.selection).to.deep.equal(['Red']);

			// Second update
			element.value = ['Green', 'Blue'];
			await element.updateComplete;
			
			checkboxListInput = element.shadowRoot?.querySelector('umb-input-checkbox-list');
			expect(checkboxListInput?.selection).to.deep.equal(['Green', 'Blue']);

			// Third update
			element.value = [];
			await element.updateComplete;
			
			checkboxListInput = element.shadowRoot?.querySelector('umb-input-checkbox-list');
			expect(checkboxListInput?.selection).to.deep.equal([]);
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

			const checkboxListInput = element.shadowRoot?.querySelector('umb-input-checkbox-list');
			expect(checkboxListInput?.selection).to.deep.equal(['Option1', 'Option3']);
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

			const checkboxListInput = element.shadowRoot?.querySelector('umb-input-checkbox-list');
			expect(checkboxListInput?.selection).to.deep.equal(['red', 'blue']);
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
	});

	describe('readonly mode', () => {
		beforeEach(async () => {
			element.config = {
				getValueByAlias: (alias: string) => {
					if (alias === 'items') {
						return ['Red', 'Green', 'Blue'];
					}
					return undefined;
				}
			} as any;
		});

		it('should respect readonly property', async () => {
			element.readonly = true;
			element.value = ['Red', 'Green'];
			await element.updateComplete;

			const checkboxListInput = element.shadowRoot?.querySelector('umb-input-checkbox-list');
			expect(checkboxListInput?.hasAttribute('readonly')).to.be.true;
		});
	});
});
