import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputDateElement } from './input-date.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputDateElement', () => {
	let element: UmbInputDateElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-date></umb-input-date> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputDateElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
