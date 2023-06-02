import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIColorPickerElement } from './property-editor-ui-color-picker.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIColorPickerElement', () => {
	let element: UmbPropertyEditorUIColorPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-color-picker></umb-property-editor-ui-color-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIColorPickerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
