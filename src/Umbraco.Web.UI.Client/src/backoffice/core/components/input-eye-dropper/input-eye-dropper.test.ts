import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputEyeDropperElement } from './input-eye-dropper.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputEyeDropperElement', () => {
	let element: UmbInputEyeDropperElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-eye-dropper></umb-input-eye-dropper> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputEyeDropperElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
