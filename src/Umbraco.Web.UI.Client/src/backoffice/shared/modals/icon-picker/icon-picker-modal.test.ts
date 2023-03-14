import { expect, fixture, html } from '@open-wc/testing';
import { UmbIconPickerModalElement } from './icon-picker-modal.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('umb-icon-picker-modal', () => {
	let element: UmbIconPickerModalElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-icon-picker-modal></umb-icon-picker-modal> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbIconPickerModalElement);
	});

	// TODO: Reinstate this test when the a11y audit is fixed on uui-color-picker
	// it('passes the a11y audit', async () => {
	// 	await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	// });
});
