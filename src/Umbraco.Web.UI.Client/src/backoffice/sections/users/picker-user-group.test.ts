import { expect, fixture, html } from '@open-wc/testing';
import { UmbPickerUserGroupElement } from './picker-user-group.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('UmbPickerLayoutUserGroupElement', () => {
	let element: UmbPickerUserGroupElement;
	beforeEach(async () => {
		element = await fixture(html`<umb-picker-user-group></umb-picker-user-group>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPickerUserGroupElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
