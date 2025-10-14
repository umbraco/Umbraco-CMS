import { UmbPropertyEditorUICheckboxListElement } from './property-editor-ui-checkbox-list.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import {
	setupBasicStringConfig,
	setupObjectConfig,
	setupEmptyConfig,
	getCheckboxListElement,
	getCheckboxSelection,
	verifyMultiSelectValueAndDOM,
	MULTI_SELECT_TEST_DATA,
} from '../utils/property-editor-test-utils.js';

describe('UmbPropertyEditorUICheckboxListElement', () => {
	let element: UmbPropertyEditorUICheckboxListElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-property-editor-ui-checkbox-list></umb-property-editor-ui-checkbox-list>`);
	});

	// Local helper function to get checked values from DOM (specific to this component)
	function getLocalCheckedValues() {
		const checkboxListInput = getCheckboxListElement(element);
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

	// Local helper function to verify both selection and DOM state
	function verifyLocalSelectionAndDOM(expectedSelection: string[], expectedChecked: string[]) {
		const checkboxListInput = getCheckboxListElement(element) as { selection?: string[] } | null;
		expect(checkboxListInput?.selection).to.deep.equal(expectedSelection);
		expect(getLocalCheckedValues().sort()).to.deep.equal(expectedChecked.sort());
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
			setupBasicStringConfig(element);
			await element.updateComplete;
		});

		it('should update UI immediately when value is set programmatically with array', async () => {
			element.value = ['Red', 'Blue'];
			await element.updateComplete;

			expect(getCheckboxListElement(element)).to.exist;
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

		it('should update UI immediately when value is set to single string', async () => {
			element.value = 'Green';
			await element.updateComplete;

			verifyLocalSelectionAndDOM(['Green'], ['Green']);
		});

		it('should handle undefined value gracefully', async () => {
			element.value = undefined;
			await element.updateComplete;

			verifyLocalSelectionAndDOM([], []);
		});

		it('should handle invalid values gracefully', async () => {
			// Set value with invalid option that doesn't exist in the configured list ['Red', 'Green', 'Blue']
			element.value = ['Red', 'InvalidColor', 'Blue'];
			await element.updateComplete;

			// Should preserve all values in selection but only check valid ones in DOM
			verifyLocalSelectionAndDOM(['Red', 'InvalidColor', 'Blue'], ['Red', 'Blue']);
		});

		it('should maintain value consistency between getter and setter', async () => {
			const testValue = ['Red', 'Green'];
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

	describe('configuration handling', () => {
		it('should handle string array configuration', async () => {
			setupBasicStringConfig(element, ['Option1', 'Option2', 'Option3']);

			element.value = ['Option1', 'Option3'];
			await element.updateComplete;

			verifyLocalSelectionAndDOM(['Option1', 'Option3'], ['Option1', 'Option3']);
		});

		it('should handle object array configuration', async () => {
			setupObjectConfig(element);

			element.value = ['red', 'blue'];
			await element.updateComplete;

			verifyLocalSelectionAndDOM(['red', 'blue'], ['red', 'blue']);
		});

		it('should handle empty configuration gracefully', async () => {
			setupEmptyConfig(element);

			element.value = ['test'];
			await element.updateComplete;

			// Should not throw error
			expect(element.value).to.deep.equal(['test']);

			// Should have no uui-checkboxes since configuration is empty
			const checkboxListInput = getCheckboxListElement(element);
			const checkboxElements = checkboxListInput?.shadowRoot?.querySelectorAll('uui-checkbox') || [];
			expect(checkboxElements).to.have.length(0);
		});
	});
});
