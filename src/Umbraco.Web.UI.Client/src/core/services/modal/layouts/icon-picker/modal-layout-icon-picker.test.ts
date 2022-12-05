import { expect, fixture, html } from '@open-wc/testing';
import { UmbModalLayoutIconPickerElement } from './modal-layout-icon-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/test-utils';

describe('umb-modal-layout-icon-picker', () => {
	let element: UmbModalLayoutIconPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-icon-picker></umb-property-editor-ui-icon-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbModalLayoutIconPickerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
