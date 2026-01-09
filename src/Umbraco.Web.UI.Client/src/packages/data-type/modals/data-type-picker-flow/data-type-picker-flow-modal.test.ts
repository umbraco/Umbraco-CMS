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

	it('should render a loader element when loading', async () => {
		// Set loading state to true (using bracket notation to access private property for testing)
		(element as any)._isLoading = true;
		await element.updateComplete;

		const loaderContainer = element.shadowRoot?.querySelector('.loader-container');
		const loader = element.shadowRoot?.querySelector('uui-loader');
		
		expect(loaderContainer).to.exist;
		expect(loader).to.exist;
	});

	it('should not render a loader element when not loading', async () => {
		// Ensure loading state is false
		(element as any)._isLoading = false;
		await element.updateComplete;

		const loaderContainer = element.shadowRoot?.querySelector('.loader-container');
		
		expect(loaderContainer).to.not.exist;
	});

	it('should hide loader after loading is complete', async () => {
		// Set loading state to true
		(element as any)._isLoading = true;
		await element.updateComplete;

		let loaderContainer = element.shadowRoot?.querySelector('.loader-container');
		expect(loaderContainer).to.exist;

		// Set loading state to false (simulating completion)
		(element as any)._isLoading = false;
		await element.updateComplete;

		loaderContainer = element.shadowRoot?.querySelector('.loader-container');
		expect(loaderContainer).to.not.exist;
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
