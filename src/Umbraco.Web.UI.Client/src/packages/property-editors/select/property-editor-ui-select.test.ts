import { UmbPropertyEditorUISelectElement } from './property-editor-ui-select.element.js';
import { fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import {
	expect,
	setupBasicStringConfig,
	setupObjectConfig,
	setupEmptyConfig,
	getSelectElement,
	getSelectedValue,
	verifySelectValueAndDOM,
	SINGLE_SELECT_TEST_DATA,
} from '../utils/property-editor-test-utils.js';

describe('UmbPropertyEditorUISelectElement', () => {
	let element: UmbPropertyEditorUISelectElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-property-editor-ui-select></umb-property-editor-ui-select>`);
	});

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
			setupBasicStringConfig(element);
			await element.updateComplete;
		});

		it('should update UI immediately when value is set programmatically', async () => {
			element.value = 'Red';
			await element.updateComplete;

			expect(getSelectElement(element)).to.exist;
			verifySelectValueAndDOM(element, 'Red', 'Red');
		});

		it('should update UI immediately when value is set to empty string', async () => {
			// First set some value
			element.value = 'Green';
			await element.updateComplete;

			// Then clear it
			element.value = '';
			await element.updateComplete;

			verifySelectValueAndDOM(element, '', '');
		});

		it('should handle undefined value gracefully', async () => {
			element.value = undefined;
			await element.updateComplete;

			verifySelectValueAndDOM(element, '', '');
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
			verifySelectValueAndDOM(element, testValue, testValue);
		});

		it('should update multiple times correctly', async () => {
			for (const update of SINGLE_SELECT_TEST_DATA) {
				element.value = update.value;
				await element.updateComplete;
				verifySelectValueAndDOM(element, update.expected, update.expected);
			}
		});
	});

	describe('configuration handling', () => {
		it('should handle string array configuration', async () => {
			setupBasicStringConfig(element, ['Option1', 'Option2', 'Option3']);
			element.value = 'Option2';
			await element.updateComplete;

			verifySelectValueAndDOM(element, 'Option2', 'Option2');
		});

		it('should handle object array configuration', async () => {
			setupObjectConfig(element);
			element.value = 'green';
			await element.updateComplete;

			verifySelectValueAndDOM(element, 'green', 'green');
		});

		it('should handle empty configuration gracefully', async () => {
			setupEmptyConfig(element);
			element.value = 'test';
			await element.updateComplete;

			// Should not throw error
			expect(element.value).to.equal('test');

			// Should have no options since configuration is empty
			const selectElement = getSelectElement(element) as { options?: HTMLOptionElement[] } | null;
			expect(selectElement?.options).to.have.length(0);
		});

		it('should update options when configuration changes', async () => {
			// Start with initial config
			setupBasicStringConfig(element);
			element.value = 'Red';
			await element.updateComplete;

			verifySelectValueAndDOM(element, 'Red', 'Red');

			// Change configuration
			setupBasicStringConfig(element, ['Yellow', 'Purple', 'Orange']);
			await element.updateComplete;

			// Value should be preserved even if not in new options
			expect(element.value).to.equal('Red');

			// Set a value from the new options
			element.value = 'Yellow';
			await element.updateComplete;

			verifySelectValueAndDOM(element, 'Yellow', 'Yellow');
		});
	});
});
