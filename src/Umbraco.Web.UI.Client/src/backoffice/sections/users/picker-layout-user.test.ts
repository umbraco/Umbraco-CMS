import { expect, fixture, html } from '@open-wc/testing';
import { UmbPickerLayoutUserElement } from './picker-layout-user.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('UmbPickerUserElement', () => {
	let element: UmbPickerLayoutUserElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-picker-layout-user></umb-picker-layout-user>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPickerLayoutUserElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
