import { expect, fixture, html } from '@open-wc/testing';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';
import UmbPickerLayoutSectionElement from './picker-layout-section.element';

describe('UmbPickerLayoutSectionElement', () => {
	let element: UmbPickerLayoutSectionElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-picker-layout-section></umb-picker-layout-section>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPickerLayoutSectionElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
