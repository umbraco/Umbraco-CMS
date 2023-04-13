import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputDatePickerElement } from './input-date-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputDatePickerElement', () => {
	let element: UmbInputDatePickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-date-picker></umb-input-date-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputDatePickerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
