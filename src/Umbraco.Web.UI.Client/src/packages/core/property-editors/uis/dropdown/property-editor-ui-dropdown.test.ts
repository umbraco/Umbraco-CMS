import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIDropdownElement } from './property-editor-ui-dropdown.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIDropdownElement', () => {
	let element: UmbPropertyEditorUIDropdownElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-dropdown></umb-property-editor-ui-dropdown> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIDropdownElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
