import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputTreeElement } from './input-tree.element.js';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputTreeElement', () => {
	let element: UmbInputTreeElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-tree></umb-input-tree> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputTreeElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
