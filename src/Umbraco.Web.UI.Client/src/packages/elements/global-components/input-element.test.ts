import { UmbInputElementElement } from './input-element.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

import '../../../packages/property-editors/entity-data-picker/input/input-entity-data.element.js';
describe('UmbInputElementElement', () => {
	let element: UmbInputElementElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-element></umb-input-element> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputElementElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
