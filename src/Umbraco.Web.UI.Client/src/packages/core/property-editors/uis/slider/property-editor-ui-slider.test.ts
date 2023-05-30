import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUISliderElement } from './property-editor-ui-slider.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUISliderElement', () => {
	let element: UmbPropertyEditorUISliderElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-slider></umb-property-editor-ui-slider> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUISliderElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
