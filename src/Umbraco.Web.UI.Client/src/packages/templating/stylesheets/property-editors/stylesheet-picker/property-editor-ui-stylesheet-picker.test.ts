import { UmbPropertyEditorUIStylesheetPickerElement } from './property-editor-ui-stylesheet-picker.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIStylesheetPickerElement', () => {
	let element: UmbPropertyEditorUIStylesheetPickerElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-stylesheet-picker></umb-property-editor-ui-stylesheet-picker>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIStylesheetPickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
