import { expect, fixture, html } from '@open-wc/testing';
import { UmbPickerSectionElement } from './picker-section.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('UmbPickerSectionElement', () => {
	let element: UmbPickerSectionElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-picker-section></umb-picker-section>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPickerSectionElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
