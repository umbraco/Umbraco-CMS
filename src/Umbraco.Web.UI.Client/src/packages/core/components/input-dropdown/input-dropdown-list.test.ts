import { UmbInputDropdownListElement } from './input-dropdown-list.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputDateElement', () => {
	let element: UmbInputDropdownListElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-dropdown-list></umb-input-dropdown-list> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputDropdownListElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
