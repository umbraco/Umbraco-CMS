import { expect, fixture, html } from '@open-wc/testing';
import { UmbEyeDropperElement } from './eye-dropper.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';
describe('UmbEyeDropperElement', () => {
	let element: UmbEyeDropperElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-eye-dropper></umb-eye-dropper> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbEyeDropperElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
