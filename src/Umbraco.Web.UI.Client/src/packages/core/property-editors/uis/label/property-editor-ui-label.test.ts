import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUILabelElement } from './property-editor-ui-label.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUILabelElement', () => {
	let element: UmbPropertyEditorUILabelElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-label></umb-property-editor-ui-label> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUILabelElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
