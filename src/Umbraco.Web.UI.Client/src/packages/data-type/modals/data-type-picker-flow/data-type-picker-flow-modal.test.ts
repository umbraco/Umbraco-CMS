import { UmbDataTypePickerFlowModalElement } from './data-type-picker-flow-modal.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbDataTypePickerFlowModalElement', () => {
	let element: UmbDataTypePickerFlowModalElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-data-type-picker-flow-modal></umb-data-type-picker-flow-modal> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbDataTypePickerFlowModalElement);
	});

	it('should have _isLoading state property', () => {
		expect(element).to.have.property('_isLoading');
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
