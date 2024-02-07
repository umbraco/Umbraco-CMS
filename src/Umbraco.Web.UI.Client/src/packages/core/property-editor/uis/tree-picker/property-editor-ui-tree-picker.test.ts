import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITreePickerElement } from './property-editor-ui-tree-picker.element.js';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITreePickerElement', () => {
	let element: UmbPropertyEditorUITreePickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-tree-picker></umb-property-editor-ui-tree-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUITreePickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
