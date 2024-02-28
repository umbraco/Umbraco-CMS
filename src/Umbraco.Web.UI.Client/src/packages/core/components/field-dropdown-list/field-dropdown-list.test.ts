import { expect, fixture, html } from '@open-wc/testing';
import { UmbFieldDropdownListElement } from './field-dropdown-list.element.js';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputDateElement', () => {
	let element: UmbFieldDropdownListElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-field-dropdown-list></umb-field-dropdown-list> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbFieldDropdownListElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
