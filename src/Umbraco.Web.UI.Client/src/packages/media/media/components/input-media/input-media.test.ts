import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputMediaElement } from './input-media.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputMediaElement', () => {
	let element: UmbInputMediaElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-media></umb-input-media> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputMediaElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
