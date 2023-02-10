import { expect, fixture, html } from '@open-wc/testing';
import { UmbColorPickerElement } from './color-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';
describe('UmbColorPickerElement', () => {
	let element: UmbColorPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-color-picker></umb-color-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbColorPickerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
