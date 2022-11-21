import { expect, fixture, html } from '@open-wc/testing';
import UmbPickerElement from './picker.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('UmbPickerElement', () => {
	let element: UmbPickerElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-picker></umb-picker>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPickerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
