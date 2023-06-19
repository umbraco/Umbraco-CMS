import { expect, fixture, html } from '@open-wc/testing';
import { UmbMediaInputElement } from './media-input.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbMediaInputElement', () => {
	let element: UmbMediaInputElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-media-input></umb-media-input> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbMediaInputElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
