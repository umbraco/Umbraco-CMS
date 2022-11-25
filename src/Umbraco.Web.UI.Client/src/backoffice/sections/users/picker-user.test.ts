import { expect, fixture, html } from '@open-wc/testing';
import { UmbPickerUserElement } from './picker-user.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('UmbPickerUserElement', () => {
	let element: UmbPickerUserElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-picker-user></umb-picker-user>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPickerUserElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
