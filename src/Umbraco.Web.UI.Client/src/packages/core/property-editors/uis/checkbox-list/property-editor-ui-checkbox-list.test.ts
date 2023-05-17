import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUICheckboxListElement } from './property-editor-ui-checkbox-list.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUICheckboxListElement', () => {
	let element: UmbPropertyEditorUICheckboxListElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-checkbox-list></umb-property-editor-ui-checkbox-list> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUICheckboxListElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
