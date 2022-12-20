import { expect, fixture, html } from '@open-wc/testing';
import { UmbModalLayoutIconPickerElement } from './modal-layout-icon-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('umb-modal-layout-icon-picker', () => {
	let element: UmbModalLayoutIconPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-modal-layout-icon-picker></umb-modal-layout-icon-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbModalLayoutIconPickerElement);
	});

	// TODO: Reinstate this test when the a11y audit is fixed on uui-color-picker
	// it('passes the a11y audit', async () => {
	// 	await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	// });
});
