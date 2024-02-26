import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITreePickerSourcePickerElement } from './property-editor-ui-tree-picker-source-picker.element.js';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITreePickerSourcePickerElement', () => {
	let element: UmbPropertyEditorUITreePickerSourcePickerElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-tree-picker-source-picker></umb-property-editor-ui-tree-picker-source-picker>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUITreePickerSourcePickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
