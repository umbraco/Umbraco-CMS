import { expect, fixture, html } from '@open-wc/testing';
import { defaultA11yConfig } from '../../../core/helpers/chai';
import { UmbPropertyEditorUIContentPickerElement } from './umb-property-editor-ui-content-picker.element';

describe('UmbPropertyEditorUIContentPickerElement', () => {
	let element: UmbPropertyEditorUIContentPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-content-picker></umb-property-editor-ui-content-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIContentPickerElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
