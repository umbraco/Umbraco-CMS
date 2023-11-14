import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputTreeElement } from './input-tree.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputTreeElement', () => {
	let element: UmbInputTreeElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-tree></umb-input-tree> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputTreeElement);
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
