import { UmbInputNumberRangeElement } from './input-number-range.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbPropertyEditorUINumberRangeElement', () => {
	let element: UmbInputNumberRangeElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-number-range></umb-input-number-range> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputNumberRangeElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
