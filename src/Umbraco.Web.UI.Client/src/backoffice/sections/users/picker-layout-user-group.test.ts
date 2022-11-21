import { expect, fixture, html } from '@open-wc/testing';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';
import UmbPickerLayoutUserGroupElement from './picker-layout-user-group.element';

describe('UmbPickerLayoutUserGroupElement', () => {
	let element: UmbPickerLayoutUserGroupElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-picker-layout-user-group></umb-picker-layout-user-group>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPickerLayoutUserGroupElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
