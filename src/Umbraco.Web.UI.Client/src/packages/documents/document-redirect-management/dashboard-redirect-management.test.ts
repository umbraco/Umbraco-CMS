import { UmbDashboardRedirectManagementElement } from './dashboard-redirect-management.element.js';
import { expect, fixture, html } from '@open-wc/testing';

import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbDashboardRedirectManagement', () => {
	let element: UmbDashboardRedirectManagementElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-dashboard-redirect-management></umb-dashboard-redirect-management>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbDashboardRedirectManagementElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).to.be.accessible(defaultA11yConfig);
		});
	}
});
