import { UmbInputBlockTypeElement } from './input-block-type.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputMediaElement', () => {
	let element: UmbInputBlockTypeElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-block-type type="block-list-type"></umb-input-block-type> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputBlockTypeElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
