import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputDropdownListElement } from './input-dropdown-list.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputDateElement', () => {
	let element: UmbInputDropdownListElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-dropdown-list></umb-input-dropdown-list> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputDropdownListElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
