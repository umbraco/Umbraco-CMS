import { expect, fixture, html } from '@open-wc/testing';
import { UmbDateInputElement } from './date-input.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbDateInputElement', () => {
	let element: UmbDateInputElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-date-input></umb-date-input> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbDateInputElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
