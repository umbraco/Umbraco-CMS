import { expect, fixture, html } from '@open-wc/testing';
import { UmbDocumentInputElement } from './document-input.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbDocumentInputElement', () => {
	let element: UmbDocumentInputElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-document-input></umb-document-input> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbDocumentInputElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
