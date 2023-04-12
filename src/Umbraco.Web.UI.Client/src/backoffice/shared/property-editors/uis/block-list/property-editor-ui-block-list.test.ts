import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorUIBlockListElement } from './property-editor-ui-block-list.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIBlockListElement', () => {
	let element: UmbPropertyEditorUIBlockListElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-block-list></umb-property-editor-ui-block-list> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIBlockListElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
