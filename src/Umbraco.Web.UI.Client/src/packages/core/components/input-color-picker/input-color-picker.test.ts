import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputColorPickerElement } from './input-color-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputColorPickerElement', () => {
	let element: UmbInputColorPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-color-picker></umb-input-color-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputColorPickerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
