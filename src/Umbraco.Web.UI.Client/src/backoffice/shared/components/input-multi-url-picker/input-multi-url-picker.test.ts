import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputMultiUrlPickerElement } from './input-multi-url-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';
describe('UmbInputMultiUrlPickerElement', () => {
	let element: UmbInputMultiUrlPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-multi-url-picker></umb-input-multi-url-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputMultiUrlPickerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
