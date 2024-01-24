import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIMemberGroupPickerElement } from './property-editor-ui-member-group-picker.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIMemberGroupPickerElement', () => {
	let element: UmbPropertyEditorUIMemberGroupPickerElement;

	beforeEach(async () => {
		element = await fixture(html`
			<umb-property-editor-ui-member-group-picker></umb-property-editor-ui-member-group-picker>
		`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIMemberGroupPickerElement);
	});

	if ((window as any).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
