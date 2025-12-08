import { UmbPublicAccessModalElement } from './public-access-modal.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPublicAccessModalElement', () => {
	let element: UmbPublicAccessModalElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-public-access-modal></umb-public-access-modal> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPublicAccessModalElement);
	});

	it('renders a loader while loading', async () => {
		await element.updateComplete;
		const loader = element.shadowRoot!.querySelector('uui-loader');
		// The loader might not be visible if data loads too quickly, but we check it exists in the component
		expect(loader).to.exist;
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
