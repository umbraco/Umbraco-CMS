import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIIconPickerElement } from './property-editor-ui-icon-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIIconPickerElement', () => {
	let element: UmbPropertyEditorUIIconPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-icon-picker></umb-property-editor-ui-icon-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIIconPickerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
