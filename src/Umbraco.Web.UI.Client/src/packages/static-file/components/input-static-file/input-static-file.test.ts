import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputStaticFileElement } from './input-static-file.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputDocumentElement', () => {
	let element: UmbInputStaticFileElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-static-file></umb-input-static-file> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputStaticFileElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
