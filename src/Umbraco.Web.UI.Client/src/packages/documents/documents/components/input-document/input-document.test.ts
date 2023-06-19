import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputDocumentElement } from './input-document.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputDocumentElement', () => {
	let element: UmbInputDocumentElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-document></umb-input-document> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputDocumentElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
