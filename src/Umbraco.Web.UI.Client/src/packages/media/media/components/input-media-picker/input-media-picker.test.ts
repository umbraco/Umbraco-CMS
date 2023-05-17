import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputMediaPickerElement } from './input-media-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputMediaPickerElement', () => {
	let element: UmbInputMediaPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-media-picker></umb-input-media-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputMediaPickerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
