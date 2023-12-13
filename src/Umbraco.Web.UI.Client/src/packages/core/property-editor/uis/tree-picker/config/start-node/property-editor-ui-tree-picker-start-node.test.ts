import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUITreePickerStartNodeElement } from './property-editor-ui-tree-picker-start-node.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUITreePickerStartNodeElement', () => {
	let element: UmbPropertyEditorUITreePickerStartNodeElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-tree-picker-start-node></umb-property-editor-ui-tree-picker-start-node>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUITreePickerStartNodeElement);
	});

	if ((window as any).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
